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
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Text;
using System.Globalization;
using FormattedOutput;
using tlbimp2;
using System.Security;

using _TYPELIBATTR = System.Runtime.InteropServices.ComTypes.TYPELIBATTR;
using _SYSKIND = System.Runtime.InteropServices.ComTypes.SYSKIND;

namespace TlbImpCode {

//******************************************************************************
// Enum passed in to LoadTypeLibEx.
//******************************************************************************
[Flags]
internal enum REGKIND
{
    REGKIND_DEFAULT         = 0,
    REGKIND_REGISTER        = 1,
    REGKIND_NONE            = 2,
    REGKIND_LOAD_TLB_AS_32BIT = 0x20,
    REGKIND_LOAD_TLB_AS_64BIT = 0x40,
}

internal class TypeLibInfo
{
    public string name;
    public Guid guid;
    public ushort majorVersion;
    public ushort minorVersion;
}

//******************************************************************************
// The typelib importer implementation.
//******************************************************************************
internal class TlbImpCode
{
    private const int SuccessReturnCode = 0;
    private const int ErrorReturnCode = 100;
    private const int MAX_PATH = 260;
    private static REGKIND s_RK;
    private static TypeLibInfo s_MissingTypeLibInfo;            // Saves the info for missing type library

    //**************************************************************************
    // Entry point called on the typelib importer in the proper app domain.
    //**************************************************************************
    public static int Run(TlbImpOptions options)
    {
        s_Options = options;

        Output.SetSilent(options.m_bSilentMode);
        Output.Silence(options.m_silenceList);

        System.Runtime.InteropServices.ComTypes.ITypeLib TypeLib = null;
        String strPIAName = null;
        String strPIACodeBase = null;

        s_RK = REGKIND.REGKIND_NONE;

        if (Environment.OSVersion.Platform != PlatformID.Win32Windows)
        {
            if (IsImportingToItanium(options.m_flags) || IsImportingToX64(options.m_flags))
            {
                s_RK |= REGKIND.REGKIND_LOAD_TLB_AS_64BIT;
            }
            else if (IsImportingToX86(options.m_flags))
            {
                s_RK |= REGKIND.REGKIND_LOAD_TLB_AS_32BIT;
            }
        }

        //----------------------------------------------------------------------
        // Load the typelib.
        try
        {           
            LoadTypeLibEx(s_Options.m_strTypeLibName, s_RK, out TypeLib);
            s_RefTypeLibraries.Add(s_Options.m_strTypeLibName, TypeLib);
        }
        catch (COMException e)
        {
            if (!s_Options.m_bSearchPathSucceeded)
            {
                // We failed to search for the typelib and we failed to load it.
                // This means that the input typelib is not available.
                Output.WriteError(Resource.FormatString("Err_InputFileNotFound", s_Options.m_strTypeLibName), ErrorCode.Err_InputFileNotFound);
            }
            else
            {
                if (e.ErrorCode == unchecked((int)0x80029C4A))
                {
                    Output.WriteError(Resource.FormatString("Err_InputFileNotValidTypeLib", s_Options.m_strTypeLibName), ErrorCode.Err_InputFileNotValidTypeLib);
                }
                else
                {
                    Output.WriteError(Resource.FormatString("Err_TypeLibLoad", e), ErrorCode.Err_TypeLibLoad);
                }
            }
            return ErrorReturnCode;
        }
        catch (Exception e)
        {
            Output.WriteError(Resource.FormatString("Err_TypeLibLoad", e), ErrorCode.Err_TypeLibLoad);
            return ErrorReturnCode;
        }

        //----------------------------------------------------------------------
        // Check to see if there already exists a primary interop assembly for 
        // this typelib.

        if (TlbImpCode.GetPrimaryInteropAssembly(TypeLib, out strPIAName, out strPIACodeBase))
        {
            Output.WriteWarning(Resource.FormatString("Wrn_PIARegisteredForTlb", strPIAName, s_Options.m_strTypeLibName), WarningCode.Wrn_PIARegisteredForTlb);
        }

        //----------------------------------------------------------------------
        // Retrieve the name of output assembly if it was not explicitly set.

        if (s_Options.m_strAssemblyName == null)
        {
            s_Options.m_strAssemblyName = Marshal.GetTypeLibName(TypeLib) + ".dll";
        }

        //----------------------------------------------------------------------
        // Do some verification on the output assembly.

        String strFileNameNoPath = Path.GetFileName(s_Options.m_strAssemblyName);
        String strExtension = Path.GetExtension(s_Options.m_strAssemblyName);   

        // Validate that the extension is valid.
        bool bExtensionValid = ".dll".Equals(strExtension.ToLower(CultureInfo.InvariantCulture));

        // If the extension is not valid then tell the user and quit.
        if (!bExtensionValid)
        {
            Output.WriteError(Resource.FormatString("Err_InvalidExtension"), ErrorCode.Err_InvalidExtension);
            return ErrorReturnCode;
        }

        // Make sure the output file will not overwrite the input file.
        String strInputFilePath = (new FileInfo(s_Options.m_strTypeLibName)).FullName.ToLower(CultureInfo.InvariantCulture);
        String strOutputFilePath;
        try
        {
            strOutputFilePath = (new FileInfo(s_Options.m_strAssemblyName)).FullName.ToLower(CultureInfo.InvariantCulture);
        }
        catch (System.IO.PathTooLongException)
        {
            Output.WriteError(Resource.FormatString("Err_OutputFileNameTooLong", s_Options.m_strAssemblyName), ErrorCode.Err_OutputFileNameTooLong);
            return ErrorReturnCode;
        }

        if (strInputFilePath.Equals(strOutputFilePath))
        {
            Output.WriteError(Resource.FormatString("Err_OutputWouldOverwriteInput"), ErrorCode.Err_OutputWouldOverwriteInput);
            return ErrorReturnCode;
        }

        //-------------------------------------------------------------------------
        // Load all assemblies provided as explicit references on the command line.
        if (s_Options.m_strAssemblyRefList != null)
        {
            String[] asmPaths = s_Options.m_strAssemblyRefList.Split(';');

            foreach (String asmPath in asmPaths)
            {
                if (!LoadAssemblyRef(asmPath))
                    return ErrorReturnCode;
            }
        }

        //-------------------------------------------------------------------------
        // And the same for type library references.
        if (s_Options.m_strTypeLibRefList != null)
        {
            String[] tlbPaths = s_Options.m_strTypeLibRefList.Split(';');

            foreach (String tlbPath in tlbPaths)
            {
                if (!LoadTypeLibRef(tlbPath))
                    return ErrorReturnCode;
            }
        }

        //-------------------------------------------------------------------------
        // Before we attempt the import, verify the references first
        if (!VerifyTypeLibReferences(s_Options.m_strTypeLibName))
            return ErrorReturnCode;

        //----------------------------------------------------------------------
        // Attempt the import.

        try
        {
            try
            {
                // Import the typelib to an assembly.
                AssemblyBuilder AsmBldr = DoImport(TypeLib, s_Options.m_strAssemblyName, s_Options.m_strAssemblyNamespace,
                    s_Options.m_AssemblyVersion, s_Options.m_aPublicKey, s_Options.m_sKeyPair, s_Options.m_strProduct,
                    s_Options.m_strProductVersion, s_Options.m_strCompany, s_Options.m_strCopyright, s_Options.m_strTrademark,
                    s_Options.m_flags, s_Options.m_isVersion2, s_Options.m_isPreserveSig, s_Options.m_isRemoveEnumPrefix);
                if (AsmBldr == null)
                    return ErrorReturnCode;
            }
            catch (TlbImpResolveRefFailWrapperException ex)
            {
                // Throw out the inner exception instead
                throw ex.InnerException;
            }
        }
        catch (ReflectionTypeLoadException e)
        {
            int i;
            Exception[] exceptions;
            Output.WriteError(Resource.FormatString("Err_TypeLoadExceptions"), ErrorCode.Err_TypeLoadExceptions);
            exceptions = e.LoaderExceptions;
            for (i = 0; i < exceptions.Length; i++)
            {
                try
                {
                    Output.WriteInfo(Resource.FormatString("Msg_DisplayException", new object[] { i, exceptions[i].GetType().ToString(), exceptions[i].Message }), MessageCode.Msg_DisplayException);
                }
                catch (Exception ex)
                {
                    Output.WriteInfo(Resource.FormatString("Msg_DisplayNestedException", new object [] { i, ex.GetType().ToString(), ex.Message }), MessageCode.Msg_DisplayNestedException);
                }
            }
            return ErrorReturnCode;
        }
        catch (TlbImpGeneralException tge)
        {
            Output.WriteTlbimpGeneralException(tge);
            return ErrorReturnCode;
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

            return ErrorReturnCode;
        }
        catch (TlbImpInvalidTypeConversionException ex)
        {
            // This usually means that a type conversion has failed outside normal conversion process...
            string name = null;
            try
            {
                name = ex.Type.GetDocumentation();
            }
            catch(Exception)
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

            return ErrorReturnCode;
        }
        catch (Exception ex)
        {
            string msg = Resource.FormatString(
                "Err_UnexpectedException",
                ex.GetType().ToString(),
                ex.Message
                );
            Output.WriteError(msg, ErrorCode.Err_UnexpectedException);

            return ErrorReturnCode;
        }

        Output.WriteInfo(Resource.FormatString("Msg_TypeLibImported", s_Options.m_strAssemblyName), MessageCode.Msg_TypeLibImported);

        return SuccessReturnCode;
    }

