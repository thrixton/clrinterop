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
using System.Diagnostics;
using System.Runtime.InteropServices;
using CoreRuleEngine;
using TypeLibTypes.Interop;
using TlbImpRuleEngine;
using FormattedOutput;

namespace tlbimp2
{
    // Used to represent a TypeLibrary and the interop assembly for it
    class ConverterAssemblyInfo
    {
        public ConverterAssemblyInfo(ConverterInfo info, Assembly assembly, TypeLib typeLib)
        {
            m_assembly = assembly;
            m_typeLib = typeLib;
            m_info = info;

            m_classInterfaceMap = new ClassInterfaceMap(m_typeLib);

            // Try GUID_ManagedName
            m_tlbNamespace = m_info.GetCustomNamespaceForTypeLib(typeLib);
            if (m_tlbNamespace == null)
            {
                // Try to use the namespace of the first type
                Type[] types = assembly.GetTypes();
                if (types.Length > 0)
                    m_tlbNamespace = types[0].Namespace;
            }

            // Otherwise use the type library name
            if (m_tlbNamespace == null)
                m_tlbNamespace = m_typeLib.GetDocumentation();
        }

        private const char NestedTypeSeparator = '+';

        public Type ResolveType(TypeInfo typeInfo, ConvType convType, out string expectedName)
        {
            // This is our best guess            
            string managedName = m_info.GetRecommendedManagedName(typeInfo, convType, m_tlbNamespace);
            int separatorPos = managedName.IndexOf(NestedTypeSeparator);
            expectedName = managedName;

            // If there is a '+' and it is neither the first or the last
            if (separatorPos > 0 && separatorPos < managedName.Length - 1)
            {
                string parentName = managedName.Substring(0, separatorPos);
                Type parentType = m_assembly.GetType(parentName);
                return parentType.GetNestedType(managedName.Substring(separatorPos + 1));
            }
            else
            {
                return m_assembly.GetType(managedName);
            }
        }

        public ClassInterfaceMap ClassInterfaceMap
        {
            get
            {
                return m_classInterfaceMap;
            }
        }

        public string Namespace
        {
            get
            {
                return m_tlbNamespace;
            }
        }

        public Assembly Assembly
        {
            get
            {
                return m_assembly;
            }
        }

        private Assembly m_assembly;
        private TypeLib m_typeLib;
        private ConverterInfo m_info;
        private ClassInterfaceMap m_classInterfaceMap;
        private string m_tlbNamespace;
    }

    struct ConverterSettings
    {
        public bool m_isGenerateClassInterfaces;
        public string m_namespace;
        public TypeLibImporterFlags m_flags;
        public bool m_isVersion2;
        public bool m_isPreserveSig;
        public RuleSet m_ruleSet;
    }

    // The class that is the heart of the ITypeInfo to managed type conversion process.
    class ConverterInfo
    {
        public ConverterInfo(ModuleBuilder moduleBuilder, TypeLib typeLib, System.Runtime.InteropServices.ITypeLibImporterNotifySink resolver, ConverterSettings settings)
        {
            m_moduleBuilder = moduleBuilder;
            m_typeLib = typeLib;
            m_resolver = resolver;

            using (TypeLibAttr attr = m_typeLib.GetLibAttr())
            {
                m_libid = attr.guid;
            }

            m_typeLibMappingTable = new Dictionary<Guid, ConverterAssemblyInfo>();
            m_symbolTable = new Dictionary<string,IConvBase>();
            m_settings = settings;
            m_memberTables = new Dictionary<string, MemberTable>();
            m_typeTable = new Hashtable();
            m_bTransformDispRetVal = (settings.m_flags & TypeLibImporterFlags.TransformDispRetVals) != 0;
            m_defaultMemberTable = new Dictionary<TypeBuilder, bool>();

            BuildGlobalNameTable();
        }


