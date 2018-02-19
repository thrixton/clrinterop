using System.Reflection;
using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Collections;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

// yirutang 7/2004
public class AssemPrinter
{
    StreamWriter sw = null;
    Assembly CurrentAssembly = null;
    StringCompare sc = new StringCompare();
    int fails = 0;
    BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static;

    private void ConnectEvent()
    {
        AppDomain currentDomain = Thread.GetDomain();
        ResolveEventHandler asmResolveHandler = new ResolveEventHandler(this.ReflectionOnlyResolveAsmEvent);
        currentDomain.ReflectionOnlyAssemblyResolve += asmResolveHandler;
    }

    private Assembly ReflectionOnlyResolveAsmEvent(Object sender, ResolveEventArgs args)
    {
        string asmName = AppDomain.CurrentDomain.ApplyPolicy(args.Name);
        return TryLoadAssembly(new AssemblyName(args.Name));
    }
    
    ArrayList asmpaths = new ArrayList();
    bool verbose = false;


    public static int PrinterAssemblyInPlace(string assemblyFileFullPath)
    {
        AssemPrinter test = new AssemPrinter();

        test.ConnectEvent();

        try
        {
            string OutputFile = assemblyFileFullPath + ".txt";
            test.sw = File.CreateText(OutputFile);
            Console.WriteLine("Created Text:" + OutputFile);
            test.CurrentAssembly = Assembly.ReflectionOnlyLoadFrom(assemblyFileFullPath);
            test.PrintAssembly(test.CurrentAssembly);
        }
        catch (Exception e)
        {
            if (e is ReflectionTypeLoadException)
            {
                ReflectionTypeLoadException rtle = (ReflectionTypeLoadException) e;
                string msg = e.Message + "\n";
                foreach(Exception ex in rtle.LoaderExceptions)
                {
                                    msg += "=> " + ex.Message + "\n";
                    if (ex.InnerException != null)
                        msg += "=> Inner => " + ex.InnerException.GetType().ToString() + " - " + ex.InnerException.Message;
                }

                            test.ReportFailure(msg);
            }
            else
                test.ReportFailure(e.ToString());
        }
        finally
        {
            test.sw.Close();
        }
        return test.ExitTest();

    }

    void ReportFailure(String s)
    {
        Console.WriteLine("Fail ==:" + s);
        fails++;
    }
    int ExitTest()
    {
        if (fails == 0) return 100;
        else return 50;
    }

    Assembly TryLoadAssembly(AssemblyName a)
    {
        // 1. Use asm path setting. In some cases there are left over DLLs in the current path in some test cases have failed
        foreach (object s in asmpaths)
        {
            if (File.Exists(Path.Combine(s.ToString(), a.Name + ".dll")))
            {
                Console.WriteLine("Loading " + Path.Combine(s.ToString(), a.Name + ".dll"));
                return Assembly.ReflectionOnlyLoadFrom(Path.Combine(s.ToString(), a.Name + ".dll"));

            }
        }

        // 2. Try loading directly to deal with strongly-named assemblies
        try
        {
            Assembly asm = Assembly.ReflectionOnlyLoad(a.FullName);
            return asm;
        }
        catch(Exception)
        {}

        // 3. Try current path
        if (File.Exists(a.Name + ".dll"))
        {
            if (verbose) Console.WriteLine("Loading " + a.Name + ".dll");
            return Assembly.ReflectionOnlyLoadFrom(a.Name + ".dll");
        }

        return null;
    }