    /// <summary>
    /// Try to load the type library and verify guid/version
    /// </summary>
    /// <returns>HRESULT. >=0 if succeeds, otherwise failed</returns>
    private static int TryLoadTypeLib(string pathName, string simpleName, Guid tlbId, ushort majorVersion, ushort minorVersion)
    {
        int hr;

        tlbimp2.ITypeLib typeLib;
        hr = TypeLib.LoadTypeLib(pathName, out typeLib);
        if (hr >= 0)
        {
            s_RefTypeLibraries.Add(pathName, typeLib as System.Runtime.InteropServices.ComTypes.ITypeLib);

            TypeLib refTypeLib = new TypeLib(typeLib);
            TypeLibAttr libAttr = refTypeLib.GetLibAttr();
            if (libAttr.guid == tlbId && libAttr.wMajorVerNum == majorVersion && libAttr.wMinorVerNum == minorVersion)
            {
                if (TlbImpCode.s_Options.m_bVerboseMode)
                {
                    Output.WriteInfo(
                        Resource.FormatString("Msg_TypeLibRefResolved",
                            new object[] { simpleName, majorVersion.ToString() + "." + minorVersion, tlbId.ToString(), pathName }),
                        MessageCode.Msg_TypeLibRefResolved);
                }

                return 0;
            }
            else
            {
                if (TlbImpCode.s_Options.m_bVerboseMode)
                {
                    Output.WriteInfo(
                        Resource.FormatString("Msg_TypeLibRefMismatch",
                            new object[] { 
                                        simpleName, majorVersion.ToString() + "." + minorVersion, tlbId.ToString(), 
                                        simpleName, libAttr.wMajorVerNum.ToString() + "." + libAttr.wMajorVerNum.ToString(), libAttr.guid,
                                        pathName }),
                        MessageCode.Msg_TypeLibRefMismatch);
                }

                return -1;
            }
        }

        return hr;
    }

