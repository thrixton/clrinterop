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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using FormattedOutput;
using CoreRuleEngine;
using TypeLibTypes.Interop;
using TlbImpRuleEngine;

namespace tlbimp2
{
    /// <summary>
    /// Represents collected mapping information for default interface & event interface to coclass / class interfaces 
    /// </summary>
    class ClassInterfaceMap
    {
        public ClassInterfaceMap(TypeLib typeLib)
        {
            m_typeLib = typeLib;
            m_defaultInterfaceInfoList = new List<DefaultInterfaceInfo>();

            Collect();
        }

        /// <summary>
        /// Collect information
        /// </summary>
        private void Collect()
        {
            // Remember all the interface name to coclass name mapping
            Hashtable interfaceToCoClassMapping = new Hashtable();

            //
            // For every coclass
            //
            int nCount = m_typeLib.GetTypeInfoCount();
            for (int n = 0; n < nCount; ++n)
            {
                TypeInfo type = m_typeLib.GetTypeInfo(n);
                //
                // Walk the list of implemented interfaces
                //
                using (TypeAttr attr = type.GetTypeAttr())
                {
                    if (attr.typekind == TypeLibTypes.Interop.TYPEKIND.TKIND_COCLASS)
                    {
                        DefaultInterfaceInfo defaultInterfaceInfo = new DefaultInterfaceInfo();
                        defaultInterfaceInfo.coclass = type;
                        defaultInterfaceInfo.coclassName = type.GetDocumentation();

                        for (int m = 0; m < attr.cImplTypes; ++m)
                        {
                            TypeLibTypes.Interop.IMPLTYPEFLAGS flags = type.GetImplTypeFlags(m);
                            bool bDefault = (flags & TypeLibTypes.Interop.IMPLTYPEFLAGS.IMPLTYPEFLAG_FDEFAULT) != 0;

                            // For invalid default interfaces, such as 
                            // coclass MyObj
                            // {
                            //     [default] interface IUnknown;
                            //     interface IA;
                            // }
                            // to use the first valid interface, which is IA;
                            // if (!bDefault) continue;

                            bool bSource = (flags & TypeLibTypes.Interop.IMPLTYPEFLAGS.IMPLTYPEFLAG_FSOURCE) != 0;

                            TypeInfo typeImpl = type.GetRefType(m);

                            using (TypeAttr attrImpl = typeImpl.GetTypeAttr())
                            {
                                // Skip IUnknown & IDispatch
                                if (attrImpl.Guid == WellKnownGuids.IID_IDispatch ||
                                    attrImpl.Guid == WellKnownGuids.IID_IUnknown)
                                    continue;

                                // Skip non-dispatch interfaces that doesn't derive from IUnknown
                                if (!attrImpl.IsDispatch && !ConvCommon.IsDerivedFromIUnknown(typeImpl))
                                    continue;

                                string name = typeImpl.GetDocumentation();

                                if (bSource)
                                {
                                    // Default source interface
                                    if (bDefault ||                                             // If explicitly stated as default, use that
                                        defaultInterfaceInfo.defaultSourceInterface == null)    // otherwise, try to use the first one
                                    {
                                        defaultInterfaceInfo.defaultSourceInterface = typeImpl;
                                        defaultInterfaceInfo.defaultSourceInterfaceName = name;
                                    }
                                }
                                else
                                {
                                    // Default interface
                                    if (bDefault ||                                     // If explicitly stated as default, use that
                                        defaultInterfaceInfo.defaultInterface == null)  // otherwise, try to use the first one
                                    {
                                        defaultInterfaceInfo.defaultInterface = typeImpl;
                                        defaultInterfaceInfo.defaultInterfaceName = name;
                                    }
                                }
                            }

                        }

                        //
                        // Walk through the list of implemented interfaces again. This time we remember all the implemented interfaces (including base)
                        //
                        for (int m = 0; m < attr.cImplTypes; ++m)
                        {
                            TypeInfo typeImpl = type.GetRefType(m);
                            string name = typeImpl.GetDocumentation();

                            while (typeImpl != null)
                            {
                                // If we arleady seen this interface 
                                if (interfaceToCoClassMapping.Contains(name))
                                {
                                    // and if it is for a different interface
                                    if ((string)interfaceToCoClassMapping[name] != defaultInterfaceInfo.coclassName)
                                    {
                                        // Set the name to null so that we know we've seen other interfaces
                                        interfaceToCoClassMapping[name] = null;
                                    }
                                }
                                else
                                {
                                    interfaceToCoClassMapping.Add(name, defaultInterfaceInfo.coclassName);
                                }

                                TypeAttr attrImpl = typeImpl.GetTypeAttr();
                                if (attrImpl.cImplTypes == 1)
                                    typeImpl = typeImpl.GetRefType(0);
                                else
                                    typeImpl = null;
                            }
                        }

                        // We do allow coclass that doesn't have any 'valid' default interfaces to have a class interface
                        // For example, 
                        // coclass MyObject {
                        //     [default] interface IUnknown;
                        // }
                        m_defaultInterfaceInfoList.Add(defaultInterfaceInfo);
                    }
                }
            }

            foreach (DefaultInterfaceInfo defaultInterfaceInfo in m_defaultInterfaceInfoList)
            {
                bool isExclusive = true;
                if (defaultInterfaceInfo.defaultInterface != null)
                {
                    if (interfaceToCoClassMapping.Contains(defaultInterfaceInfo.defaultInterfaceName))
                    {
                        if (interfaceToCoClassMapping[defaultInterfaceInfo.defaultInterfaceName] == null)
                        {
                            isExclusive = false;
                        }
                    }
                    defaultInterfaceInfo.isExclusive = isExclusive;
                }
            }
        }