    void PrintAssembly(Assembly asm)
    {
        // assembly name
        AssemblyName[] asms = asm.GetReferencedAssemblies();

        // Pre-Load the referenced assemblies first
        // user can specify customerized reference through /asmpath option

        foreach (AssemblyName a in asms)
        {
            if (TryLoadAssembly(a) == null)
            {
                throw new ApplicationException("Fail to Find " + a.Name);
            }
        }
        Console.WriteLine("Loaded Assemblies");
        Assembly[] asmss = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies();
        for (int i = 0; i < asmss.Length; i++)
            Console.WriteLine(asmss[i]);

        Array.Sort(asms, sc);
        foreach (AssemblyName a in asms)
            PrintAssemblyName(null, a, true);
        PrintAssemblyName(asm, asm.GetName(), false);
        // module
        Module amod = asm.ManifestModule;
        Module[] mods = asm.GetModules(true);
        Array.Sort(mods, sc);
        foreach (Module mod in mods)
        {
            if (!mod.Name.Equals(amod.Name))
                sw.WriteLine(".module manifest " + mod.Name);
            else if (!mod.IsResource())
                sw.WriteLine(".module " + mod.Name);
            else sw.WriteLine(".resource " + mod.Name);
            PrintCustomAttributes(CustomAttributeData.GetCustomAttributes(mod));

        }
        // file
        FileStream[] files = asm.GetFiles();
        Array.Sort(files, sc);
        foreach (FileStream file in files)
            sw.WriteLine(".file " + Path.GetFileName(file.Name)); // TODO seems the file api has provided less than CLI such as hashcode
        // .subsystem (Console or UI)We cannot get it from reflection
        // .corflags ( 64, 32 or IL platforms) 
        PortableExecutableKinds PEKind = (PortableExecutableKinds)0;
        ImageFileMachine Machine = (ImageFileMachine)0;
        asm.ManifestModule.GetPEKind(out PEKind, out Machine);
        sw.WriteLine(".pekind " + PEKind);
        sw.WriteLine(".imageMachine " + Machine);
        // .class
        Type[] ts = amod.GetTypes();
        Array.Sort(ts, sc);
        foreach (Type t in ts)
            PrintType(t, false);
        // .field and .method (only the manifest module ... )
        FieldInfo[] fis = amod.GetFields(bf);
        Array.Sort(fis, sc);
        foreach (FieldInfo fi in fis) // global fields
            PrintField(fi);
        MethodInfo[] mis = amod.GetMethods(bf);
        Array.Sort(mis, sc);
        foreach (MethodInfo mi in mis) // global methods
            PrintMethod(mi);

        Console.WriteLine("Loaded Assemblies");
        asmss = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies();
        for (int i = 0; i < asmss.Length; i++)
            Console.WriteLine(asmss[i]);

    }
    void PrintAssemblyName(Assembly asm, AssemblyName asmName, bool IsExtern)
    {
        if (IsExtern)
            sw.WriteLine(".assembly extern " + asmName.Name);
        else sw.WriteLine(".assembly " + asmName.Name);
        sw.WriteLine("{");
        // we don't care the referenced assembly version here in order to make the program stronger
        sw.WriteLine(".ver |");
        sw.WriteLine(".hash " + asmName.HashAlgorithm);
        sw.WriteLine(".culture " + ((asmName.CultureInfo != null) ? "NULL" : asmName.CultureInfo.ToString())); // for null we just print nothing
        sw.WriteLine(".versionCompatibility " + asmName.VersionCompatibility);
        if (asmName.KeyPair != null)
            sw.WriteLine(".keypair " + BytesToString(asmName.KeyPair.PublicKey));
        sw.WriteLine(".publickey " + BytesToString(asmName.GetPublicKey()));
        if (asmName.GetPublicKeyToken() != null)
            sw.WriteLine(".publickeytoken " + BytesToString(asmName.GetPublicKeyToken()));

        // print out assembly name flags
        string s = null;
        // add back in orcas
        /*if ((asmName.Flags & AssemblyNameFlags.Library) != 0)
            s = "Library";
        else if ((asmName.Flags & AssemblyNameFlags.AppDomainPlatform) != 0)
            s = "AppDomainPlatform";
        else if ((asmName.Flags & AssemblyNameFlags.ProcessPlatform) != 0)
            s = "ProcessPlatform";
        else if ((asmName.Flags & AssemblyNameFlags.SystemPlatform) != 0)
            s = "SystemPlatform";*/
        sw.WriteLine(".flags " + s);

        if (!IsExtern)
        {
            PrintCustomAttributes(CustomAttributeData.GetCustomAttributes(asm));
        }
        sw.WriteLine("} // end of assembly");
        // We don't care codebase
    }
    void PrintCustomAttributes(IList<CustomAttributeData> cads)
    {
        ArrayList sortedList = new ArrayList();
        // customattributes
        foreach (CustomAttributeData cad in cads)
            sortedList.Add(cad);
        sortedList.Sort(sc);
        foreach (CustomAttributeData ca in sortedList)
            sw.WriteLine(".custom " + AttrToString(ca));


    }
    // print out manifest resource
    void PrintManifestResource(ManifestResourceInfo msi)
    {
        //.mresource [public | private] <dottedname>  [( <QSTRING> )] { <manResDecl>* }
        //!! Is there customAttribute on Resource?? Is there public private resource?
        if (msi.FileName != null)
        {
            sw.WriteLine("{ .file " + msi.FileName + "}");
        }
        else if (msi.ReferencedAssembly != null)
        {
            if (!msi.ReferencedAssembly.Equals(CurrentAssembly))
                sw.WriteLine("{.assmebly extern " + msi.ReferencedAssembly.GetName().Name + "}");
        }
    }
    // print out typeinfo
    void PrintType(Type t, bool IsExtern)
    {
        try
        {
            sw.Write(".class ");
            if (IsExtern)
                sw.Write("extern ");
            sw.Write(t.FullName);
            if (t.IsGenericTypeDefinition)
            {
                Type[] ts = t.GetGenericArguments(); // we must have at least one generic argument	
                sw.Write("<");
                for (int i = 0; i < ts.Length; i++)
                {
                    sw.Write("("); // we should always have some generic constrains
                    Type[] cts = ts[i].GetGenericParameterConstraints();
                    Array.Sort(cts, sc);
                    for (int j = 0; j < cts.Length; j++)
                    {
                        sw.Write(PrintTypeWithAssem(cts[j]));
                        if (j < cts.Length - 1)
                            sw.Write(", ");
                    }
                    sw.Write(") ");
                    sw.Write(ts[i]);
                    if (i < (ts.Length - 1))
                        sw.Write(", ");
                }
                sw.Write(">");
            }
            sw.Write(" (");
            if (t.IsInterface)
                sw.Write("Interface ");
            sw.WriteLine(ProcessAttributeString(t.Attributes.ToString()) + ")");
            if (t.BaseType != null)
            {
                sw.Write(".extends [" + t.BaseType.Assembly.GetName().Name + "]" + t.BaseType);
            }
            Array infs = t.GetInterfaces();

            if (infs.Length != 0)
            {
                sw.Write(Environment.NewLine + ".implements ");
                Array.Sort(infs, sc);
            }
            foreach (Type inf in t.GetInterfaces())
            {
                sw.Write(inf + ", ");
            }
            sw.WriteLine();


            sw.WriteLine("{");
            PrintCustomAttributes(CustomAttributeData.GetCustomAttributes(t));
            ConstructorInfo[] CList = t.GetConstructors(bf);
            Array.Sort(CList, sc);
            foreach (ConstructorInfo ci in CList)
                PrintMethod(ci);

            FieldInfo[] FList = t.GetFields(bf);
            Array.Sort(FList, sc);
            foreach (FieldInfo fi in FList)
                PrintField(fi);

            MethodInfo[] MList = t.GetMethods(bf);
            Array.Sort(MList, sc);
            foreach (MethodInfo mi in MList)
                PrintMethod(mi);

            PropertyInfo[] PList = t.GetProperties(bf);
            Array.Sort(PList, sc);
            foreach (PropertyInfo pi in PList)
                PrintProperty(pi);

            EventInfo[] EList = t.GetEvents(bf);
            Array.Sort(EList, sc);
            foreach (EventInfo ei in EList)
                PrintEvent(ei);

            sw.WriteLine(@"} //end of class " + t);
        }
        catch (Exception e)
        {
            Console.WriteLine("UnExpected Exception thrown at type : " + t);
            Console.WriteLine(e);
        }
    }
    string PrintMember(MemberInfo mi)
    {
        StringBuilder sb = new StringBuilder();
        Type t = null;
        if (mi.MemberType == MemberTypes.Field)
            t = ((FieldInfo)mi).FieldType;
        else if (mi.MemberType == MemberTypes.Property)
            t = ((PropertyInfo)mi).PropertyType;
        sb.Append(PrintTypeWithAssem(t));
        sb.Append(" ");
        sb.Append(mi.Name);
        return sb.ToString();

    }
    // printout fieldinfo
    // field attribute, custom attribute, field.tostring
    void PrintField(FieldInfo fi)
    {
        sw.Write(".field (" + ProcessAttributeString(fi.Attributes.ToString()) + ") " + PrintMember(fi));
        // we should be able to get the literal value out of fieldinfo
        if (fi.IsLiteral)
        {
            try
            {
                sw.WriteLine(" = " + fi.GetRawConstantValue());
            }
            catch (NotSupportedException) //TODO:remove this once VSWhidbey 398502 gets fixed
            {
                sw.Write(Environment.NewLine);
            }
        }
        else
        {
            sw.Write(Environment.NewLine);
        }
        PrintCustomAttributes(CustomAttributeData.GetCustomAttributes(fi));
    }
    // print out the method (can be used for methodinfo and constructorinfo)
    // method attributes, custom attrubtes (entry point) and methodbody
    void PrintMethod(MethodBase mi)
    {
        try
        {
            sw.Write(".method ");
            sw.Write(" (" + ProcessAttributeString(mi.Attributes.ToString()) + ") ");
            sw.WriteLine(PrintMethodHead(mi));

            // print out the .override
            if (typeof(MethodInfo).IsInstanceOfType(mi))
                if (((MethodInfo)mi).GetBaseDefinition().Equals(mi))
                    sw.Write(".override " + mi.DeclaringType + "." + mi.Name);

            sw.WriteLine("{");
            // method custom attribute
            PrintCustomAttributes(CustomAttributeData.GetCustomAttributes(mi));

            // parameter custom attributes
            // only method info has a return type
            if (typeof(MethodInfo).IsInstanceOfType(mi))
            {
                ParameterInfo ret = ((MethodInfo)mi).ReturnParameter;
                if ((ret != null) && (CustomAttributeData.GetCustomAttributes(ret).Count != 0))
                {
                    sw.WriteLine(".param [0]");
                    PrintCustomAttributes(CustomAttributeData.GetCustomAttributes(ret));
                }
            }
            ParameterInfo[] pis = mi.GetParameters();
            for (int i = 0; i < pis.Length; i++)
            {
                if ((CustomAttributeData.GetCustomAttributes(pis[i]).Count != 0) /*|| (pi.Attributes & ParameterAttributes.HasDefault) != 0)*/)
                {
                    sw.Write(".param [" + (i + 1) + "]");
                    // default value on parameter Info
                    /*if ((pi.Attributes & ParameterAttributes.HasDefault) != 0)
                    {
                        sw.WriteLine(" = " + pis[i].GetRawConstantValue());
                    }
                    else*/
                    sw.WriteLine();
                    if (CustomAttributeData.GetCustomAttributes(pis[i]).Count != 0)
                        PrintCustomAttributes(CustomAttributeData.GetCustomAttributes(pis[i]));
                }
            }
            if (mi.Equals(mi.DeclaringType.Assembly.EntryPoint))
                sw.WriteLine(".entrypoint");

            // print out the method body content
            MethodBody mbd = mi.GetMethodBody();
            if (mbd != null)
            {
                // .maxstack
                sw.WriteLine(".maxstack " + mbd.MaxStackSize);
                // .locals
                sw.Write(".locals ");
                if (mbd.InitLocals)
                    sw.Write("int ");
                sw.Write("(");
                foreach (LocalVariableInfo lv in mbd.LocalVariables)
                    sw.Write(lv.ToString() + " ");
                sw.WriteLine(")");
                // printout the opcode
                ILReader.ILReader ilr = new ILReader.ILReader(mi, CurrentAssembly);
                foreach (ILReader.Opcode opc in ilr.GetOpcodes())
                    sw.WriteLine(opc.DisplayIL());
            }

            sw.WriteLine("} // end of method " + mi.Name);
        }
        catch (Exception e)
        {
            Console.WriteLine("Unexpected Exception At " + mi.ReflectedType + "::" + mi);
            Console.WriteLine(e);
        }
    }