    /// <summary>
    /// Used to receive ITypeLibResolver.ResolveTypeLib callback and try to resolve the type library using provided information
    /// </summary>
    private class TypeLibResolverHelper : ITypeLibResolver
    {
        #region ITypeLibResolver Members

        public int ResolveTypeLib(string simpleName, Guid tlbId, int lcid, ushort majorVersion, ushort minorVersion, tlbimp2.SYSKIND syskind, out string bstrPathName)
        {
            // Remember the type lib info 
            s_MissingTypeLibInfo = new TypeLibInfo();
            s_MissingTypeLibInfo.name = simpleName;
            s_MissingTypeLibInfo.guid = tlbId;
            s_MissingTypeLibInfo.majorVersion = majorVersion;
            s_MissingTypeLibInfo.minorVersion = minorVersion;

            //
            // Find this type library in list of referenced type libraries
            //
            foreach (string pathName in s_RefTypeLibraries.Keys)
            {
                TypeLib refTypeLib = new TypeLib(s_RefTypeLibraries[pathName] as tlbimp2.ITypeLib);
                TypeLibAttr libAttr = refTypeLib.GetLibAttr();
                if (libAttr.guid == tlbId)
                {
                    if (libAttr.wMajorVerNum == majorVersion && libAttr.wMinorVerNum == minorVersion)
                    {
                        // Resolved to a matching type lib
                        bstrPathName = pathName;
                        return 0;
                    }
                }
            }

            //
            // Find using GUID
            //
            int hr = TypeLib.QueryPathOfRegTypeLib(ref tlbId, majorVersion, minorVersion, lcid, out bstrPathName);
            if (hr >= 0)
            {
                // Try loading the type library and verify guid/version
                hr = TryLoadTypeLib(bstrPathName, simpleName, tlbId, majorVersion, minorVersion);
                if (hr >= 0) return hr;
            }

            //
            // Try to load current directory
            //
            bstrPathName = Path.Combine(Directory.GetCurrentDirectory(), simpleName);
            if (File.Exists(bstrPathName))
            {
                // Try loading the type library guid/version
                hr = TryLoadTypeLib(bstrPathName, simpleName, tlbId, majorVersion, minorVersion);
                if (hr >= 0) return hr;
            }

            if (TlbImpCode.s_Options.m_bVerboseMode)
            {
                Output.WriteInfo(
                    Resource.FormatString("Msg_TypeLibRefResolveFailed",
                        new object[] { simpleName, majorVersion.ToString() + "." + minorVersion, tlbId.ToString() }),
                    MessageCode.Msg_TypeLibRefResolveFailed);
            }

            return -1;
        }

        #endregion
    }

    /// <summary>
    /// Callback used in LoadTypeLibWithResolver
    /// </summary>
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ITypeLibResolver
    {
        [PreserveSig]
        int ResolveTypeLib(string simpleName, Guid tlbId, int lcid, ushort majorVersion, ushort minorVersion, tlbimp2.SYSKIND syskind, out string bstrName);
    }

    [DllImport("tlbref.dll")]
    private static extern int LoadTypeLibWithResolver([MarshalAs(UnmanagedType.LPWStr)] string file, REGKIND kind, ITypeLibResolver resolver, out tlbimp2.ITypeLib typeLib);

