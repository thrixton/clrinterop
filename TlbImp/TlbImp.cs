///////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation. All rights reserved.
///////////////////////////////////////////////////////////////////////////////
//
// Type Library Importer utility
//
// This program imports all the types in the type library into a interop assembly
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Resources;
using System.Collections;
using System.Runtime.Remoting;
using System.Globalization;
using System.Threading;
using TlbImpCode;
using FormattedOutput;
using System.Security;

namespace tlbimp2
{

internal class TlbImp
{
    private const int SuccessReturnCode = 0;
    private const int ErrorReturnCode = 100;
    private const int MAX_PATH = 260;

    public static int Main(String []aArgs)
    {
        int retCode = SuccessReturnCode;
        try
        {
            try
            {
                SetConsoleUI();

                // Parse the command line arguments.
                if (!ParseArguments(aArgs, ref s_Options, ref retCode))
                    return retCode;

                PrintLogo();

                retCode = Run();
            }
            catch (TlbImpGeneralException tge)
            {
                if (tge.NeedToPrintLogo)
                    PrintLogo();

                Output.WriteTlbimpGeneralException(tge);
                retCode = ErrorReturnCode;
            }
            catch (COMException ex)
            {
                if ((uint)ex.ErrorCode == HResults.TYPE_E_CANTLOADLIBRARY)
                {
                    // Give a more specific message
                    Output.WriteError(Resource.FormatString("Err_RefTlbCantLoad"), ErrorCode.Err_RefTlbCantLoad);
                }
                else
                {
                    // TlbImp COM exception
                    string msg = Resource.FormatString(
                        "Err_UnexpectedException",
                        ex.GetType().ToString(),
                        ex.Message
                        );
                    Output.WriteError(msg, ErrorCode.Err_UnexpectedException);
                }

                retCode = ErrorReturnCode;
            }
            catch (TlbImpInvalidTypeConversionException ex)
            {
                // This usually means that a type conversion has failed outside normal conversion process...
                string name = null;
                try
                {
                    name = ex.Type.GetDocumentation();
                }
                catch (Exception)
                {
                }

                if (name != null)
                    Output.WriteError(Resource.FormatString("Err_FatalErrorInConversion_Named", name), ErrorCode.Err_FatalErrorInConversion_Named);
                else
                    Output.WriteError(Resource.FormatString("Err_FatalErrorInConversion_Unnamed"), ErrorCode.Err_FatalErrorInConversion_Unnamed);

                return ErrorReturnCode;
            }
            catch (SecurityException ex)
            {
                // Only treat SecurityException with PermissionType != null as permission issue
                if (ex.PermissionType == null)
                {
                    string msg = Resource.FormatString(
                        "Err_UnexpectedException",
                        ex.GetType().ToString(),
                        ex.Message
                        );
                    Output.WriteError(msg, ErrorCode.Err_UnexpectedException);
                }
                else
                {
                    Output.WriteError(Resource.GetString("Err_PermissionException"), ErrorCode.Err_PermissionException);
                }

                retCode = ErrorReturnCode;
            }
            catch (Exception ex)
            {
                string msg = Resource.FormatString(
                    "Err_UnexpectedException",
                    ex.GetType().ToString(),
                    ex.Message
                    );
                Output.WriteError(msg, ErrorCode.Err_UnexpectedException);

                retCode = ErrorReturnCode;
            }
        }
        catch (TlbImpResourceNotFoundException ex)
        {
            Output.WriteError(ex.Message, ErrorCode.Err_ResourceNotFound);
            retCode = ErrorReturnCode;
        }

        return retCode;
    }