    // the ToString function of CustomAttribute Data
    string AttrToString(CustomAttributeData ca)
    {
        StringBuilder ctorArgs = new StringBuilder();
        //bool shouldReplace = false;
        for (int i = 0; i < ca.ConstructorArguments.Count; i++)
        {
            // some intro whidbey breaking change, swallow the diff 
            // one of MarshalAsAttributes's field value change from IidParamIndex to IidParameterIndex
            /*if (ca.Constructor.DeclaringType == typeof(System.Runtime.InteropServices.MarshalAsAttribute))
            {
                shouldReplace = true;
            }*/
            ctorArgs.Append(string.Format(i == 0 ? "{0}" : ", {0}", ca.ConstructorArguments[i]));
        }

        StringBuilder namedArgs = new StringBuilder();
        for (int i = 0; i < ca.NamedArguments.Count; i++)
        {
            if (ca.NamedArguments[i].TypedValue.ToString().Contains("System.Runtime.InteropServices.CustomMarshalers"))
            {
                namedArgs.Append(String.Format(i == 0 && ctorArgs.Length == 0 ? "{0}" : ", {0}", Regex.Replace(ca.NamedArguments[i].ToString(), @"\d+\.\d+\.\d+\.\d+", "2.0.0.0")));
            }
            else
                namedArgs.Append(String.Format(i == 0 && ctorArgs.Length == 0 ? "{0}" : ", {0}", ca.NamedArguments[i]));
        }
        if (ca.Constructor.DeclaringType.Assembly != CurrentAssembly)
            return String.Format("[{0}]{1}({2}{3})", ca.Constructor.DeclaringType.Assembly.GetName().Name, ca.Constructor.DeclaringType.FullName, ctorArgs.ToString(), namedArgs.ToString());
        else
            return String.Format("{0}({1}{2})", ca.Constructor.DeclaringType.FullName, ctorArgs.ToString(), namedArgs.ToString());
    }