    /// <summary>
    /// Verify that whether we can resolve all type library references.
    /// </summary>
    /// <returns>true if succeed. false if failed</returns>
    private static bool VerifyTypeLibReferences(string tlbFileName)
    {
        // For now, we do the check only in verbose mode to minimize the potential impact
        if (!TlbImpCode.s_Options.m_bVerboseMode)
            return true;

        tlbimp2.ITypeLib typeLib;
        int hr = LoadTypeLibWithResolver(tlbFileName, s_RK, new TypeLibResolverHelper(), out typeLib);
        
        // Disable the check because the API is too strict with minor versions
        //if (hr < 0)
        //{
        //    // The API might have failed without telling us which type library is missing... ignore it
        //    // Otherwise, tell user which type library is missing
        //    if (s_MissingTypeLibInfo != null)
        //    {
        //        // We failed to find in the current directory. Even though type lib load API might be able to resolve it, let's still fail
        //        // and have user specify it
        //        Output.WriteError(
        //            Resource.FormatString("Err_RefTypeLibMissing",
        //                new object[] { s_MissingTypeLibInfo.name, s_MissingTypeLibInfo.majorVersion.ToString() + "." + s_MissingTypeLibInfo.minorVersion, s_MissingTypeLibInfo.guid.ToString() }),
        //            ErrorCode.Err_RefTypeLibMissing);
        //    }

        //    return false;
        //}

        return true;
    }

    //**************************************************************************
    // Load an assembly reference specified on the command line.
    //**************************************************************************
    private static bool LoadAssemblyRef(String path)
    {
        Assembly asm = null;

        // We're guaranteed to have a fully qualified path at this point.              
        try
        {
            // Load the assembly.
            asm = Assembly.ReflectionOnlyLoadFrom(path);
                    
            // Retrieve the GUID and add the assembly to the hashtable of referenced assemblies.
            Guid TypeLibId = Marshal.GetTypeLibGuidForAssembly(asm);
                    
            // Add the assembly to the list of referenced assemblies if it isn't already present.
            if (s_AssemblyRefs.Contains(TypeLibId))
            {
                // If this is the same assembly and same version, just return
                if (asm == s_AssemblyRefs[TypeLibId])
                    return true;

                // Otherwise, we have two versions of the same type assembly.
                Output.WriteError(Resource.FormatString("Err_MultipleVersionsOfAssembly", TypeLibId), ErrorCode.Err_MultipleVersionsOfAssembly);
                return false;
            }
            
            s_AssemblyRefs.Add(TypeLibId, asm);
        } 
        catch (BadImageFormatException)
        {
            Output.WriteError(Resource.FormatString("Err_RefAssemblyInvalid", path), ErrorCode.Err_RefAssemblyInvalid);
            return false;
        }
        catch (FileNotFoundException)
        {
            Output.WriteError(Resource.FormatString("Err_RefAssemblyNotFound", path), ErrorCode.Err_RefAssemblyNotFound);
            return false;
        }
        catch (FileLoadException e)
        {
            Output.WriteError(Resource.FormatString("Err_RefAssemblyCantLoad", path), ErrorCode.Err_RefAssemblyCantLoad);
            Output.WriteError(e);
            return false;
        }
        catch (ApplicationException e)
        {
            Output.WriteError(e);
            return false;
        }

        return true;
    }

    //**************************************************************************
    // Load a type library specified on the command line.
    //**************************************************************************
    private static bool LoadTypeLibRef(String path)
    {
        // For now just expect the user to supply a full path to the type
        // library. We can improve this later as need be.
        try
        {
            System.Runtime.InteropServices.ComTypes.ITypeLib typeLib;
            LoadTypeLibEx(path, s_RK, out typeLib);
            s_RefTypeLibraries.Add(path, typeLib);
        }
        catch (COMException e)
        {
            if (e.ErrorCode == unchecked((int)0x80029C4A))
                Output.WriteError(Resource.FormatString("Err_InputFileNotValidTypeLib", path), ErrorCode.Err_InputFileNotValidTypeLib);
            else
                Output.WriteError(Resource.FormatString("Err_TypeLibLoad", path, e), ErrorCode.Err_TypeLibLoad);
            return false;
        }
        catch (Exception e)
        {
            Output.WriteError(Resource.FormatString("Err_TypeLibLoad", e), ErrorCode.Err_TypeLibLoad);
            return false;
        }

        return true;
    }