        // Used to resolve a given type reference (params, fields, etc.)
        public IConvBase GetTypeRef(ConvType convType, TypeInfo type)
        {
            IConvBase ret = null;
            if (!ResolveInternal(type, convType, out ret))
            {
                switch (convType)
                {
                    case ConvType.Enum:
                        ret = new ConvEnumLocal(this, type);
                        break;
                    case ConvType.Interface:
                        ret = new ConvInterfaceLocal(this, type);
                        break;
                    case ConvType.Struct:
                        ret = new ConvStructLocal(this, type);
                        break;
                    case ConvType.Union:
                        ret = new ConvUnionLocal(this, type);
                        break;
                    case ConvType.CoClass:
                        ret = new ConvCoClassLocal(this, type);
                        break;

                    case ConvType.Module:
                        ret = new ConvModuleLocal(this, type);
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            Debug.Assert(ret.ConvType == convType);

            return ret;
        }
        // Helpers to perform actual definitions for types.
        public IConvBase GetInterface(TypeInfo type, TypeAttr attr)
        {
            IConvInterface convInterface = (IConvInterface)GetTypeRef(ConvType.Interface, type);
            convInterface.Create();
            return convInterface;
        }
       
        public IConvBase GetStruct(TypeInfo type, TypeAttr attr)
        {
            IConvStruct convStruct = (IConvStruct)GetTypeRef(ConvType.Struct, type);
            convStruct.Create();
            return convStruct;
        }
        public IConvBase GetUnion(TypeInfo type, TypeAttr attr)
        {
            IConvUnion convUnion = (IConvUnion)GetTypeRef(ConvType.Union, type);
            convUnion.Create();
            return convUnion;
        }
        public IConvBase GetEnum(TypeInfo type, TypeAttr attr)
        {
            IConvEnum convEnum = (IConvEnum)GetTypeRef(ConvType.Enum, type);
            convEnum.Create();
            return convEnum;
        }
        public IConvBase GetCoClass(TypeInfo type, TypeAttr attr)
        {
            IConvCoClass convCoClass = (IConvCoClass)GetTypeRef(ConvType.CoClass, type);
            convCoClass.Create();
            return convCoClass;
        }

        public IConvBase GetModule(TypeInfo type, TypeAttr attr)
        {
            IConvModule convModule = (IConvModule)GetTypeRef(ConvType.Module, type);
            convModule.Create();
            return convModule;
        }

        #region Global type name collision

        private struct GlobalNameEntry : IComparable<GlobalNameEntry>
        {
            public GlobalNameEntry(string _typeName, ConvType _convType)
            {
                typeName = _typeName;
                convType = _convType;
            }

            public string typeName;
            public ConvType convType;

            #region IComparable<GlobalNameEntry> Members

            public int CompareTo(GlobalNameEntry other)
            {
                if (typeName == other.typeName)
                    return convType.CompareTo(other.convType);
                else
                    return typeName.CompareTo(other.typeName);                        
            }

            #endregion
        }

        /// <summary>
        /// Build global name table for existing types in the type library
        /// The reason we need to put those names into the table first is that 
        /// their name should not be changed because they come from existing types
        /// </summary>
        private void BuildGlobalNameTable()
        {
            m_globalNameTable = new SortedDictionary<string,string>();
            m_globalManagedNames = new SortedDictionary<string, string>();

            //
            // Traverse each type and add the names into the type. Their names are already in the type library
            // and should not be changed
            //
            int nCount = m_typeLib.GetTypeInfoCount();
            for (int n = 0; n < nCount; ++n)
            {
                try
                {
                    TypeInfo type = m_typeLib.GetTypeInfo(n);
                    using (TypeAttr attr = type.GetTypeAttr())
                    {
                        switch (attr.typekind)
                        {
                            case TypeLibTypes.Interop.TYPEKIND.TKIND_COCLASS:
                                // class interface use the original name of the coclass, and coclass use the generated name XXXClass
                                GetUniqueManagedName(type, ConvType.ClassInterface);
                                break;

                            case TypeLibTypes.Interop.TYPEKIND.TKIND_INTERFACE:
                                GetUniqueManagedName(type, ConvType.Interface);
                                break;

                            case TypeLibTypes.Interop.TYPEKIND.TKIND_RECORD:
                                GetUniqueManagedName(type, ConvType.Struct);
                                break;

                            case TypeLibTypes.Interop.TYPEKIND.TKIND_UNION:
                                GetUniqueManagedName(type, ConvType.Union);
                                break;

                            case TypeLibTypes.Interop.TYPEKIND.TKIND_ENUM:
                                GetUniqueManagedName(type, ConvType.Enum);
                                break;

                            case TypeLibTypes.Interop.TYPEKIND.TKIND_MODULE:
                                GetUniqueManagedName(type, ConvType.Module);
                                break;


                        }
                    }
                }
                catch (TlbImpInvalidTypeConversionException)
                {
                    // Swallow this exception. Usually it is caused by duplicated managed name which we can definitely ignore this
                }
            }
        }

        /// <summary>
        /// Get the unqiue managed name for the ITypeInfo & ConvType
        /// Can be called multiple times as it will cache the entries in a internal table
        /// Note that this is for global types only (not members)
        /// </summary>
        /// <returns>The unique name</returns>
        public string GetUniqueManagedName(TypeInfo type, ConvType convType)
        {
            string recommendedName = GetRecommendedManagedName(type, convType);
            string internalName = GetInternalEncodedManagedName(type, convType);
            string managedName;
            if (!m_globalNameTable.TryGetValue(internalName, out managedName))
            {
                managedName = recommendedName;
                if (convType == ConvType.EventInterface || convType == ConvType.CoClass)
                {
                    // We generate unique type names by add "_<n" for EventInterface/CoClass
                    // because these names are "forged"
                    int index = 2;
                    while (m_globalManagedNames.ContainsKey(managedName))
                        managedName = recommendedName + "_" + index.ToString();
                }
                else
                {
                    // Duplicated custom managed name needs an warning
                    if (m_globalManagedNames.ContainsKey(recommendedName))
                    {
                        ReportEvent(
                            WarningCode.Wrn_DuplicateTypeName,
                            Resource.FormatString("Wrn_DuplicateTypeName", recommendedName));

                        throw new TlbImpInvalidTypeConversionException(type);
                    }
                }

                m_globalNameTable.Add(internalName, managedName);
                m_globalManagedNames.Add(managedName, managedName);
            }

            return managedName;
        }

        /// <summary>
        /// Get the unique type name for types that doesn't fall into one of the ConvTypesm, such as event delegates.
        /// Cannot be called multiple times. Multiple calls for the same name will generate multiple name entries
        /// </summary>
        /// <param name="name">The recommended name for delegate</param>
        /// <returns>The unique name</returns>
        public string GetUniqueManagedName(string name)
        {
            string managedName = name;
            int index = 2;
            while (m_globalManagedNames.ContainsKey(managedName))
                managedName = name + "_" + index.ToString();

            m_globalManagedNames.Add(managedName, managedName);

            return managedName;
        }

        /// <summary>
        /// Check if this type has managed name (GUID_ManagedName)
        /// </summary>
        /// <param name="type">The type to be checked</param>
        /// <returns>The managed name, or null if the GUID_ManagedNamea doesn't exist</returns>
        private string GetCustomManagedName(TypeInfo type)
        {
            //
            // Support GUID_ManagedName
            //
            return type.GetCustData(CustomAttributeGuids.GUID_ManagedName) as string;
        }

        public string GetCustomNamespaceForTypeLib(TypeLib typeLib)
        {
            //
            // Support for GUID_ManagedName (for namespace)
            // Favor the custom name over everything else including /namespace option
            //
            string customManagedNamespace = typeLib.GetCustData(CustomAttributeGuids.GUID_ManagedName) as string;
            if (customManagedNamespace != null)
            {
                customManagedNamespace = customManagedNamespace.Trim();
                if (customManagedNamespace.ToUpper().EndsWith(".DLL"))
                    customManagedNamespace = customManagedNamespace.Substring(0, customManagedNamespace.Length - 4);
                else if (customManagedNamespace.ToUpper().EndsWith(".EXE"))
                    customManagedNamespace = customManagedNamespace.Substring(0, customManagedNamespace.Length - 4);

                return customManagedNamespace;
            }

            return null;
        }

        /// <summary>
        /// Gets the namespace for the type lib
        /// </summary>
        private string GetNamespaceForTypeLib(TypeInfo type, TypeLib typeLib)
        {
            using (TypeLibAttr attr = typeLib.GetLibAttr())
            {
                string tlbNamespace = GetCustomNamespaceForTypeLib(typeLib);
                if (tlbNamespace != null)
                    return tlbNamespace;

                if (attr.guid == m_libid)
                    tlbNamespace = m_settings.m_namespace;
                else
                {
                    tlbNamespace = typeLib.GetDocumentation();

                    if (tlbNamespace.IndexOfAny(m_invalidChars) >= 0)
                    {
                        string tlbFilePath;
                        Guid tlbGuid = attr.guid;
                        int hr = TypeLib.QueryPathOfRegTypeLib(ref tlbGuid, (ushort)attr.wMajorVerNum, (ushort)attr.wMinorVerNum, (int)attr.lcid, out tlbFilePath);

                        ReportEvent(
                            WarningCode.Wrn_InvalidNamespace,
                            Resource.FormatString("Wrn_InvalidNamespace", tlbFilePath, tlbNamespace));

                        throw new TlbImpInvalidTypeConversionException(type);
                    }
                }

                return tlbNamespace;
            }
        }

        /// <summary>
        /// Gets the generated managed name from the namespace & typeinfo name
        /// </summary>
        private string GetGeneratedManagedName(TypeInfo type, bool useDefaultNamespace)
        {
            string docName = type.GetDocumentation();

            //
            // Figure out assembly namespace (using best guess)
            //
            string tlbNamespace;
            if (useDefaultNamespace)
            {
                tlbNamespace = GetNamespaceForTypeLib(type, m_typeLib);
            }
            else
            {
                TypeLib typeLib = type.GetContainingTypeLib();
                tlbNamespace = GetNamespaceForTypeLib(type, typeLib);
            }

            if (string.IsNullOrEmpty(tlbNamespace))
                return docName;
            else
                return tlbNamespace + "." + docName;
        }

        /// <summary>
        /// Gets the generated managed name from the namespace & typeinfo name
        /// </summary>
        private string GetGeneratedManagedName(TypeInfo type, string theNamespace)
        {
            string docName = type.GetDocumentation();

            if (string.IsNullOrEmpty(theNamespace))
                return docName;
            else
                return theNamespace + "." + docName;
        }

        public string GetManagedName(TypeInfo type)
        {
            return GetManagedName(type, false);
        }

        /// <summary>
        /// Returns the managed name, which is namespace + type name
        /// Guid_ManagedName is also considered
        /// </summary>
        /// <param name="type">The ITypeInfo you want to get managed name</param>
        /// <param name="useDefaultNamespace">Whether you want to force use of the default namespace (from the importing type lib)</param>
        public string GetManagedName(TypeInfo type, bool useDefaultNamespace)
        {
            string name = GetCustomManagedName(type);
            if (name != null) return name;

            return GetGeneratedManagedName(type, useDefaultNamespace);
        }

        public string GetManagedName(TypeInfo type, string theNamespace)
        {
            string name = GetCustomManagedName(type);
            if (name != null) return name;

            return GetGeneratedManagedName(type, theNamespace);
        }

        public string GetRecommendedManagedName(TypeInfo type, ConvType convType)
        {
            return GetRecommendedManagedName(type, convType, false);
        }

        /// <summary>
        /// Get recommended managed name according to information obtained from ITypeInfo. 
        /// Doesn't guarantee that the name is unique. Can be called multiple times
        /// GUID_ManagedName is also considered
        /// </summary>
        /// <returns>The recommended name</returns>
        public string GetRecommendedManagedName(TypeInfo type, ConvType convType, bool useDefaultNamespace)
        {
            if (convType == ConvType.EventInterface)
            {
                //
                // Special treatment for event interfaces
                // For coclass that are referring to external source interfaces, we can define the event interfaces
                // in local assemblies and the namespace will always be the namespace of the importing type lib
                //
                string tlbNamespace = GetNamespaceForTypeLib(type, m_typeLib);

                string docName = type.GetDocumentation() + "_Event";

                if (string.IsNullOrEmpty(tlbNamespace))
                    return docName;
                else
                    return tlbNamespace + "." + docName;                
            }
            else
            {
                string name = GetManagedName(type, useDefaultNamespace);

                // Rule Engine
                string changeNameActionResult = GetChangeManagedNameActionResult(type, convType, name);
                if (convType == ConvType.CoClass)
                    changeNameActionResult = changeNameActionResult + "Class";
                return changeNameActionResult;
            }
        }

        private string GetChangeManagedNameActionResult(TypeInfo typeInfo,
            ConvType convType, string oldName)
        {
            if (Settings.m_ruleSet != null)
            {
                ICategory category = TypeCategory.GetInstance();
                TypeInfoMatchTarget target = null;
                using (TypeAttr attr = typeInfo.GetTypeAttr())
                {
                    TypeLibTypes.Interop.TYPEKIND kind = attr.typekind;
                    target = new TypeInfoMatchTarget(typeInfo.GetContainingTypeLib(), typeInfo, kind);
                }
                AbstractActionManager actionManager = RuleEngine.GetActionManager();
                List<Rule> changeNameRules = Settings.m_ruleSet.GetRule(
                    category, ChangeManagedNameActionDef.GetInstance(), target);
                if (changeNameRules.Count != 0)
                {
                    if (changeNameRules.Count > 1)
                    {
                        Output.WriteWarning(Resource.FormatString("Wrn_RuleMultipleMatch",
                                            ChangeManagedNameActionDef.GetInstance()),
                            WarningCode.Wrn_RuleMultipleMatch);
                    }
                    Rule changeNameRule = changeNameRules[changeNameRules.Count - 1];
                    int namespaceSplit = oldName.LastIndexOf('.');
                    string oldNamespace = "";
                    if (namespaceSplit != -1)
                        oldNamespace = oldName.Substring(0, namespaceSplit + 1);
                    return oldNamespace + (changeNameRule.Action as ChangeManagedNameAction).NewName;
                }
            }
            return oldName;
        }

        public string GetRecommendedManagedName(TypeInfo type, ConvType convType, string theNamespace)
        {
            if (convType == ConvType.EventInterface)
            {
                return GetRecommendedManagedName(type, convType, false);
            }
            else
            {
                string name = GetManagedName(type, theNamespace);

                if (convType == ConvType.CoClass)
                    return name + "Class";
                else
                    return name;
            }
        }
        
#endregion

        private bool ResolveInternal(TypeInfo type, ConvType convType, out IConvBase convBase)
        {
            IConvBase ret = null;

            // See if it is already mapped
            if (!m_symbolTable.TryGetValue(GetInternalEncodedManagedName(type, convType), out ret))
            {
                TypeLib typeLib = type.GetContainingTypeLib();
                Guid libid;
                using (TypeLibAttr libAttr = typeLib.GetLibAttr())
                {
                    libid = libAttr.guid;
                }
                // See if this is defined in a different type library
                if(libid != m_libid)
                {
                    ConverterAssemblyInfo converterAssemblyInfo = null;
                    // See if we have not already imported this assembly
                    if (!m_typeLibMappingTable.TryGetValue(libid, out converterAssemblyInfo))
                    {
                        Assembly assembly = null;
                        string asmName = typeLib.GetCustData(CustomAttributeGuids.GUID_ExportedFromComPlus) as string;
                        if (asmName != null)
                        {
                            try
                            {
                                assembly = Assembly.ReflectionOnlyLoad(asmName);
                            }
                            catch (Exception)
                            {
                            }
                        }

                        if (assembly == null)
                        {
                            try
                            {
                                assembly = m_resolver.ResolveRef(typeLib.GetTypeLib());
                            }
                            catch (TlbImpResolveRefFailWrapperException)
                            {
                                // Avoid wrapping wrapper with wrapper exception
                                throw;
                            }
                            catch (Exception ex)
                            {
                                throw new TlbImpResolveRefFailWrapperException(ex);
                            }
                        }

                        if (assembly == null)
                        {
                            // null means that the resolver has failed and we should skip this failure and continue with the next type
                            throw new TlbImpInvalidTypeConversionException(type);
                        }

                        converterAssemblyInfo = new ConverterAssemblyInfo(this, assembly, typeLib);
                        m_typeLibMappingTable.Add(libid, converterAssemblyInfo);
                    }
                    string expectedName;
                    Type convertedType = converterAssemblyInfo.ResolveType(type, convType, out expectedName);
                    
                    if (convertedType == null)
                    {
                        throw new TlbImpGeneralException(
                            Resource.FormatString("Err_CanotFindReferencedType", expectedName, converterAssemblyInfo.Assembly.FullName),
                            ErrorCode.Err_CanotFindReferencedType);
                    }
                    else
                    {
                        // Create external IConvBase instance
                        switch (convType)
                        {
                            case ConvType.Interface :
                                ret = new ConvInterfaceExternal(this, type, convertedType, converterAssemblyInfo);
                                break;

                            case ConvType.Enum :
                                ret = new ConvEnumExternal(this, type, convertedType);
                                break;

                            case ConvType.Struct :
                                ret = new ConvStructExternal(this, type, convertedType);
                                break;

                            case ConvType.Union :
                                ret = new ConvUnionExternal(this, type, convertedType);
                                break;

                            case ConvType.ClassInterface :
                                Debug.Assert(false, "Only ConvCoClassExternal can create ConvClassInterfaceExternal");
                                break;

                            case ConvType.EventInterface :
                                Debug.Assert(false, "We should not reference a external event interface!");
                                break;

                            case ConvType.CoClass:
                                ret = new ConvCoClassExternal(this, type, convertedType, converterAssemblyInfo);
                                break;
                        }
                    }
                }
            }
            convBase = ret;
            return ret != null;
        }

        /// <summary>
        /// Maintain a list of members (method, event, property)
        /// Used to generate unique member name for a particular type
        /// </summary>
        class MemberTable
        {
            public bool HasDuplicateMember(string name, Type[] paramTypes, MemberTypes memberType)
            {
                if (paramTypes == null) paramTypes = new Type[] {};

                // The rule is : there should be no method/property/event that has the same name
                // except that methods can have the same name but different signature, and cannot have the same
                // name as other property/events
                switch (memberType)
                {
                    case MemberTypes.Method:
                        if (HasMethodWithParam(name, paramTypes) || HasEvent(name) || HasProperty(name))
                            return true;
                        else
                        {
                            List<Type[]> paramTypeArrays;
                            if (m_methods.ContainsKey(name))
                                paramTypeArrays = m_methods[name];
                            else
                            {
                                paramTypeArrays = new List<Type[]>();
                                m_methods.Add(name, paramTypeArrays);
                            }

                            paramTypeArrays.Add(paramTypes);
                        }
                        break;

                    case MemberTypes.Property :
                        if (HasProperty(name) || HasEvent(name) || HasMethod(name))
                            return true;
                        else
                            m_properties.Add(name, true);
                        break;

                    case MemberTypes.Event :
                        if (HasEvent(name) || HasProperty(name) || HasMethod(name))
                            return true;
                        else
                            m_events.Add(name, true);
                        break;

                    default:
                        Debug.Assert(false, "Should not get here!");
                        break;
                }

                return false;
            }

            private bool HasMethodWithParam(string name, Type[] paramTypes)
            {
                if (!m_methods.ContainsKey(name)) return false;
                
                List<Type[]> paramTypesArray = m_methods[name];
                foreach (Type[] paramTypesInArray in paramTypesArray)
                {
                    if (paramTypesInArray.Length != paramTypes.Length) continue;    

                    bool match = true;
                    for (int i = 0; i < paramTypesInArray.Length; ++i)
                    {
                        if (paramTypesInArray[i] != paramTypes[i])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                        return true;
                }

                return false;
            }

            private bool HasMethod(string name)
            {
                return m_methods.ContainsKey(name);
            }

            private bool HasEvent(string name)
            {
                return m_events.ContainsKey(name);
            }

            private bool HasProperty(string name)
            {
                return m_properties.ContainsKey(name);
            }

            private Dictionary<string, List<Type[]>> m_methods = new Dictionary<string,List<Type[]>>();
            private Dictionary<string, bool> m_events = new Dictionary<string, bool>();
            private Dictionary<string, bool> m_properties = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Check whether a type builder has a member with the specified name & parameter type
        /// </summary>
        public bool HasDuplicateMemberName(TypeBuilder builder, string name, Type[] paramType, MemberTypes memberType)
        {
            // Retrieve the member name table
            MemberTable memberTable = null;
            if (m_memberTables.ContainsKey(builder.FullName))
                memberTable = m_memberTables[builder.FullName];
            else
            {
                memberTable = new MemberTable();
                m_memberTables.Add(builder.FullName, memberTable);
            }

            return memberTable.HasDuplicateMember(name, paramType, memberType);
        }

        public bool GenerateClassInterfaces { get { return m_settings.m_isGenerateClassInterfaces; } }

        /// <summary>
        /// Register the type so that later it could be looked-up to find the corresonding IConvBase
        /// It could be a TypeBuilder or a real Type
        /// </summary>
        /// <param name="type">The type to register</param>
        /// <param name="convBase">The IConvBase to register</param>
        public void RegisterType(Type type, IConvBase convBase)
        {
            // Note: I found out that in some cases that there will be hash code collision between
            // TypeBuilder & RuntimeType. So use name instead
            if (!m_typeTable.ContainsKey(type.FullName))
                m_typeTable.Add(type.FullName, convBase);    
        }

        /// <summary>
        /// Look up the runtime type and returns IConvBase. It could be a TypeBuilder or a real Type
        /// </summary>
        /// <param name="type">The type to lookup</param>
        /// <returns>IConvBase instance if found</returns>
        public IConvBase LookupType(Type type)
        {
            if (m_typeTable.ContainsKey(type.FullName))
                return (m_typeTable[type.FullName] as IConvBase);
            else
                return null;
        }

        /// <summary>
        /// Add to symbol table. Basically this should be called in every constructor of IConvBase derived classes
        /// A IConvBase instance should be added to the symbol type the moment it is defined so that we know a IConvBase
        /// instance is already created for this particular name
        /// </summary>
        /// <remarks>
        /// The name we use in internal symbol table is actually the name of the TypeInfo, instead of the name
        /// of the real managed type. This is no different than using the TypeInfo as the key.
        /// </remarks>
        public void AddToSymbolTable(TypeInfo typeInfo, ConvType convType, IConvBase convBase)
        {
            string name = GetInternalEncodedManagedName(typeInfo, convType);
            m_symbolTable.Add(name, convBase);
        }

        /// <summary>
        /// The name represents a (TypeInfo, ConvType) pair and is unique to a type library. Used in SymbolTable
        /// </summary>
        private string GetInternalEncodedManagedName(TypeInfo typeInfo, ConvType convType)
        {
            using (TypeLibAttr typeLibAttr = typeInfo.GetContainingTypeLib().GetLibAttr())
            {
                return typeInfo.GetDocumentation() + "[" + convType.ToString() + "," + typeLibAttr.guid + "]";
            }
        }

        public void SetDefaultMember(TypeBuilder typeBuilder, string name)
        {
            if (!m_defaultMemberTable.ContainsKey(typeBuilder))
            {
                typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForDefaultMember(name));
                m_defaultMemberTable.Add(typeBuilder, true);
            }
        }

        public void ReportEvent(WarningCode code, string eventMsg)
        {
            m_resolver.ReportEvent(ImporterEventKind.NOTIF_CONVERTWARNING, (int)code, eventMsg);
        }

        public void ReportEvent(MessageCode code, string eventMsg)
        {
            m_resolver.ReportEvent(ImporterEventKind.NOTIF_TYPECONVERTED, (int)code, eventMsg);
        }

        public ClassInterfaceMap GetClassInterfaceMap()
        {
            if (m_classInterfaceMap == null)
                m_classInterfaceMap = new ClassInterfaceMap(m_typeLib);

            return m_classInterfaceMap;
        }

        /// <summary>
        /// Return all IConvBase as IEnumerable
        /// </summary>
        public IEnumerable GetAllConvBase
        {
            get
            {
                return m_symbolTable.Values;
            }
        }

        public ConverterSettings Settings
        {
            get
            {
                return m_settings;
            }
        }

        public ModuleBuilder ModuleBuilder
        {
            get
            {
                return m_moduleBuilder;
            }
        }

        public bool TransformDispRetVal
        {
            get
            {
                return m_bTransformDispRetVal;
            }
        }

            

        // Type library currently being converted
        private TypeLib m_typeLib;
        // Library ID of m_typeLib
        private Guid m_libid;
        // Reflection Emit helper to generate types for the interop assembly
        private ModuleBuilder m_moduleBuilder;
        // Callback to client to resolve assemblies and notify of events
        private System.Runtime.InteropServices.ITypeLibImporterNotifySink m_resolver;
        // LIBID to assembly mapping table
        private Dictionary<Guid, ConverterAssemblyInfo> m_typeLibMappingTable;
        // Symbol table for type references
        private Dictionary<string, IConvBase> m_symbolTable;
        // Settings for converter
        private ConverterSettings m_settings;

        // Mapping from TypeBuilder to NameTable (Dictionary<string, Type[]>)
        // Used for generating unique member names
        private Dictionary<string, MemberTable> m_memberTables;

        // Mapping from Type/TypeBuilder to IConvBase
        private Hashtable m_typeTable;

        private SortedDictionary<string, string> m_globalNameTable;         // maps a internal name to a unique managed name 
                                                                            // so that we could find the assigned name for a existing type
        private SortedDictionary<string, string> m_globalManagedNames;      // keeps a list of all managed name for duplication check

        private readonly char[] m_invalidChars = "/\\".ToCharArray();

        private ClassInterfaceMap m_classInterfaceMap;
    
        private bool m_bTransformDispRetVal;
        private Dictionary<TypeBuilder, bool> m_defaultMemberTable;
    }
}