        public bool GetExclusiveDefaultInterfaceForCoclass(TypeInfo coclassTypeInfo, out TypeInfo exclusiveDefaultInterfaceTypeInfo)
        {
            exclusiveDefaultInterfaceTypeInfo = null;
            string expectedName = coclassTypeInfo.GetDocumentation();

            foreach (DefaultInterfaceInfo defaultInterfaceInfo in m_defaultInterfaceInfoList)
            {
                if (defaultInterfaceInfo.isExclusive)
                {
                    if (defaultInterfaceInfo.coclassName == expectedName)
                    {
                        exclusiveDefaultInterfaceTypeInfo = defaultInterfaceInfo.defaultInterface;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool GetCoClassForExclusiveDefaultInterface(TypeInfo interfaceTypeInfo, out TypeInfo coclassTypeInfo)
        {
            coclassTypeInfo = null;

            string expectedName = interfaceTypeInfo.GetDocumentation();

            foreach (DefaultInterfaceInfo defaultInterfaceInfo in m_defaultInterfaceInfoList)
            {
                if (defaultInterfaceInfo.isExclusive)
                {
                    if (defaultInterfaceInfo.defaultInterfaceName == expectedName)
                    {
                        coclassTypeInfo = defaultInterfaceInfo.coclass;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Create local class interfaces
        /// </summary>
        public void CreateLocalClassInterfaces(ConverterInfo converterInfo)
        {
            // Walk the candidate list and generate class interfaces
            // Note: We need to create the class interface in two phases because
            // during creation of class interface, we'll need to create interface & event interface, which
            // could have parameters resolve back to coclass which requires certain class interface
            // So split into two stages so that when we are doing creation all necessary information are inplace
            List<ConvClassInterfaceLocal> convClassInterfaceLocals = new List<ConvClassInterfaceLocal>();

            // 
            // Phase 1 : Create ConvClassInterfaceLocal instances and associate interface with class interfaces
            //
            foreach (DefaultInterfaceInfo info in m_defaultInterfaceInfoList)
            {
                try
                {
                    ConvClassInterfaceLocal local = new ConvClassInterfaceLocal(
                        converterInfo,
                        info.coclass,
                        info.defaultInterface,
                        info.defaultSourceInterface,
                        info.isExclusive);
                    convClassInterfaceLocals.Add(local);
                }
                catch (ReflectionTypeLoadException)
                {
                    throw; // Fatal failure. Throw
                }
                catch (TlbImpResolveRefFailWrapperException)
                {
                    throw; // Fatal failure. Throw
                }
                catch (TlbImpGeneralException)
                {
                    throw; // Fatal failure. Throw
                }
                catch (TypeLoadException)
                {
                    throw; // TypeLoadException is critical. Throw.
                }
                catch (Exception)
                {
                }
            }

            //
            // Phase 2 : Create the class interface
            //
            foreach (ConvClassInterfaceLocal local in convClassInterfaceLocals)
            {
                try
                {
                    local.Create();
                }
                catch (ReflectionTypeLoadException)
                {
                    throw; // Fatal failure. Throw
                }
                catch (TlbImpResolveRefFailWrapperException)
                {
                    throw; // Fatal failure. Throw
                }
                catch (TlbImpGeneralException)
                {
                    throw; // Fatal failure. Throw
                }
                catch (TypeLoadException)
                {
                    throw; // TypeLoadException is critical. Throw.
                }
                catch (Exception)
                {
                }
            }

        }

        /// <summary>
        /// Struct that holds default interface information for the coclass 
        /// At most the coclass has two default interfaces, one default interface and one source interface
        /// </summary>
        private class DefaultInterfaceInfo
        {
            public TypeInfo coclass;
            public string coclassName;

            public TypeInfo defaultInterface;
            public string defaultInterfaceName;

            public TypeInfo defaultSourceInterface;
            public string defaultSourceInterfaceName;

            public bool isExclusive;
        }

        #region Private members

        TypeLib                     m_typeLib;                      // Corresponding type library
        List<DefaultInterfaceInfo>  m_defaultInterfaceInfoList;     // List of DefaultInterfaceInfo that maintains the mapping
                                                    

        #endregion

    }


    class Process
    {
        public ModuleBuilder CreateModuleBuilder(AssemblyBuilder assemblyBuilder, string strmodule)
        {
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(strmodule);
            return moduleBuilder;
        }
        public AssemblyBuilder CreateAssemblyBuilder(AssemblyName name, TypeLib tlb, TypeLibImporterFlags flags)
        {
            using (TypeLibAttr attr = tlb.GetLibAttr())
            {
                // New assembly as well as loaded assembly should be all in a ReflectionOnly context as we don't need to run the code
                AssemblyBuilder assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.ReflectionOnly);

                // Handle the type library name
                assemblyBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForImportedFromTypeLib(tlb.GetDocumentation()));

                // Handle the type library version
                assemblyBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForTypeLibVersion(attr.wMajorVerNum, attr.wMinorVerNum));

                // Handle the LIBID
                assemblyBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForGuid(attr.guid));

                // If we are generating a PIA, then set the PIA custom attribute.
                if ((flags & TypeLibImporterFlags.PrimaryInteropAssembly) != 0)
                    assemblyBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForPrimaryInteropAssembly(attr.wMajorVerNum, attr.wMinorVerNum));

                return assemblyBuilder;
            }
        }

        /// <summary>
        /// Create class interfaces for the specified type library
        /// </summary>
        private void CreateClassInterfaces()
        {
            m_converterInfo.GetClassInterfaceMap().CreateLocalClassInterfaces(m_converterInfo);    
        }

        public AssemblyBuilder DoProcess(
            Object typeLib,
            string asmFilename,
            TypeLibImporterFlags flags,
            ITypeLibImporterNotifySink notifySink,
            byte[] publicKey,
            StrongNameKeyPair keyPair,
            string asmNamespace,
            Version asmVersion,
            bool isVersion2,
            bool isPreserveSig,
            string ruleSetFileName)
        {
            m_resolver = notifySink;

            TypeLib tlb = new TypeLib((TypeLibTypes.Interop.ITypeLib)typeLib);

            if (asmNamespace == null)
            {
                asmNamespace = tlb.GetDocumentation();

                string fileName = System.IO.Path.GetFileNameWithoutExtension(asmFilename);
                if (fileName != asmNamespace)
                    asmNamespace = fileName;

                //
                // Support for GUID_ManagedName (for namespace)
                //
                string customManagedNamespace = tlb.GetCustData(CustomAttributeGuids.GUID_ManagedName) as string;
                if (customManagedNamespace != null)
                {
                    customManagedNamespace = customManagedNamespace.Trim();
                    if (customManagedNamespace.ToUpper().EndsWith(".DLL"))
                        customManagedNamespace = customManagedNamespace.Substring(0, customManagedNamespace.Length - 4);
                    else if (customManagedNamespace.ToUpper().EndsWith(".EXE"))
                        customManagedNamespace = customManagedNamespace.Substring(0, customManagedNamespace.Length - 4);

                    asmNamespace = customManagedNamespace;
                }
            }

            //
            // Check for GUID_ExportedFromComPlus
            //
            object value = tlb.GetCustData(CustomAttributeGuids.GUID_ExportedFromComPlus);
            if (value != null)
            {
                // Make this a critical failure, instead of returning null which will be ignored.
                throw new TlbImpGeneralException(Resource.FormatString("Err_CircularImport", asmNamespace), ErrorCode.Err_CircularImport);
            }

            string strModuleName = asmFilename;

            if (asmFilename.Contains("\\"))
            {
                int nIndex;
                for (nIndex = strModuleName.Length; strModuleName[nIndex - 1] != '\\'; --nIndex) ;
                strModuleName = strModuleName.Substring(nIndex);
            }

            // If the version information was not specified, then retrieve it from the typelib.
            if (asmVersion == null)
            {
                using (TypeLibAttr attr = tlb.GetLibAttr())
                {
                    asmVersion = new Version(attr.wMajorVerNum, attr.wMinorVerNum, 0, 0);
                }
            }

            // Assembly name should not have .DLL
            // while module name must contain the .DLL
            string strAsmName = String.Copy(strModuleName);
            if (strAsmName.EndsWith(".DLL", StringComparison.InvariantCultureIgnoreCase))
                strAsmName = strAsmName.Substring(0, strAsmName.Length - 4);

            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = strAsmName;
            assemblyName.SetPublicKey(publicKey);
            assemblyName.Version = asmVersion;
            assemblyName.KeyPair = keyPair;

            m_assemblyBuilder = CreateAssemblyBuilder(assemblyName, tlb, flags);

            m_moduleBuilder = CreateModuleBuilder(m_assemblyBuilder, strModuleName);

            // Add a listener for the reflection load only resolve events.
            AppDomain currentDomain = Thread.GetDomain();
            ResolveEventHandler asmResolveHandler = new ResolveEventHandler(ReflectionOnlyResolveAsmEvent);
            currentDomain.ReflectionOnlyAssemblyResolve += asmResolveHandler;

            ConverterSettings settings;
            settings.m_isGenerateClassInterfaces = true;
            settings.m_namespace = asmNamespace;
            settings.m_flags = flags;
            settings.m_isVersion2 = isVersion2;
            settings.m_isPreserveSig = isPreserveSig;
            RuleEngine.InitRuleEngine(new TlbImpActionManager(),
                                      new TlbImpCategoryManager(),
                                      new TlbImpConditionManager(),
                                      new TlbImpOperatorManager());
            if (ruleSetFileName != null)
            {
                try
                {
                    RuleFileParser parser = new RuleFileParser(ruleSetFileName);
                    settings.m_ruleSet = parser.Parse();
                }
                catch (Exception ex)
                {
                    Output.WriteWarning(Resource.FormatString("Wrn_LoadRuleFileFailed",
                                                              ruleSetFileName, ex.Message),
                                        WarningCode.Wrn_LoadRuleFileFailed);
                    settings.m_ruleSet = null;
                }
            }
            else
            {
                settings.m_ruleSet = null;
            }

            m_converterInfo = new ConverterInfo(m_moduleBuilder, tlb, m_resolver, settings);

            //
            // Generate class interfaces
            // NOTE:
            // We have to create class interface ahead of time because of the need to convert default interfaces to
            // class interfafces. However, this creates another problem that the event interface is always named first 
            // before the other interfaces, because we need to create the type builder for the event interface first
            // so that we can create a class interface that implement it. But in the previous version of TlbImp,
            // it doesn't have to do that because it can directly create a typeref with the class interface name,
            // without actually creating anything like the TypeBuilder. The result is that the name would be different 
            // with interop assemblies generated by old tlbimp in this case.
            // Given the nature of reflection API, this cannot be easily workarounded unless we switch to metadata APIs. 
            // I believe this is acceptable because this only happens when:
            // 1. People decide to migrate newer .NET framework
            // 2. The event interface name conflicts with a normal interface
            //
            // In this case the problem can be easily fixed with a global refactoring, so I wouldn't worry about that
            //
            if (m_converterInfo.GenerateClassInterfaces)
            {
                CreateClassInterfaces();
            }

            //
            // Generate the remaining types except coclass
            // Because during creating coclass, we require every type, including all the referenced type to be created
            // This is a restriction of reflection API that when you override a method in parent interface, the method info
            // is needed so the type must be already created and loaded
            //
            List<TypeInfo> coclassList = new List<TypeInfo>();
            int nCount = tlb.GetTypeInfoCount();
            for (int n = 0; n < nCount; ++n)
            {
                TypeInfo type = null;
                try
                {
                    type = tlb.GetTypeInfo(n);
                    string strType = type.GetDocumentation();

                    TypeInfo typeToProcess;
                    TypeAttr attrToProcess;

                    using (TypeAttr attr = type.GetTypeAttr())
                    {
                        TypeLibTypes.Interop.TYPEKIND kind = attr.typekind;
                        if (kind == TypeLibTypes.Interop.TYPEKIND.TKIND_ALIAS)
                        {
                            ConvCommon.ResolveAlias(type, attr.tdescAlias, out typeToProcess, out attrToProcess);
                            if (attrToProcess.typekind == TypeLibTypes.Interop.TYPEKIND.TKIND_ALIAS)
                            {
                                continue;
                            }
                            else
                            {
                                // We need to duplicate the definition of the user defined type in the name of the alias
                                kind = attrToProcess.typekind;
                                typeToProcess = type;
                                attrToProcess = attr;                                
                            }
                        }
                        else
                        {
                            typeToProcess = type;
                            attrToProcess = attr;
                        }

                        switch (kind)
                        {
                            // Process coclass later because of reflection API requirements
                            case TypeLibTypes.Interop.TYPEKIND.TKIND_COCLASS:
                                coclassList.Add(typeToProcess);
                                break;

                            case TypeLibTypes.Interop.TYPEKIND.TKIND_ENUM:
                                m_converterInfo.GetEnum(typeToProcess, attrToProcess);
                                break;

                            case TypeLibTypes.Interop.TYPEKIND.TKIND_DISPATCH:
                            case TypeLibTypes.Interop.TYPEKIND.TKIND_INTERFACE:
                                m_converterInfo.GetInterface(typeToProcess, attrToProcess);
                                break;

                            case TypeLibTypes.Interop.TYPEKIND.TKIND_MODULE:
                                m_converterInfo.GetModule(typeToProcess, attrToProcess);
                                break;

                            case TypeLibTypes.Interop.TYPEKIND.TKIND_RECORD:
                                m_converterInfo.GetStruct(typeToProcess, attrToProcess);
                                break;
                            case TypeLibTypes.Interop.TYPEKIND.TKIND_UNION:
                                m_converterInfo.GetUnion(typeToProcess, attrToProcess);
                                break;
                        }

                        m_converterInfo.ReportEvent(
                            MessageCode.Msg_TypeInfoImported,
                            Resource.FormatString("Msg_TypeInfoImported", typeToProcess.GetDocumentation()));
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    throw; // Fatal failure. Throw
                }
                catch (TlbImpResolveRefFailWrapperException)
                {
                    throw; // Fatal failure. Throw
                }
                catch (TlbImpGeneralException)
                {
                    throw; // Fatal failure. Throw
                }
                catch (TypeLoadException)
                {
                    throw; // TypeLoadException is critical. Throw.
                }
                catch (Exception)
                {
                }
            }

            // Process coclass after processing all the other types
            foreach (TypeInfo type in coclassList)
            {
                using (TypeAttr attr = type.GetTypeAttr())
                {
                    try
                    {
                        m_converterInfo.GetCoClass(type, attr);
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        throw; // Fatal failure. Throw
                    }
                    catch (TlbImpResolveRefFailWrapperException)
                    {
                        throw; // Fatal failure. Throw
                    }
                    catch (TlbImpGeneralException)
                    {
                        throw; // Fatal failure. Throw
                    }
                    catch (TypeLoadException)
                    {
                        throw; // TypeLoadException is critical. Throw.
                    }
                    catch (Exception)
                    {                        
                    }
                }
            }

            //
            // Build an array of EventItfInfo & generate event provider / event sink helpers
            //

            Event.TCEAdapterGenerator eventAdapterGenerator = new Event.TCEAdapterGenerator();
            List<Event.EventItfInfo> eventItfList = new List<Event.EventItfInfo>();

            foreach (IConvBase symbol in m_converterInfo.GetAllConvBase)
            {
                IConvInterface convInterface = symbol as IConvInterface;
                if (convInterface != null)
                {
                    if (convInterface.EventInterface != null)
                    {
                        Debug.Assert(convInterface.EventInterface is ConvEventInterfaceLocal);
                        ConvEventInterfaceLocal local = convInterface.EventInterface as ConvEventInterfaceLocal;

                        Type eventInterfaceType = convInterface.EventInterface.ManagedType;

                        // Build EventItfInfo and add to the list
                        Type sourceInterfaceType = convInterface.ManagedType;
                        string sourceInterfaceName = sourceInterfaceType.FullName;
                        Event.EventItfInfo eventItfInfo = new Event.EventItfInfo(
                            eventInterfaceType.FullName,
                            sourceInterfaceName,
                            local.EventProviderName,
                            eventInterfaceType,
                            convInterface.ManagedType);
                        eventItfList.Add(eventItfInfo);
                    }
                }
            }

            eventAdapterGenerator.Process(m_moduleBuilder, eventItfList);

            return m_assemblyBuilder;
        }

        private Assembly ReflectionOnlyResolveAsmEvent(Object sender, ResolveEventArgs args)
        {
            string asmName = AppDomain.CurrentDomain.ApplyPolicy(args.Name);
            return Assembly.ReflectionOnlyLoad(args.Name);
        }

        AssemblyBuilder m_assemblyBuilder;
        ModuleBuilder m_moduleBuilder;
        System.Runtime.InteropServices.ITypeLibImporterNotifySink m_resolver;
        ConverterInfo m_converterInfo;
    }
}