    //**************************************************************************
    // Static importer function used by main and the callback.
    //**************************************************************************
    public static AssemblyBuilder DoImport(System.Runtime.InteropServices.ComTypes.ITypeLib TypeLib,
                                           String strAssemblyFileName,
                                           String strAssemblyNamespace,
                                           Version asmVersion,
                                           byte[] publicKey,
                                           StrongNameKeyPair keyPair,
                                           String strProduct,
                                           String strProductVersion,
                                           String strCompany,
                                           String strCopyright,
                                           String strTrademark,
                                           TypeLibImporterFlags flags,
                                           bool isVersion2,
                                           bool isPreserveSig,
                                           bool isRemoveEnumPrefix)
    {
        // Detemine the assembly file name.
        String asmFileName = Path.GetFileName(strAssemblyFileName);

        // Add this typelib to list of importing typelibs.
        Guid guid = Marshal.GetTypeLibGuid(TypeLib);
        s_ImportingLibraries.Add(guid.ToString(), guid);

        // If the type library is 64-bit, make sure the user specified a platform type.
        IntPtr pTLibAttr = IntPtr.Zero;
        TypeLib.GetLibAttr(out pTLibAttr);
        _TYPELIBATTR TLibAttr = (_TYPELIBATTR)Marshal.PtrToStructure(pTLibAttr, typeof(_TYPELIBATTR));
        TypeLib.ReleaseTLibAttr(pTLibAttr);

        // Validate the machine options
        if (!ValidateMachineType(flags, TLibAttr.syskind))
            return null;

        // Convert the typelib.
        ImporterCallback callback = new ImporterCallback();
        tlbimp2.Process process = new Process();
        AssemblyBuilder AsmBldr = process.DoProcess(
            TypeLib,
            strAssemblyFileName,
            flags,
            callback,
            publicKey,
            keyPair,
            strAssemblyNamespace,
            asmVersion,
            isVersion2,
            isPreserveSig,
            isRemoveEnumPrefix);

        if (AsmBldr == null) return null;

        // Remove this typelib from list of importing typelibs.
        s_ImportingLibraries.Remove(guid.ToString());

        // Delete the output assembly.
        File.Delete(asmFileName);

        AsmBldr.DefineVersionInfoResource(strProduct, strProductVersion, strCompany, strCopyright, strTrademark);

        if (IsImportingToX64(flags))
        {
            AsmBldr.Save(asmFileName, PortableExecutableKinds.ILOnly | PortableExecutableKinds.PE32Plus, 
                ImageFileMachine.AMD64);
        }
        else if (IsImportingToItanium(flags))
        {
            AsmBldr.Save(asmFileName, PortableExecutableKinds.ILOnly | PortableExecutableKinds.PE32Plus, 
                ImageFileMachine.IA64);
        }
        else if (IsImportingToX86(flags))
        {
            AsmBldr.Save(asmFileName, PortableExecutableKinds.ILOnly | PortableExecutableKinds.Required32Bit, 
                ImageFileMachine.I386);
        }
        else
        {
            // Default is agnostic
            AsmBldr.Save(asmFileName);        
        }

        return AsmBldr;
    }