    // print out the property info
    // inlcuding property attributes, custom attribute, get method, set method and other methods
    void PrintProperty(PropertyInfo pi)
    {
        sw.WriteLine(".property (" + ProcessAttributeString(pi.Attributes.ToString()) + ") " + PrintMember(pi));
        sw.WriteLine("{");
        // get all including non-public ones
        MethodInfo[] pims = pi.GetAccessors(true);
        PrintCustomAttributes(CustomAttributeData.GetCustomAttributes(pi));
        Array.Sort(pims, sc);
        foreach (MethodInfo mi in pims)
        {
            if (mi.Name.Equals("get_" + pi.Name))
                sw.WriteLine(".get (" + ProcessAttributeString(mi.Attributes.ToString()) + ") " + PrintMethodHead(mi));
            else if (mi.Name.Equals("set_" + pi.Name))
                sw.WriteLine(".set (" + ProcessAttributeString(mi.Attributes.ToString()) + ") " + PrintMethodHead(mi));
            else sw.WriteLine(".other (" + ProcessAttributeString(mi.Attributes.ToString()) + ") " + PrintMethodHead(mi));
        }
        sw.WriteLine("} // end of property " + pi.Name);
    }
    // print out the event info
    // including event attributes, custom attribute, add method, remove method and raise method
    void PrintEvent(EventInfo ei)
    {
        sw.WriteLine(".event (" + ProcessAttributeString(ei.Attributes.ToString()) + ") " + ei);
        sw.WriteLine("{");
        PrintCustomAttributes(CustomAttributeData.GetCustomAttributes(ei));
        MethodInfo ami = ei.GetAddMethod(true);
        if (ami != null)
            sw.WriteLine(".addmethod (" + ProcessAttributeString(ami.Attributes.ToString()) + ") " + PrintMethodHead(ami));
        MethodInfo rmi = ei.GetRemoveMethod(true);
        if (rmi != null)
            sw.WriteLine(".removemethod (" + ProcessAttributeString(rmi.Attributes.ToString()) + ") " + PrintMethodHead(rmi));
        MethodInfo rami = ei.GetRaiseMethod(true);
        if (rami != null)
            sw.WriteLine(".raisemethod (" + ProcessAttributeString(rami.Attributes.ToString()) + ") " + PrintMethodHead(rami));
        sw.WriteLine("} // end of event " + ei.Name);
    }
    // if type is in current assembly print t
    // else print [assembly]t
    // suppose it is never a generic parameter
    string PrintTypeWithAssem(Type t)
    {
        string s = "";
        if (!t.IsPrimitive && (t != typeof(void)) && (t != typeof(string)) && (t != typeof(object)) && (t.Assembly != CurrentAssembly))
            s = "[" + t.Assembly.GetName().Name + "]";
        return (s + t);

    }
    string PrintMethodHead(MethodBase mb)
    {
        StringBuilder s = new StringBuilder();

        if (typeof(MethodInfo).IsInstanceOfType(mb))
        {
            s.Append(PrintTypeWithAssem(((MethodInfo)mb).ReturnType) + " ");
        }
        s.Append(mb.Name);
        if (mb.IsGenericMethodDefinition)
        {

            Type[] ts = mb.GetGenericArguments(); // we must have at least one generic argument	
            s.Append(" <");
            for (int i = 0; i < ts.Length; i++)
            {
                s.Append("("); // we should always have some generic constrains
                Type[] cts = ts[i].GetGenericParameterConstraints();
                Array.Sort(cts, sc);
                for (int j = 0; j < cts.Length; j++)
                {
                    s.Append(PrintTypeWithAssem(cts[j]));
                    if (j < cts.Length - 1)
                        s.Append(", ");
                }
                s.Append(") ");
                s.Append(ts[i]);
                if (i < (ts.Length - 1))
                    s.Append(", ");
            }
            s.Append(">");
        }
        else if (mb.IsGenericMethod)
        {
            s.Append(" <");
            Type[] param = mb.GetGenericArguments();
            for (int i = 0; i < param.Length; i++)
            {
                s.Append(PrintTypeWithAssem(param[i]));
                if (i < (param.Length - 1))
                    s.Append(", ");
            }
            s.Append("> ");
        }
        s.Append(" (");
        int count = 0;
        foreach (ParameterInfo pa in mb.GetParameters())
        {
            // if the parameter is a generic parameter
            // we excerise some more info that we can get
            if (pa.ParameterType.IsGenericParameter)
            {
                if (pa.ParameterType.DeclaringMethod != null)
                    s.Append("!!" + pa.ParameterType.GenericParameterPosition + " " + pa.ParameterType);
                else s.Append("!" + pa.ParameterType.GenericParameterPosition + " " + pa.ParameterType);

            }
            else
            {
                s.Append(PrintTypeWithAssem(pa.ParameterType));
            }
            if ((++count) < mb.GetParameters().Length)
                s.Append(", ");
            else s.Append(")");
        }
        if (count == 0) s.Append(")");
        return s.ToString();
    }