    public static int Run()
    {
        int RetCode = SuccessReturnCode;

        string TypeLibName = s_Options.m_strTypeLibName;
        s_Options.m_strTypeLibName = GetFullPath(s_Options.m_strTypeLibName, true);
        if (s_Options.m_strTypeLibName == null)
        {
            // We failed to find the typelib. This might be because a resource ID is specified
            // so let's have LoadTypeLibEx try to load it but remember that we failed to find it.
            s_Options.m_bSearchPathSucceeded = false;
            s_Options.m_strTypeLibName = TypeLibName;
        }
        else
        {
            // We found the typelib.
            s_Options.m_bSearchPathSucceeded = true;
        }

        // Retrieve the full path name of the output file.
        if ("".Equals(Path.GetExtension(s_Options.m_strAssemblyName)))
        {
            s_Options.m_strAssemblyName = s_Options.m_strAssemblyName + ".dll";
        }
        if (s_Options.m_strAssemblyName != null)
        {
            try
            {
                s_Options.m_strAssemblyName = (new FileInfo(s_Options.m_strAssemblyName)).FullName;
            }
            catch (System.IO.PathTooLongException)
            {
                throw new TlbImpGeneralException(Resource.FormatString("Err_OutputFileNameTooLong", s_Options.m_strAssemblyName), ErrorCode.Err_OutputFileNameTooLong);
            }

            if (Directory.Exists(s_Options.m_strAssemblyName))
                throw new TlbImpGeneralException(Resource.FormatString("Err_OutputCannotBeDirectory"), ErrorCode.Err_OutputCannotBeDirectory);
        }

        // Determine the output directory for the generated assembly.
        if (s_Options.m_strAssemblyName != null)
        {
            // An output file has been provided so use its directory as the output directory.
            s_Options.m_strOutputDir = Path.GetDirectoryName(s_Options.m_strAssemblyName);
        }
        else
        {
            // No output file has been provided so use the current directory as the output directory.
            s_Options.m_strOutputDir = Environment.CurrentDirectory;
        }

        if (!Directory.Exists(s_Options.m_strOutputDir))
        {
            try
            {
                Directory.CreateDirectory(s_Options.m_strOutputDir);
            }
            catch (System.IO.IOException)
            {
                throw new TlbImpGeneralException(Resource.FormatString("Err_InvalidOutputDirectory"), ErrorCode.Err_InvalidOutputDirectory);
            }
        }

        // If the output directory is different from the current directory then change to that directory.
        if (String.Compare(s_Options.m_strOutputDir, Environment.CurrentDirectory, true, CultureInfo.InvariantCulture) != 0)
            Environment.CurrentDirectory = s_Options.m_strOutputDir;

        // TlbImp uses ReflectionOnly loading.
        s_Options.m_flags |= TypeLibImporterFlags.ReflectionOnlyLoading;

        // Create an AppDomain to load the implementation part of the app into.
        AppDomainSetup options = new AppDomainSetup();
        options.ApplicationBase = s_Options.m_strOutputDir;
        AppDomain domain = AppDomain.CreateDomain("TlbImp", null, options);
        if (domain == null)
            throw new TlbImpGeneralException(Resource.FormatString("Err_CannotCreateAppDomain"), ErrorCode.Err_CannotCreateAppDomain);

        // Create the remote component that will perform the rest of the conversion.
        ObjectHandle h = domain.CreateInstanceFrom(typeof(TlbImpCode.RemoteTlbImp).Assembly.CodeBase,
                                                   "TlbImpCode.RemoteTlbImp");
        if (h == null)
            throw new TlbImpGeneralException(Resource.FormatString("Err_CannotCreateRemoteTlbImp"), ErrorCode.Err_CannotCreateRemoteTlbImp);

        // Have the remote component perform the rest of the conversion.
        RemoteTlbImp code = (RemoteTlbImp)h.Unwrap();

        // We just can't pass the TlbImpOptions class, because remoting won't be able
        // to resolve the object when it re-emerges in the other AppDomain, because the
        // other AppDomain doesn't know anything about the executing assembly (where 
        // TlbImpOptions is defined)
        if (code != null)
            RetCode = code.Run(s_Options.m_strTypeLibName,
                               s_Options.m_strAssemblyName,
                               s_Options.m_strAssemblyNamespace,
                               s_Options.m_strOutputDir,
                               s_Options.m_aPublicKey,
                               s_Options.m_sKeyPair,
                               s_Options.m_strAssemblyRefList,
                               s_Options.m_strTypeLibRefList,
                               s_Options.m_AssemblyVersion,
                               s_Options.m_flags,
                               s_Options.m_bNoLogo,
                               s_Options.m_bSilentMode,
                               s_Options.m_silenceList,
                               s_Options.m_bVerboseMode,
                               s_Options.m_bStrictRef,
                               s_Options.m_bStrictRefNoPia,
                               s_Options.m_bSearchPathSucceeded,
                               s_Options.m_strProduct,
                               s_Options.m_strProductVersion,
                               s_Options.m_strCompany,
                               s_Options.m_strCopyright,
                               s_Options.m_strTrademark,
                               s_Options.m_isVersion2,
                               s_Options.m_isPreserveSig,
                               s_Options.m_isRemoveEnumPrefix);

        // Unload the app domain now that we've finished the import.
        AppDomain.Unload(domain);
        
        return RetCode;
    }