    //**************************************************************************
    // Helper to get a PIA from a typelib.
    //**************************************************************************
    internal static bool GetPrimaryInteropAssembly(Object TypeLib, out String asmName, out String asmCodeBase)
    {
        IntPtr pAttr = (IntPtr)0;
        _TYPELIBATTR Attr;
        System.Runtime.InteropServices.ComTypes.ITypeLib pTLB = (System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib;
        int Major = 0;
        int Minor = 0;
        Guid TlbId;
        int lcid = 0;

        // Retrieve the major and minor version from the typelib.
        try
        {
            pTLB.GetLibAttr(out pAttr);
            Attr = (_TYPELIBATTR)Marshal.PtrToStructure(pAttr, typeof(_TYPELIBATTR));
            Major = Attr.wMajorVerNum;
            Minor = Attr.wMinorVerNum;
            TlbId = Attr.guid;
            lcid = Attr.lcid;
        }
        finally
        {
            // Release the typelib attributes.
            if (pAttr != (IntPtr)0)
                pTLB.ReleaseTLibAttr(pAttr);
        }

        // Ask the converter for a PIA for this typelib.
        return s_TypeLibConverter.GetPrimaryInteropAssembly(TlbId, Major, Minor, lcid, out asmName, out asmCodeBase);
    }

    internal static bool IsPrimaryInteropAssembly(Assembly asm)
    {
        // Retrieve the list of PIA attributes.
        IList<CustomAttributeData> aPIAAttrs = CustomAttributeData.GetCustomAttributes(asm);
        int NumPIAAttrs = aPIAAttrs.Count;

        for (int cPIAAttrs = 0; cPIAAttrs < NumPIAAttrs; cPIAAttrs++)
        {
            if (aPIAAttrs[cPIAAttrs].Constructor.DeclaringType == typeof(PrimaryInteropAssemblyAttribute))
                return true;
        }        

        return false;
    }

    internal static bool ValidateMachineType(TypeLibImporterFlags flags, _SYSKIND syskind)
    {
        int count = 0;
        if (IsImportingToX86(flags))
            count++;
        if (IsImportingToX64(flags))
            count++;
        if (IsImportingToItanium(flags))
            count++;
        if (IsImportingToAgnostic(flags))
            count++;

        if (count > 1)
        {
            Output.WriteError(Resource.FormatString("Err_BadMachineSwitch"), ErrorCode.Err_BadMachineSwitch);
            return false;      
        }

        
        // Check the import type against the type of the type library.
        if (syskind == _SYSKIND.SYS_WIN64)
        {
            // If x86 was chosen, throw an error.
            if (IsImportingToX86(flags))
            {
                Output.WriteError(Resource.FormatString("Err_BadMachineSwitch"), ErrorCode.Err_BadMachineSwitch);
                return false;
            }

            // If nothing is chosen, output a warning on all platforms.
            if (IsImportingToDefault(flags))
            {
                Output.WriteWarning(Resource.FormatString("Wrn_AgnosticAssembly"), WarningCode.Wrn_AgnosticAssembly);
            }
        }
        else if (syskind == _SYSKIND.SYS_WIN32)
        {
            // If a 64-bit option was chosen, throw an error.
            if (IsImportingToItanium(flags) || IsImportingToX64(flags))
            {
                Output.WriteError(Resource.FormatString("Err_BadMachineSwitch"), ErrorCode.Err_BadMachineSwitch);
                return false;
            }

#if WIN64
            // If nothing is chosen, and we're on a 64-bit machine, output a warning
            if (IsImportingToDefault(flags))
            {
                Output.WriteWarning(Resource.FormatString("Wrn_AgnosticAssembly"), WarningCode.Wrn_AgnosticAssembly);
            }
#endif

        }

        return true;
    }

    internal static bool IsImportingToItanium(TypeLibImporterFlags flags)
    {
        return ((flags & TypeLibImporterFlags.ImportAsItanium) != 0);
    }

    internal static bool IsImportingToX64(TypeLibImporterFlags flags)
    {
        return ((flags & TypeLibImporterFlags.ImportAsX64) != 0);
    }

    internal static bool IsImportingToX86(TypeLibImporterFlags flags)
    {
        return ((flags & TypeLibImporterFlags.ImportAsX86) != 0);
    }
    
    internal static bool IsImportingToAgnostic(TypeLibImporterFlags flags)
    {
        return ((flags & TypeLibImporterFlags.ImportAsAgnostic) != 0);
    }

    internal static bool IsImportingToDefault(TypeLibImporterFlags flags)
    {
        return !(IsImportingToItanium(flags) || IsImportingToX64(flags) || IsImportingToX86(flags) || IsImportingToAgnostic(flags));
    }

    [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    private static extern void LoadTypeLibEx(String strTypeLibName, REGKIND regKind, out System.Runtime.InteropServices.ComTypes.ITypeLib TypeLib);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int SearchPath(String path, String fileName, String extension, int numBufferChars, StringBuilder buffer, int[] filePart);

    internal static TlbImpOptions s_Options;

    // List of libraries being imported, as guids.
    internal static Hashtable s_ImportingLibraries = new Hashtable();
    
    // List of libraries that have been imported, as guids.
    internal static Hashtable s_AlreadyImportedLibraries = new Hashtable();

    // Assembly references provided on the command line via /reference; keyed by guid.
    internal static Hashtable s_AssemblyRefs = new Hashtable();

    // TypeLib converter.
    internal static TypeLibConverter s_TypeLibConverter = new TypeLibConverter();

    // Array of type libraries just to keep the references alive.
    private static Dictionary<string, System.Runtime.InteropServices.ComTypes.ITypeLib> s_RefTypeLibraries = new Dictionary<string,System.Runtime.InteropServices.ComTypes.ITypeLib>();
}

//******************************************************************************
// The resolution callback class.
//******************************************************************************
internal class ImporterCallback : ITypeLibImporterNotifySink
{
    public void ReportEvent(ImporterEventKind EventKind, int EventCode, String EventMsg)
    {
        if (EventKind == ImporterEventKind.NOTIF_TYPECONVERTED)
        {
            if (TlbImpCode.s_Options.m_bVerboseMode)
                Output.WriteInfo(EventMsg, (MessageCode)EventCode);
        }
        else if (EventKind == ImporterEventKind.NOTIF_CONVERTWARNING)
        {
            Output.WriteWarning(EventMsg, (WarningCode)EventCode);
        }
        else
        {
            Output.Write(EventMsg);
        }
    }

    public Assembly ResolveRef(Object TypeLib)
    {
        Assembly rslt = null;
        System.Runtime.InteropServices.ComTypes.ITypeLib pTLB = (System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib;
        String strPIAName = null;
        String strPIACodeBase = null;
        bool bExistingAsmLoaded = false;
        bool bGeneratingPIA = (TlbImpCode.s_Options.m_flags & TypeLibImporterFlags.PrimaryInteropAssembly) != 0;     


        //----------------------------------------------------------------------
        // Display a message indicating we are resolving a reference.

        if (TlbImpCode.s_Options.m_bVerboseMode)
            Output.WriteInfo(Resource.FormatString("Msg_ResolvingRef", Marshal.GetTypeLibName(pTLB)), MessageCode.Msg_ResolvingRef);


        //----------------------------------------------------------------------
        // Check our list of referenced assemblies.

        rslt = (Assembly)TlbImpCode.s_AssemblyRefs[Marshal.GetTypeLibGuid((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib)];
        if (rslt != null)
        {
            // PIA should only reference PIA
            if (bGeneratingPIA && !TlbImpCode.IsPrimaryInteropAssembly(rslt))
                throw new TlbImpGeneralException(Resource.FormatString("Err_ReferencedPIANotPIA", rslt.GetName().Name), ErrorCode.Err_ReferencedPIANotPIA);

            // If we are in verbose mode then display message indicating we successfully resolved the assembly 
            // from the list of referenced assemblies.
            if (TlbImpCode.s_Options.m_bVerboseMode)
                Output.WriteInfo(Resource.FormatString("Msg_RefFoundInAsmRefList", Marshal.GetTypeLibName(pTLB), rslt.GetName().Name), MessageCode.Msg_RefFoundInAsmRefList);

            return rslt;
        }

        // If the assembly wasn't provided on the command line and the user
        // doesn't want us touching the registry for PIAs, throw an error now.
        if (TlbImpCode.s_Options.m_bStrictRefNoPia)
            throw new TlbImpGeneralException(Resource.FormatString("Err_RefNotInList", Marshal.GetTypeLibName((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib)), ErrorCode.Err_RefNotInList);

        //----------------------------------------------------------------------
        // Look for a primary interop assembly for the typelib.

        if (TlbImpCode.GetPrimaryInteropAssembly(TypeLib, out strPIAName, out strPIACodeBase))
        {
            if (strPIAName != null)
                strPIAName = AppDomain.CurrentDomain.ApplyPolicy(strPIAName);

            // Load the primary interop assembly.
            try
            {
                // First try loading the assembly using its full name.
                rslt = Assembly.ReflectionOnlyLoad(strPIAName);
            }
            catch (FileNotFoundException)
            {
                // If that failed, try loading it using LoadFrom bassed on the codebase if specified.
                if (strPIACodeBase != null)
                    rslt = Assembly.ReflectionOnlyLoadFrom(strPIACodeBase);
                else
                    throw;
            }
            catch (FileLoadException)
            {
                // If that failed, try loading it using LoadFrom bassed on the codebase if specified.
                if (strPIACodeBase != null)
                    rslt = Assembly.ReflectionOnlyLoadFrom(strPIACodeBase);
                else
                    throw;
            }

            // Validate that the assembly is indeed a PIA.
            if (!TlbImpCode.IsPrimaryInteropAssembly(rslt))
                throw new TlbImpGeneralException(Resource.FormatString("Err_RegisteredPIANotPIA", rslt.GetName().Name, Marshal.GetTypeLibName((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib)), ErrorCode.Err_RegisteredPIANotPIA);

            // If we are in verbose mode then display message indicating we successfully resolved the PIA.
            if (TlbImpCode.s_Options.m_bVerboseMode)
                Output.WriteInfo(Resource.FormatString("Msg_ResolvedRefToPIA", Marshal.GetTypeLibName(pTLB), rslt.GetName().Name), MessageCode.Msg_ResolvedRefToPIA);

            return rslt;
        }


        //----------------------------------------------------------------------
        // If we are generating a primary interop assembly or if strict ref mode
        // is enabled, then the resolve ref has failed.

        if (bGeneratingPIA)
            throw new TlbImpGeneralException(Resource.FormatString("Err_NoPIARegistered", Marshal.GetTypeLibName((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib)), ErrorCode.Err_NoPIARegistered);
        if (TlbImpCode.s_Options.m_bStrictRef)
            throw new TlbImpGeneralException(Resource.FormatString("Err_RefNotInList", Marshal.GetTypeLibName((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib)), ErrorCode.Err_RefNotInList);

        
        //----------------------------------------------------------------------
        // See if this has already been imported.

        rslt = (Assembly)TlbImpCode.s_AlreadyImportedLibraries[Marshal.GetTypeLibGuid((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib)];
        if (rslt != null)
        {
            // If we are in verbose mode then display message indicating we successfully resolved the assembly 
            // from the list of referenced assemblies.
            if (TlbImpCode.s_Options.m_bVerboseMode)
                Output.WriteInfo(Resource.FormatString("Msg_AssemblyResolved", Marshal.GetTypeLibName((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib)), MessageCode.Msg_AssemblyResolved);

            return rslt;
        }


        //----------------------------------------------------------------------
        // Try to load the assembly.

        String FullyQualifiedAsmFileName = Path.Combine(TlbImpCode.s_Options.m_strOutputDir, Marshal.GetTypeLibName((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib) + ".dll");
        try
        {
            IntPtr pAttr = (IntPtr)0;
            _TYPELIBATTR Attr;
            Int16 MajorTlbVer = 0;
            Int16 MinorTlbVer = 0;
            Guid TlbId;

            // Check to see if we've already built the assembly.
            rslt = Assembly.ReflectionOnlyLoadFrom(FullyQualifiedAsmFileName);

            // Remember we loaded an existing assembly.
            bExistingAsmLoaded = true;

            // Get the major and minor version number from the TypeLibAttr.
            try
            {
                ((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib).GetLibAttr(out pAttr);
                Attr = (_TYPELIBATTR)Marshal.PtrToStructure(pAttr, typeof(_TYPELIBATTR));
                MajorTlbVer = Attr.wMajorVerNum;
                MinorTlbVer = Attr.wMinorVerNum;
                TlbId = Attr.guid;
            }
            finally
            {
                if (pAttr != (IntPtr)0)
                    ((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib).ReleaseTLibAttr(pAttr);
            }

            // Make sure the assembly is for the current typelib and that the version number of the 
            // loaded assembly is the same as the version number of the typelib.
            Version asmVersion = rslt.GetName().Version;
            if ((Marshal.GetTypeLibGuidForAssembly(rslt) == TlbId) &&
                ((asmVersion.Major == MajorTlbVer && asmVersion.Minor == MinorTlbVer) ||
                 (asmVersion.Major == 0 && asmVersion.Minor == 0 && MajorTlbVer == 1 && MinorTlbVer == 0)) )
            {
                // If we are in verbose mode then display message indicating we successfully loaded the assembly.
                if (TlbImpCode.s_Options.m_bVerboseMode)
                    Output.WriteInfo(Resource.FormatString("Msg_AssemblyLoaded", FullyQualifiedAsmFileName), MessageCode.Msg_AssemblyLoaded);

                // Remember the loaded assembly.
                TlbImpCode.s_AlreadyImportedLibraries[Marshal.GetTypeLibGuid((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib)] = rslt;
            
                return rslt;
            }
            else if (TlbImpCode.s_Options.m_bVerboseMode)
            {
                // If we are in verbose mode then display message indicating we found an assembly that doesn't match
                Output.WriteInfo(Resource.FormatString("Msg_AsmRefLookupMatchProblem", new Object[] {
                        FullyQualifiedAsmFileName, Marshal.GetTypeLibGuidForAssembly(rslt), asmVersion.Major, 
                        asmVersion.Minor, Marshal.GetTypeLibName((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib), TlbId, MajorTlbVer, MinorTlbVer }), MessageCode.Msg_AsmRefLookupMatchProblem);
            }
        }
        catch (System.IO.FileNotFoundException)
        {
            // This is actually great, just fall through to create the new file.
        }


        //----------------------------------------------------------------------
        // Make sure an existing assembly will not be overwritten by the 
        // assembly generated by the typelib being imported.

        if (bExistingAsmLoaded)
            throw new TlbImpGeneralException(Resource.FormatString("Err_ExistingAsmOverwrittenByRefAsm", Marshal.GetTypeLibName((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib), FullyQualifiedAsmFileName), ErrorCode.Err_ExistingAsmOverwrittenByRefAsm);


        //----------------------------------------------------------------------
        // Make sure the current assembly will not be overriten by the 
        // assembly generated by the typelib being imported.
        
        if (String.Compare(FullyQualifiedAsmFileName, TlbImpCode.s_Options.m_strAssemblyName, true, CultureInfo.InvariantCulture) == 0)
            throw new TlbImpGeneralException(Resource.FormatString("Err_RefAsmOverwrittenByOutput", Marshal.GetTypeLibName((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib), FullyQualifiedAsmFileName), ErrorCode.Err_RefAsmOverwrittenByOutput);


        //----------------------------------------------------------------------
        // See if this is already on the stack.

        if (TlbImpCode.s_ImportingLibraries.Contains(Marshal.GetTypeLibGuid((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib).ToString()))
        {
            // Print an error message and return null to stop importing the current type but
            // continue with the rest of the import.
            Output.WriteWarning(Resource.FormatString("Wrn_CircularReference", Marshal.GetTypeLibName((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib)), WarningCode.Wrn_CircularReference);
            return null;
        }


        //----------------------------------------------------------------------
        // If we have not managed to load the assembly then import the typelib.

        if (TlbImpCode.s_Options.m_bVerboseMode)
            Output.WriteInfo(Resource.FormatString("Msg_AutoImportingTypeLib", Marshal.GetTypeLibName((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib), FullyQualifiedAsmFileName), MessageCode.Msg_AutoImportingTypeLib);
    
        try
        {
            rslt = TlbImpCode.DoImport((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib, 
                                    FullyQualifiedAsmFileName, 
                                    null,
                                    null,
                                    TlbImpCode.s_Options.m_aPublicKey, 
                                    TlbImpCode.s_Options.m_sKeyPair, 
                                    TlbImpCode.s_Options.m_strProduct,
                                    TlbImpCode.s_Options.m_strProductVersion,
                                    TlbImpCode.s_Options.m_strCompany,
                                    TlbImpCode.s_Options.m_strCopyright,
                                    TlbImpCode.s_Options.m_strTrademark,
                                    TlbImpCode.s_Options.m_flags,
                                    TlbImpCode.s_Options.m_isVersion2,
                                    TlbImpCode.s_Options.m_isPreserveSig,
                                    TlbImpCode.s_Options.m_isRemoveEnumPrefix);
            
            // The import could fail. In this case, 
            if (rslt == null) return null;

            // Remember the imported assembly.
            TlbImpCode.s_AlreadyImportedLibraries[Marshal.GetTypeLibGuid((System.Runtime.InteropServices.ComTypes.ITypeLib)TypeLib)] = rslt;
        }
        catch (ReflectionTypeLoadException e)
        {
            // Display the type load exceptions that occurred and rethrow the exception.
            int i;
            Exception[] exceptions;
            Output.WriteError(Resource.FormatString("Err_TypeLoadExceptions"), ErrorCode.Err_TypeLoadExceptions);
            exceptions = e.LoaderExceptions;
            for (i = 0; i < exceptions.Length; i++)
            {
                try 
                {
                    Output.WriteInfo(
                        Resource.FormatString(
                            "Msg_DisplayException", 
                            new object[] { i, exceptions[i].GetType().ToString(), exceptions[i].Message } ), 
                        MessageCode.Msg_DisplayException);
                }
                catch (Exception ex)
                {
                    Output.WriteInfo(
                        Resource.FormatString(
                            "Msg_DisplayNestedException", 
                            new object[] { i, ex.GetType().ToString(), ex.Message } ), 
                        MessageCode.Msg_DisplayNestedException);
                }
            }
            throw e;
        }
        
        return rslt;
    }
}

}