    public class StringCompare : IComparer
    {
        int IComparer.Compare(Object x, Object y)
        {
            return String.Compare(x.ToString(),y.ToString(), true, CultureInfo.InvariantCulture);
        }
    }
    public string BytesToString(byte[] bytes)
    {
        if (bytes != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(string.Format(" {0:x}", bytes[i]));
            }
            sb.Append(" )");
            return sb.ToString();
        }
        else return "NULL";
    }
    public string ProcessAttributeString(string input)
    {
        String[] pieces = input.Split(new Char[] { ',', ' ' });
        ArrayList processed = new ArrayList();
        // if the attribute doesn't have Public string, it means it is private.
        // In TypeAttributes, NotPublic, Class, AutoLayout, AnsiClass are all of value 0, so there is a potential dangerous
        // of they appear randomly in ToString(). It is too late to screen out AutoLayout and AnsiClass since we arelady generated the baseline
        // we are screening out NotPublic and Class
        foreach (string p in pieces)
            if (!p.Equals(string.Empty) && !p.Equals("ClassSemanticsMask") && !p.Equals("NotPublic") && !p.Equals("Class"))
                processed.Add(p);
        processed.Sort(sc);
        StringBuilder sb = new StringBuilder();
        foreach (string p in processed)
            sb.Append(p + ", ");
        return sb.ToString(0, sb.Length - 2);
    }
}