    private static void SetConsoleUI()
    {
        Thread t = Thread.CurrentThread;
        
        t.CurrentUICulture = CultureInfo.CurrentUICulture.GetConsoleFallbackUICulture();

        if (Environment.OSVersion.Platform != PlatformID.Win32Windows)
        {        
            if ( (System.Console.OutputEncoding.CodePage != t.CurrentUICulture.TextInfo.OEMCodePage) &&
                 (System.Console.OutputEncoding.CodePage != t.CurrentUICulture.TextInfo.ANSICodePage))
            {
                t.CurrentUICulture = new CultureInfo("en-US");
            }
        }
    }

    private static bool ParseArguments(String []aArgs, ref TlbImpOptions Options, ref int ReturnCode)
    {
        CommandLine cmdLine;
        Option opt;
        bool delaysign = false;

        // Create the options object that will be returned.
        Options = new TlbImpOptions();

        // Parse the command line arguments using the command line argument parser.
        cmdLine = new CommandLine(aArgs, new String[] { "*out", "*publickey", "*keyfile", "*keycontainer", "delaysign", "*reference",
                                                        "unsafe", "nologo", "silent", "verbose", "+strictref", "primary", "*namespace", 
                                                        "*asmversion", "sysarray", "*transform", "?", "help", "*tlbreference",
                                                        "noclassmembers", "*machine", "*silence", "*product", "*productversion", 
                                                        "*company", "*copyright", "*trademark", "v2", "preservesig", "removeenumprefix" });

        // Make sure there is at least one argument.
        if ((cmdLine.NumArgs + cmdLine.NumOpts) < 1)
        {
            PrintUsage();
            ReturnCode = SuccessReturnCode;
            return false;
        }

        // Get the name of the COM typelib.
        Options.m_strTypeLibName = cmdLine.GetNextArg();

        // Go through the list of options.
        while ((opt = cmdLine.GetNextOption()) != null)
        {
            // Determine which option was specified.
            if (opt.Name.Equals("out"))
            {
                Options.m_strAssemblyName = opt.Value;
            }
            else if (opt.Name.Equals("namespace"))
            {
                Options.m_strAssemblyNamespace = opt.Value;
            }
            else if (opt.Name.Equals("asmversion"))
            {
                try
                {
                    Options.m_AssemblyVersion = new Version(opt.Value);
                }
                catch(Exception)
                {
                    throw new TlbImpGeneralException(Resource.FormatString("Err_InvalidVersion"), ErrorCode.Err_InvalidVersion, true);
                }
            }
            else if (opt.Name.Equals("reference"))
            {
                String FullPath = null;
                
                FullPath = GetFullPath(opt.Value, false);
                
                if (FullPath == null)
                {
                    ReturnCode = ErrorReturnCode;
                    return false;
                }
                
                if (Options.m_strAssemblyRefList == null)
                    Options.m_strAssemblyRefList = FullPath;
                else
                    Options.m_strAssemblyRefList = Options.m_strAssemblyRefList + ";" + FullPath;
            }
            else if (opt.Name.Equals("tlbreference"))
            {
                String FullPath = null;
                
                FullPath = GetFullPath(opt.Value, false);
                if (FullPath == null)
                {
                    ReturnCode = ErrorReturnCode;
                    return false;
                }
                
                if (Options.m_strTypeLibRefList == null)
                    Options.m_strTypeLibRefList = FullPath;
                else
                    Options.m_strTypeLibRefList = Options.m_strTypeLibRefList + ";" + FullPath;
            }
            else if (opt.Name.Equals("delaysign"))
            {
                delaysign = true;
            }
            else if (opt.Name.Equals("publickey"))
            {
                if (Options.m_sKeyPair != null || Options.m_aPublicKey != null)
                {
                    throw new TlbImpGeneralException(Resource.FormatString("Err_TooManyKeys"), ErrorCode.Err_TooManyKeys, true);
                }
                // Read data from binary file into byte array.
                byte[] aData;
                FileStream fs = null;
                try
                {
                    fs = new FileStream(opt.Value, FileMode.Open, FileAccess.Read, FileShare.Read);
                    int iLength = (int)fs.Length;
                    aData = new byte[iLength];
                    fs.Read(aData, 0, iLength);
                }
                catch (Exception ex)
                {
                    throw new TlbImpGeneralException(Resource.FormatString("Err_ErrorWhileOpenningFile", new object[] { opt.Value, ex.GetType().ToString(), ex.Message }), ErrorCode.Err_ErrorWhileOpenningFile, true);
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                }
                Options.m_aPublicKey = aData;
            }
            else if (opt.Name.Equals("keyfile"))
            {
                if (Options.m_sKeyPair != null || Options.m_aPublicKey != null)
                {
                    throw new TlbImpGeneralException(Resource.FormatString("Err_TooManyKeys"), ErrorCode.Err_TooManyKeys, true);
                }
                
                // Read data from binary file into byte array.
                byte[] aData;
                FileStream fs = null;
                try
                {
                    fs = new FileStream(opt.Value, FileMode.Open, FileAccess.Read, FileShare.Read);
                    int iLength = (int)fs.Length;
                    aData = new byte[iLength];
                    fs.Read(aData, 0, iLength);
                }
                catch (Exception ex)
                {
                    throw new TlbImpGeneralException(Resource.FormatString("Err_ErrorWhileOpenningFile", new object[] { opt.Value, ex.GetType().ToString(), ex.Message }), ErrorCode.Err_ErrorWhileOpenningFile, true);
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                }
                Options.m_sKeyPair = new StrongNameKeyPair(aData);
            }
            else if (opt.Name.Equals("keycontainer"))
            {
                if ((Options.m_sKeyPair != null) || (Options.m_aPublicKey != null))
                {
                    throw new TlbImpGeneralException(Resource.FormatString("Err_TooManyKeys"), ErrorCode.Err_TooManyKeys, true);
                }
                Options.m_sKeyPair = new StrongNameKeyPair(opt.Value);
            }
            else if (opt.Name.Equals("unsafe"))
            {
                Options.m_flags |= TypeLibImporterFlags.UnsafeInterfaces;
            }
            else if (opt.Name.Equals("primary"))
            {
                Options.m_flags |= TypeLibImporterFlags.PrimaryInteropAssembly;
            }
            else if (opt.Name.Equals("sysarray"))
            {
                Options.m_flags |= TypeLibImporterFlags.SafeArrayAsSystemArray;
            }
            else if (opt.Name.Equals("nologo"))
            {
                Options.m_bNoLogo = true;
            }
            else if (opt.Name.Equals("silent"))
            {
                Output.SetSilent(true);
                Options.m_bSilentMode = true;
            }
            else if (opt.Name.Equals("silence"))
            {
                int warningNumber = int.Parse(opt.Value, System.Globalization.NumberStyles.HexNumber);
                Output.Silence(warningNumber);
                Options.m_silenceList.Add(warningNumber);
            }
            else if (opt.Name.Equals("verbose"))
            {
                Options.m_bVerboseMode = true;
            }
            else if (opt.Name.Equals("noclassmembers"))
            {
                Options.m_flags |= TypeLibImporterFlags.PreventClassMembers;
            }
            else if (opt.Name.Equals("strictref"))
            {
                if (opt.Value != null)
                {
                    if (String.Compare(opt.Value, "nopia", true) == 0)
                    {
                        Options.m_bStrictRefNoPia = true;
                    }
                    else
                    {
                        throw new TlbImpGeneralException(Resource.FormatString("Err_UnknownStrictRefOpt", opt.Value), ErrorCode.Err_UnknownStrictRefOpt, true);
                    }
                }
                else
                    Options.m_bStrictRef = true;
            }
            else if (opt.Name.Equals("transform"))
            {
                if (opt.Value.ToLower(CultureInfo.InvariantCulture) == "dispret")
                {
                    Options.m_flags |= TypeLibImporterFlags.TransformDispRetVals;
                }
                else if (opt.Value.ToLower(CultureInfo.InvariantCulture) == "serializablevalueclasses")
                {
                    Options.m_flags |= TypeLibImporterFlags.SerializableValueClasses;
                }
                else
                {
                    throw new TlbImpGeneralException(Resource.FormatString("Err_InvalidTransform", opt.Value), ErrorCode.Err_InvalidTransform, true);
                }
            }
            else if (opt.Name.Equals("machine"))
            {
                if (opt.Value.ToLower(CultureInfo.InvariantCulture) == "itanium")
                {
                    Options.m_flags |= TypeLibImporterFlags.ImportAsItanium;
                }
                else if (opt.Value.ToLower(CultureInfo.InvariantCulture) == "x64")
                {
                    Options.m_flags |= TypeLibImporterFlags.ImportAsX64;
                }
                else if (opt.Value.ToLower(CultureInfo.InvariantCulture) == "x86")
                {
                    Options.m_flags |= TypeLibImporterFlags.ImportAsX86;
                }
                else if (opt.Value.ToLower(CultureInfo.InvariantCulture) == "agnostic")
                {
                    Options.m_flags |= TypeLibImporterFlags.ImportAsAgnostic;
                }
                else
                {
                    throw new TlbImpGeneralException(Resource.FormatString("Err_InvalidMachine", opt.Value), ErrorCode.Err_InvalidMachine, true);
                }
            }
            else if (opt.Name.Equals("product"))
            {
                Options.m_strProduct = opt.Value;
            }
            else if (opt.Name.Equals("productversion"))
            {
                Options.m_strProductVersion = opt.Value;
            }
            else if (opt.Name.Equals("company"))
            {
                Options.m_strCompany = opt.Value;
            }
            else if (opt.Name.Equals("copyright"))
            {
                Options.m_strCopyright = opt.Value;
            }
            else if (opt.Name.Equals("trademark"))
            {
                Options.m_strTrademark = opt.Value;
            }
            else if (opt.Name.Equals("?") || opt.Name.Equals("help"))
            {
                PrintUsage();
                ReturnCode = SuccessReturnCode;
                return false;
            }
            else if (opt.Name.Equals("v2"))
            {
                Options.m_isVersion2 = true;
            }
            else if (opt.Name.Equals("preservesig"))
            {
                Options.m_isPreserveSig = true;
            }
            else if (opt.Name.Equals("removeenumprefix"))
            {
                Options.m_isRemoveEnumPrefix = true;
            }
        }

        // Validate that the typelib name has been specified.
        if (Options.m_strTypeLibName == null)
        {
            throw new TlbImpGeneralException(Resource.FormatString("Err_NoInputFile"), ErrorCode.Err_NoInputFile, true);
        }

        // Gather information needed for strong naming the assembly (if
        // the user desires this).
        if ((Options.m_sKeyPair != null) && (Options.m_aPublicKey == null))
        {
            try
            {
                Options.m_aPublicKey = Options.m_sKeyPair.PublicKey;
            }
            catch
            {
                throw new TlbImpGeneralException(Resource.FormatString("Err_InvalidStrongName"), ErrorCode.Err_InvalidStrongName, true);
            }
        }

        if (delaysign && Options.m_sKeyPair != null)
            Options.m_sKeyPair = null;

        // To be able to generate a PIA, we must also be strong naming the assembly.
        if ((Options.m_flags & TypeLibImporterFlags.PrimaryInteropAssembly) != 0)
        {
            if (Options.m_aPublicKey == null && Options.m_sKeyPair == null)
            {
                throw new TlbImpGeneralException(Resource.FormatString("Err_PIAMustBeStrongNamed"), ErrorCode.Err_PIAMustBeStrongNamed, true);
            }
        }

        return true;
    }
    
    private static string GetFullPath(String FileName, bool fInputFile)
    {
        String fullpath = null;
        
        // Try resolving the partial path (or if we just got a filename, the current path)
        //  to a full path and check for the file.
        fullpath = (new FileInfo(Path.GetFullPath(FileName))).FullName;
        if (!File.Exists(fullpath))
        {
            // Next, call SearchPath to find the full path of the typelib to load.
            StringBuilder sb = new StringBuilder(MAX_PATH + 1);
            if (SearchPath(null, FileName, null, sb.Capacity + 1, sb, null) == 0)
            {
                fullpath = null;
                
                if (!fInputFile)
                    throw new TlbImpGeneralException(Resource.FormatString("Err_ReferenceNotFound", FileName), ErrorCode.Err_ReferenceNotFound, true);
            }
            else
            {
                fullpath = (new FileInfo(sb.ToString())).FullName;
            }
        }

        if ((fullpath != null) && (s_Options.m_bVerboseMode == true))
        {
            Output.WriteInfo(Resource.FormatString("Msg_ResolvedFile", FileName, fullpath), MessageCode.Msg_ResolvedFile);
        }
        
        return fullpath;
    }

    private static void PrintLogo()
    {
        if (!s_Options.m_bNoLogo)
        {
            Output.Write(Resource.FormatString("Msg_Copyright", Assembly.GetExecutingAssembly().ImageRuntimeVersion));
        }
    }

    private static void PrintUsage()
    {
        PrintLogo();

        string resNameBase = "Msg_Usage_";
        string outputStr = "temp";
        string resName;
        int index = 0;

        while (outputStr != null)
        {
            if (index < 10)
                resName = resNameBase + "0" + index;
            else
                resName = resNameBase + index;

            outputStr = Resource.GetStringIfExists(resName);

            if (outputStr != null)
                Output.Write(outputStr);
            
            index++;
        }
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int SearchPath(String path, String fileName, String extension, int numBufferChars, StringBuilder buffer, int[] filePart);

    internal static TlbImpOptions s_Options = null;
}

}
