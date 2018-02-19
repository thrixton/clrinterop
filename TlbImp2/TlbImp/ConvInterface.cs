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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using TypeLibTypes.Interop;
using System.Runtime.InteropServices;

namespace tlbimp2
{
    /// <summary>
    /// Conversion from a local ITypeInfo to a interface
    /// </summary>
    interface IConvInterface : IConvBase
    {
        /// <summary>
        /// Returns InterfaceMemberInfo collection
        /// Represents all the members for this interface used to create methods/properties
        /// </summary>
        IEnumerable<InterfaceMemberInfo> AllMembers
        {
            get;
        }

        /// <summary>
        /// Corresponding event interface
        /// </summary>
        IConvEventInterface EventInterface
        {
            get;
        }

        /// <summary>
        /// Whether this interface supports dispatch
        /// </summary>
        bool SupportsDispatch
        {
            get;
        }

        /// <summary>
        /// Whether this interface implements IEnumerable
        /// </summary>
        bool ImplementsIEnumerable
        {
            get;
        }

        /// <summary>
        /// Get event interface instance for the current interface, if it is a source interface.
        /// However we cannot determine if the interface is a source interface or not, so we should make sure
        /// we don't call this function for non-source interfaces
        /// Note: after calling this function, the event interface is not created yet, you'll need to call IConvEventInterface->Create
        /// to create it
        /// </summary>
        IConvEventInterface DefineEventInterface();

        /// <summary>
        /// Associates this interface exclusively with a class interface for a particular coclass.
        /// This means this interface is only being exposed by one particular coclass
        /// </summary>
        void AssociateWithExclusiveClassInterface(IConvClassInterface convClassInterface);
    }

    /// <summary>
    /// Type of the interface member, either a method or a variable
    /// </summary>
    enum InterfaceMemberType
    {
        Method,                     // This interface member is a method, either a regular method or a property method
        Variable                    // This interface member is a variable, also called dispatch property
    }

    /// <summary>
    /// Member information for a certain interface. Inmutable, except for the name part
    /// </summary>
    class InterfaceMemberInfo
    {
        public InterfaceMemberInfo(ConverterInfo info, TypeInfo typeInfo, int index, string basicName, string originalName, InterfaceMemberType type, TypeLibTypes.Interop.INVOKEKIND kind, int memId, FuncDesc funcDesc, VarDesc varDesc)
        {
            m_typeInfo = typeInfo;
            m_basicName = basicName;
            m_recommendedName = originalName;
            m_uniqueName = originalName;
            m_memberType = type;
            m_invokeKind = kind;
            m_memId = memId;
            m_dispId = memId;
            m_refFuncDesc = funcDesc;
            m_refVarDesc = varDesc;
            m_index = index;

            //
            // Support for Guid_DispIdOverrid
            //
            m_dispIdIsOverriden = ConvCommon.GetOverrideDispId(info, typeInfo, index, m_memberType, ref m_dispId, true);
        }

        /// <summary>
        /// Recommended member name for this interface (not for coclass).
        /// </summary>
        public string RecommendedName
        {
            get
            {
                return m_recommendedName;
            }
        }

        /// <summary>
        /// Unique member name for this interface. Will be updated when creating a interface. 
        /// </summary>
        public string UniqueName
        {
            get
            {
                return m_uniqueName;
            }
        }

        /// <summary>
        /// Valid FuncDesc if MemberType == Method, otherwise null
        /// </summary>
        public FuncDesc RefFuncDesc
        {
            get
            {
                return m_refFuncDesc;
            }
        }

        /// <summary>
        /// Valid VarDesc if MemberType == Var, otherwise null
        /// </summary>
        public VarDesc RefVarDesc
        {
            get
            {
                return m_refVarDesc;
            }
        }

        /// <summary>
        /// A function or a variable
        /// </summary>
        public InterfaceMemberType MemberType
        {
            get
            {
                return m_memberType;
            }
        }

        /// <summary>
        /// Whether the DispId has been overridden by GUID_DispIdOverride or not
        /// </summary>
        public bool DispIdIsOverridden
        {
            get
            {
                return m_dispIdIsOverriden;
            }
        }

        /// <summary>
        /// InvokeKind = Func/PropGet/PropPut/PropPutRef
        /// </summary>
        public TypeLibTypes.Interop.INVOKEKIND InvokeKind
        {
            get
            {
                return m_invokeKind;
            }
        }

        /// <summary>
        /// Member id = Dispatch Id
        /// </summary>
        public int MemId
        {
            get
            {
                return m_memId;
            }
        }

        /// <summary>
        /// Dispatch ID. Could be override by GUID_DispIdOverride. Otherwise = MemId
        /// </summary>
        public int DispId
        { 
            get 
            { 
                return m_dispId; 
            } 
        }

        public bool IsProperty
        {
            get
            {
                return m_invokeKind != TypeLibTypes.Interop.INVOKEKIND.INVOKE_FUNC;
            }
        }

        public bool IsPropertyGet
        {
            get
            {
                return (m_invokeKind & TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYGET) != 0;
            }
        }

        public bool IsPropertyPut
        {
            get
            {
                return (m_invokeKind & TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYPUT) != 0;
            }
        }

        public bool IsPropertyPutRef
        {
            get
            {
                return (m_invokeKind & TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYPUTREF) != 0;
            }
        }

        public InterfacePropertyInfo PropertyInfo
        {
            get { return m_propertyInfo; }
        }

        public TypeInfo RefTypeInfo
        {
            get { return m_typeInfo; }
        }

        /// <summary>
        /// Index of the member info. Same as the index of the FuncDesc/VarDesc which is used to call certain type lib APIs
        /// such as GetFuncCustData/GetVarCustData
        /// </summary>
        public int Index
        {
            get { return m_index; }
        }

        /// <summary>
        /// The name probably isn't unique at first, later we'll make it unique during method creation process
        /// </summary>
        /// <param name="uniqueName">The new unique name</param>
        public void UpdateUniqueName(string uniqueName)
        {
            m_uniqueName = uniqueName;
        }

        /// <summary>
        /// Builds a list of members acoording to TypeInfo information
        /// </summary>
        /// <param name="type">The TypeInfo used for generating the member list</param>
        public static List<InterfaceMemberInfo> BuildMemberList(ConverterInfo info, TypeInfo type, IConvBase convBase, bool implementsIEnumerable)
        {
            List<InterfaceMemberInfo> allMembers = new List<InterfaceMemberInfo>();

            using (TypeAttr attr = type.GetTypeAttr())
            {
                Dictionary<string, TypeLibTypes.Interop.INVOKEKIND> propertyInvokeKinds = new Dictionary<string, TypeLibTypes.Interop.INVOKEKIND>();

                //
                // 1. Walk through all the propput/propget/propref properties and collect information
                //
                for (int i = ConvCommon.GetIndexOfFirstMethod(type, attr); i < attr.cFuncs; ++i)
                {
                    using (FuncDesc funcDesc = type.GetFuncDesc(i))
                    {
                        TypeLibTypes.Interop.INVOKEKIND invKind = funcDesc.invkind;
                        if (invKind == TypeLibTypes.Interop.INVOKEKIND.INVOKE_FUNC) continue;

                        string name = type.GetDocumentation(funcDesc.memid);
                        if (!propertyInvokeKinds.ContainsKey(name))
                            propertyInvokeKinds.Add(name, (TypeLibTypes.Interop.INVOKEKIND)0);

                        TypeLibTypes.Interop.INVOKEKIND totalInvokeKind = propertyInvokeKinds[name];
                        totalInvokeKind |= invKind;
                        propertyInvokeKinds[name] = totalInvokeKind;
                    }
                }

                Dictionary<string, int> allNames = new Dictionary<string, int>();
                SortedDictionary<int, InterfacePropertyInfo> propertyList = new SortedDictionary<int, InterfacePropertyInfo>();

                //
                // 2. Walk through all vars (for disp interface) and generate name for get/set accessors
                // Fortunately we don't need to consider name collision here because variables are always first
                //
                for (int i = 0; i < attr.cVars; ++i)
                {
                    VarDesc varDesc = type.GetVarDesc(i);

                    int memid = varDesc.memid;

                    string name = type.GetDocumentation(memid);
                    string getFuncName, setFuncName;

                    bool isNewEnumMember = false;

                    if (ConvCommon.IsNewEnumDispatchProperty(info, type, varDesc, i))
                    {
                        name = getFuncName = setFuncName = "GetEnumerator";
                        isNewEnumMember = true;
                    }
                    else
                    {
                        getFuncName = "get_" + name;
                        setFuncName = "set_" + name;
                    }

                    InterfaceMemberInfo memberInfo =
                        new InterfaceMemberInfo(info, type, i, name, getFuncName, InterfaceMemberType.Variable, TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYGET, memid, null, varDesc);
                    allNames.Add(memberInfo.RecommendedName, memid);
                    allMembers.Add(memberInfo);
                    SetPropertyInfo(propertyList, memberInfo, PropertyKind.VarProperty);


                    if (!varDesc.IsReadOnly &&  // don't generate set_XXX if the var is read-only
                        !isNewEnumMember        // don't generate set_XXX if the var is actually a new enum property
                        )
                    {
                        memberInfo = new InterfaceMemberInfo(info, type, i, name, setFuncName, InterfaceMemberType.Variable, TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYPUT, memid, null, varDesc);
                        allNames.Add(memberInfo.RecommendedName, memid);
                        allMembers.Add(memberInfo);
                        if (memberInfo.IsProperty)
                            SetPropertyInfo(propertyList, memberInfo, PropertyKind.VarProperty);
                    }
                }

                //
                // 3. Walk through all funcdesc and generate unique name
                //
                bool allowNewEnum = !implementsIEnumerable;
                for (int i = ConvCommon.GetIndexOfFirstMethod(type, attr); i < attr.cFuncs; ++i)
                {
                    FuncDesc funcDesc = type.GetFuncDesc(i);
                    int memid = funcDesc.memid;

                    TypeLibTypes.Interop.INVOKEKIND invKind = funcDesc.invkind;

                    bool explicitManagedNameUsed = false;
                    bool isNewEnumMember = false;                    

                    string basicName = type.GetDocumentation(memid);

                    if (allowNewEnum && ConvCommon.IsNewEnumFunc(info, type, funcDesc, i))
                    {
                        basicName = "GetEnumerator";
                        allowNewEnum = false;
                        isNewEnumMember = true;

                        // To prevent additional methods from implementing the NewEnum method
                        implementsIEnumerable = false;
                    }
                    else
                    {
                        string managedName = type.GetFuncCustData(i, CustomAttributeGuids.GUID_ManagedName) as string;
                        if (managedName != null)
                        {
                            basicName = managedName;
                            explicitManagedNameUsed = true;
                        }
                    }

                    //
                    // First, check whether GUID_Function2Getter is set
                    //
                    bool functionToGetter = false;
                    if (type.GetFuncCustData(i, CustomAttributeGuids.GUID_Function2Getter) != null)
                    {
                        functionToGetter = true;
                    }

                    // secondly, check for the propget and propset custom attributes if this not already a property.
                    if ((invKind & (TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYGET | TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYPUT | TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYPUTREF)) == 0)
                    {
                        if (type.GetFuncCustData(i, CustomAttributeGuids.GUID_PropGetCA) != null)
                        {
                            invKind = TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYGET;
                        }
                        else
                        {
                            if (type.GetFuncCustData(i, CustomAttributeGuids.GUID_PropPutCA) != null)
                            {
                                invKind = TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYPUT;
                            }
                        }
                    }

                    //
                    // Generate the name and take the kind of property into account (get/set/let)
                    //
                    string name = null;
                    if (!explicitManagedNameUsed && !isNewEnumMember && !functionToGetter)
                    {
                        switch (invKind)
                        {
                            case TypeLibTypes.Interop.INVOKEKIND.INVOKE_FUNC:
                                name = basicName;
                                break;
                            case TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYGET:     // [propget]
                                name = "get_" + basicName;
                                break;
                            case TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYPUT:     // [propput]
                                {
                                    if (!propertyInvokeKinds.ContainsKey(basicName))
                                        propertyInvokeKinds.Add(basicName, (TypeLibTypes.Interop.INVOKEKIND)0);
                                    if ((propertyInvokeKinds[basicName] & TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYPUTREF) != 0)
                                        name = "let_" + basicName;
                                    else
                                        name = "set_" + basicName;
                                    break;
                                }
                            case TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYPUTREF:  // [propputref]
                                name = "set_" + basicName;
                                break;
                        }
                    }
                    else
                    {
                        // If explicit managed name is used, use the original name
                        invKind = TypeLibTypes.Interop.INVOKEKIND.INVOKE_FUNC;
                        name = basicName;
                    }

                    //
                    // Reset to original name if collision occurs and don't treat this as a property
                    //
                    if (allNames.ContainsKey(name))
                    {
                        name = basicName;

                        // Force it to be a normal function to align with TlbImp behavior and reduce test cost
                        // This also makes sense because if we both have set_Prop & also a Prop marked with propput, something is wrong
                        invKind = TypeLibTypes.Interop.INVOKEKIND.INVOKE_FUNC;
                    }
                    else
                        allNames.Add(name, memid);

                    //
                    // Create the memberinfo
                    //
                    InterfaceMemberInfo memberInfo =
                        new InterfaceMemberInfo(info, type, i, basicName, name, InterfaceMemberType.Method, invKind, memid, funcDesc, null);
                    allMembers.Add(memberInfo);

                    //
                    // Fill in properties
                    //
                    if (memberInfo.IsProperty)
                        SetPropertyInfo(propertyList, memberInfo, PropertyKind.FunctionProperty);
                }

                //
                // 4. Walk through all the properties and determine property type
                //
                foreach (InterfacePropertyInfo funcPropInfo in propertyList.Values)
                {
                    if (!funcPropInfo.DeterminePropType(info, convBase))
                    {
                        //
                        // Determine prop type failed. Meaning that this is not a valid property
                        //

                        if (funcPropInfo.Get != null)
                        {
                            InterfaceMemberInfo memInfo = funcPropInfo.Get;
                            // Keep the property information around so that we can emit a warning later
                            // memInfo.m_propertyInfo = null;

                            memInfo.m_invokeKind = TypeLibTypes.Interop.INVOKEKIND.INVOKE_FUNC;
                            memInfo.m_recommendedName = memInfo.m_uniqueName = memInfo.m_basicName;
                        }

                        if (funcPropInfo.Put != null)
                        {
                            InterfaceMemberInfo memInfo = funcPropInfo.Put;
                            // Keep the property information around so that we can emit a warning later
                            // memInfo.m_propertyInfo = null;
                            memInfo.m_invokeKind = TypeLibTypes.Interop.INVOKEKIND.INVOKE_FUNC;
                            memInfo.m_recommendedName = memInfo.m_uniqueName = memInfo.m_basicName;
                        }

                        if (funcPropInfo.PutRef != null)
                        {
                            InterfaceMemberInfo memInfo = funcPropInfo.PutRef;
                            // Keep the property information around so that we can emit a warning later
                            // memInfo.m_propertyInfo = null;
                            memInfo.m_invokeKind = TypeLibTypes.Interop.INVOKEKIND.INVOKE_FUNC;
                            memInfo.m_recommendedName = memInfo.m_uniqueName = memInfo.m_basicName;
                        }

                    }
                }

                propertyList.Clear();

                //
                // 5. Sort member list according to v-table (for non-dispinterfaces)
                //
                if (!attr.IsDispatch)
                {
                    allMembers.Sort(s_comparer);
                }
            }

            return allMembers;
        }

        private static InterfaceMemberInfoComparer s_comparer = new InterfaceMemberInfoComparer();

        class InterfaceMemberInfoComparer : IComparer<InterfaceMemberInfo>
        {
            #region IComparer<InterfaceMemberInfo> Members

            public int Compare(InterfaceMemberInfo x, InterfaceMemberInfo y)
            {
                Debug.Assert(x.RefFuncDesc != null);
                Debug.Assert(y.RefFuncDesc != null);

                return (x.RefFuncDesc.oVft - y.RefFuncDesc.oVft);
            }

            #endregion
        }

        private static void SetPropertyInfo(SortedDictionary<int, InterfacePropertyInfo> propertyList, InterfaceMemberInfo memberInfo, PropertyKind kind)
        {
            InterfacePropertyInfo funcPropInfo;
            if (!propertyList.ContainsKey(memberInfo.MemId))
                propertyList.Add(memberInfo.MemId, new InterfacePropertyInfo(memberInfo.RefTypeInfo, kind));

            funcPropInfo = propertyList[memberInfo.MemId];
            switch (memberInfo.InvokeKind)
            {
                case TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYGET:
                    funcPropInfo.SetGetMethod(memberInfo);
                    memberInfo.m_propertyInfo = funcPropInfo;
                    break;

                case TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYPUT:
                    funcPropInfo.SetPutMethod(memberInfo);
                    memberInfo.m_propertyInfo = funcPropInfo;
                    break;

                case TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYPUTREF:
                    funcPropInfo.SetPutRefMethod(memberInfo);
                    memberInfo.m_propertyInfo = funcPropInfo;
                    break;
            }

        }

        #region Private members

        string m_basicName;                                     // Non-decorated name
        string m_recommendedName;                               // Recommended name, generated from basic name
        string m_uniqueName;                                    // Unique member name. Generated from original name
        FuncDesc m_refFuncDesc;                                 // Corresponding FuncDesc, if this MemberInfo is a function (property)
        VarDesc m_refVarDesc;                                   // Corresponding VarDesc, if this MemberInfo is a dispatch property
        InterfaceMemberType m_memberType;                       
        TypeLibTypes.Interop.INVOKEKIND m_invokeKind;                                
        int m_memId;                                            // Member ID specified using [id]
        private int m_dispId;                                   // Usually = MemberID unless explicitly overridden with GUID_DispIdOverride
        private InterfacePropertyInfo m_propertyInfo;
        private TypeInfo m_typeInfo;
        private int m_index;
        private bool m_dispIdIsOverriden;                       // Whether the dispid is overridden by Guid_DispIdOverride

        #endregion
    }

    /// <summary>
    /// Kind of the property, either function or variable
    /// </summary>
    enum PropertyKind { FunctionProperty, VarProperty }

    /// <summary>
    /// Represents a property (either from a function or a variable) in a interface
    /// </summary>
    class InterfacePropertyInfo
    {
        public InterfacePropertyInfo(TypeInfo typeInfo, PropertyKind propKind)
        {
            m_typeInfo = typeInfo;
            m_propertyKind = propKind;
        }

        /// <summary>
        /// InterfaceMemberInfo for propget
        /// </summary>
        public InterfaceMemberInfo Get
        {
            get { return m_memInfoGet; }
        }

        /// <summary>
        /// InterfaceMemberInfo for propput
        /// </summary>
        public InterfaceMemberInfo Put
        {
            get { return m_memInfoPut; }
        }

        /// <summary>
        /// InterfaceMemberInfo for propputref
        /// </summary>
        public InterfaceMemberInfo PutRef
        {
            get { return m_memInfoPutRef; }
        }

        /// <summary>
        /// RecommendedName of the property. Not unique
        /// </summary>
        public string RecommendedName
        {
            get { return m_typeInfo.GetDocumentation(MemId); }
        }

        /// <summary>
        /// MemId
        /// </summary>
        public int MemId
        {
            get { return m_bestMemInfo.MemId; }        
        }

        /// <summary>
        /// Dispatch id. Could be overridden
        /// </summary>
        public int DispId
        {
            get { return m_bestMemInfo.DispId; }
        }

        /// <summary>
        /// The best InterfaceMemberInfo that can represent this property. Used to determine many properties of this property
        /// </summary>
        public InterfaceMemberInfo BestMemberInfo
        {
            get { return m_bestMemInfo; }
        }

        /// <summary>
        /// The best FuncDesc that can represent this property. Used to determine the exact type of the property
        /// </summary>
        public FuncDesc BestFuncDesc
        {
            get
            {
                Debug.Assert(m_propertyKind == PropertyKind.FunctionProperty);
                return m_bestMemInfo.RefFuncDesc;
            }
        }

        /// <summary>
        /// The best VarDesc that can represent this property. Used to determine the exact type of the property
        /// </summary>
        public VarDesc BestVarDesc
        {
            get
            {
                Debug.Assert(m_propertyKind == PropertyKind.VarProperty);
                return m_bestMemInfo.RefVarDesc;
            }
        }

        /// <summary>
        /// Kind of the property. Either function or variable
        /// </summary>
        public PropertyKind Kind
        {
            get { return m_propertyKind; }
        }

        /// <summary>
        /// TypeInfo that this memberinfo belongs to
        /// </summary>
        public TypeInfo RefTypeInfo
        {
            get { return m_typeInfo; }
        }

        /// <summary>
        /// Is this property has a invalid getter that doesn't have a valid return value?
        /// </summary>
        public bool HasInvalidGetter
        {
            get { return m_hasInvalidGetter; }
        }

        /// <summary>
        /// Associate self with the memberinfo as propget
        /// </summary>
        public void SetGetMethod(InterfaceMemberInfo memberInfo)
        {
            m_memInfoGet = memberInfo;
        }

        /// <summary>
        /// Associate self with the memberinfo as propput
        /// </summary>
        public void SetPutMethod(InterfaceMemberInfo memberInfo)
        {
            m_memInfoPut = memberInfo;
        }

        /// <summary>
        /// Associate self with the memberinfo as propputref
        /// </summary>
        public void SetPutRefMethod(InterfaceMemberInfo memberInfo)
        {
            m_memInfoPutRef = memberInfo;
        }

        /// <summary>
        /// Determine the best representing InterfaceMemberInfo for this property so that we can know the exact type
        /// of the property later
        /// </summary>
        /// <param name="convBase">
        /// Corresponding IConvBase that this property belongs to
        /// </param>
        /// <returns>True if the property is valid, false if the property is invalid</returns>        
        public bool DeterminePropType(ConverterInfo info, IConvBase convBase)
        {
            Debug.Assert(m_bestMemInfo == null);

            // propget is the best
            if (m_memInfoGet != null && m_bestMemInfo == null)
                m_bestMemInfo = m_memInfoGet;

            // otherwise try propput
            if (m_memInfoPut != null && m_bestMemInfo == null)
                m_bestMemInfo = m_memInfoPut;

            // otherwise we'll have to use propputref
            if (m_memInfoPutRef != null && m_bestMemInfo == null)
                m_bestMemInfo = m_memInfoPutRef;

            Debug.Assert(m_bestMemInfo != null);

            if (m_propertyKind == PropertyKind.FunctionProperty)
            {
                m_conversionType = ConversionType.ReturnValue;

                // 1. Find the best FUNCDESC for functions
                Debug.Assert(m_propertyTypeDesc == null);

                FuncDesc bestFuncDesc = m_bestMemInfo.RefFuncDesc;

                // 2. Determine the type of the property
                // Need to use m_memInfoGet instead of IsPropertyGet because of Guid_PropGetCA
                if (m_memInfoGet != null)
                {
                    // find the last [retval] for non-dispatch interfaces or if transform:dispret is specified
                    // for dispatch interfaces, the return value is the real return value
                    if (!bestFuncDesc.IsDispatch || info.TransformDispRetVal)
                    {
                        for (int i = bestFuncDesc.cParams - 1; i >= 0; --i)
                        {
                            ElemDesc elemDesc = bestFuncDesc.GetElemDesc(i);
                            if (elemDesc.paramdesc.IsRetval)
                            {
                                m_propertyTypeDesc = elemDesc.tdesc;
                                m_conversionType = ConversionType.ParamRetVal;
                            }
                        }
                    }

                    // if no [retval], check return type (must not be VT_VOID/VT_HRESULT)
                    TypeDesc retTypeDesc = bestFuncDesc.elemdescFunc.tdesc;
                    if (m_propertyTypeDesc == null &&
                        retTypeDesc.vt != (int)VarEnum.VT_VOID && retTypeDesc.vt != (int)VarEnum.VT_HRESULT)
                    {
                        m_propertyTypeDesc = retTypeDesc;
                        m_conversionType = ConversionType.ReturnValue;
                    }

                    // Don't use VT_VOID
                    if (m_propertyTypeDesc != null && m_propertyTypeDesc.vt == (int)VarEnum.VT_VOID)
                        m_propertyTypeDesc = null;

                    if (m_propertyTypeDesc == null)
                    {
                        m_hasInvalidGetter = true;
                        return false;
                    }
                }
                else
                {
                    if (bestFuncDesc.cParams < 1)
                        return false;

                    // It is possible to write a PROPERTYPUT with [retval], in this case we just ignore it because
                    // if we convert it to a property, it is impossible for C#/VB code to get the return value
                    ElemDesc elemDesc = bestFuncDesc.GetElemDesc(bestFuncDesc.cParams - 1);

                    if (bestFuncDesc.IsDispatch && !info.TransformDispRetVal)
                    {
                        // Skip retval check for dispatch functions while TransformDispRetVal is false
                    }
                    else if (elemDesc.paramdesc.IsRetval)
                    {
                        // RetVal. This is not a valid property
                        return false;
                    }

                    m_propertyTypeDesc = elemDesc.tdesc;
                    // It is a parameter, but it will be the type of the property, so we use ConversionType.ReturnValue
                    m_conversionType = ConversionType.ReturnValue;

                    Debug.Assert(m_propertyTypeDesc != null, "Cannot determine the type for property!");
                }
            }
            else
            {
                m_propertyTypeDesc = m_bestMemInfo.RefVarDesc.elemdescVar.tdesc;
            }

            return true;
        }

        /// <summary>
        /// Get the TypeDesc that represents the property type
        /// </summary>
        public TypeDesc PropertyTypeDesc
        {
            get
            {
                return m_propertyTypeDesc;
            }
        }

        /// <summary>
        /// Get the type converter for the property
        /// </summary>
        public TypeConverter GetPropertyTypeConverter(ConverterInfo info, TypeInfo typeInfo)
        {
            if (m_propertyKind == PropertyKind.FunctionProperty)
                return new TypeConverter(info, typeInfo, m_propertyTypeDesc, m_conversionType);
            else
                return new TypeConverter(info, typeInfo, m_propertyTypeDesc, ConversionType.ReturnValue);
        }

        #region Private members

        private InterfaceMemberInfo m_memInfoGet;
        private InterfaceMemberInfo m_memInfoPut;
        private InterfaceMemberInfo m_memInfoPutRef;
        private TypeDesc m_propertyTypeDesc;
        private InterfaceMemberInfo m_bestMemInfo;
        private ConversionType m_conversionType;
        private PropertyKind m_propertyKind;
        private TypeInfo m_typeInfo;
        private bool m_hasInvalidGetter;

        #endregion
    }

    /// <summary>
    /// Performs the conversion of an ITypeInfo representing an interface to a managed interface.
    /// </summary>
    class ConvInterfaceLocal : ConvLocalBase, IConvInterface
    {
        public ConvInterfaceLocal(ConverterInfo info, TypeInfo type)
        {
            DefineType(info, type, false);
        }

        private TypeLibTypes.Interop.TYPEFLAGS HandleDualIntf()
        {
            TypeInfo typeInfo = RefNonAliasedTypeInfo;

            // I finally figured this part out
            // For dual interfaces, it has a "funky" TKIND_DISPATCH|TKIND_DUAL interface with a parter of TKIND_INTERFACE|TKIND_DUAL interface
            // The first one is pretty bad and has duplicated all the interface members of its parent, which is not we want
            // We want the second v-table interface
            // So, if we indeed has seen this kind of interface, prefer its partner
            // However, we should not blindly get the partner because those two interfaces partners with each other
            // So we need to first test to see if the interface is both dispatch & dual, and then get its partner interface
            using (TypeAttr typeAttr = typeInfo.GetTypeAttr())
            {
                TypeLibTypes.Interop.TYPEFLAGS typeFlags = typeAttr.wTypeFlags;

                if (typeAttr.IsDual)
                {
                    if (typeAttr.IsDispatch)
                    {
                        // The passed interface is the original dual interface
                        TypeInfo refTypeInfo = typeInfo.GetRefTypeNoComThrow();
                        if (refTypeInfo != null)
                            typeInfo = refTypeInfo;
                    }
                    else
                    {
                        // We are dealing with the 'funky' interface, get the original one
                        TypeInfo refTypeInfo = typeInfo.GetRefTypeNoComThrow();
                        using (TypeAttr dualTypeAttr = refTypeInfo.GetTypeAttr())
                        {
                            typeFlags = dualTypeAttr.wTypeFlags;
                        }
                    }
                }

                ReInit(RefTypeInfo, ConvCommon.GetAlias(typeInfo));

                // Prefer the type flag in the alias
                using (TypeAttr refTypeAttr = RefTypeInfo.GetTypeAttr())
                {
                    if (refTypeAttr.IsAlias)
                    {
                        return refTypeAttr.wTypeFlags;
                    }
                }

                return typeFlags;
            }
        }

        protected override void OnDefineType()
        {
            TypeLibTypes.Interop.TYPEFLAGS typeFlags = HandleDualIntf();

            TypeInfo typeInfo = RefNonAliasedTypeInfo;
            using (TypeAttr typeAttr = typeInfo.GetTypeAttr())
            {
                // A interface that has v-table support must be derived from IUnknown
                if (typeAttr.IsInterface || (typeAttr.IsDispatch || typeAttr.IsDual))
                {
                    
                    if (!ConvCommon.IsDerivedFromIUnknown(typeInfo) && typeAttr.Guid != WellKnownGuids.IID_IUnknown)
                    {
                        string msg = Resource.FormatString("Wrn_NotIUnknown", typeInfo.GetDocumentation());
                        m_info.ReportEvent(WarningCode.Wrn_NotIUnknown, msg);

                        return;
                    }

                    // Dual but not derived from IDispatch, continue anyway
                    if (typeAttr.IsDual && !ConvCommon.IsDerivedFromIDispatch(typeInfo))
                    {
                        string msg = Resource.FormatString("Wrn_DualNotDispatch", typeInfo.GetDocumentation());
                        m_info.ReportEvent(WarningCode.Wrn_DualNotDispatch, msg);
                    }
                }

                m_parentInterface = null;
                m_parentInterfaceTypeInfo = null;
                string interfacename = m_info.GetUniqueManagedName(RefTypeInfo, ConvType.Interface);
                Type typeParent = null;

                m_implementsIEnumerable = ConvCommon.ExplicitlyImplementsIEnumerable(typeInfo, typeAttr);

                List<Type> implTypeList = new List<Type>();

                if (typeAttr.cImplTypes == 1)
                {
                    TypeInfo parent = typeInfo.GetRefType(0);
                    using (TypeAttr parentAttr = parent.GetTypeAttr())
                    {
                        // Are we derived from something besides IUnknown?
                        if (WellKnownGuids.IID_IUnknown != parentAttr.Guid && WellKnownGuids.IID_IDispatch != parentAttr.Guid)
                        {
                            IConvBase convBase = m_info.GetTypeRef(ConvType.Interface, parent);
                            m_parentInterface = convBase as IConvInterface;
                            Debug.Assert(m_parentInterface != null);

                            ConvCommon.ThrowIfImplementingExportedClassInterface(RefTypeInfo, m_parentInterface);

                            m_parentInterfaceTypeInfo = parent;
                            typeParent = m_parentInterface.RealManagedType;
                            implTypeList.Add(typeParent);
                        }
                    }
                }

                // If this interface has a NewEnum member but doesn't derive from IEnumerable directly, 
                // then have it implement IEnumerable.
                if (!m_implementsIEnumerable && ConvCommon.HasNewEnumMember(m_info, typeInfo, interfacename))
                    implTypeList.Add(typeof(System.Collections.IEnumerable));

                m_typeBuilder = m_info.ModuleBuilder.DefineType(
                    interfacename,
                    TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.AnsiClass | TypeAttributes.Abstract | TypeAttributes.Import,
                        null,
                        implTypeList.ToArray());

                // Handled [Guid(...)] custom attribute
                ConvCommon.DefineGuid(RefTypeInfo, RefNonAliasedTypeInfo, m_typeBuilder);

                // Handle [TypeLibType(...)] if evaluate to non-0
                if (typeFlags != 0)
                    m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForTypeLibType((TypeLibTypeFlags)typeFlags));

                // Dual is the default
                m_supportsDispatch = true;
                if (typeAttr.IsDual)
                {
                    if (!ConvCommon.IsDerivedFromIDispatch(typeInfo))
                    {
                        // OK. Now we have a dual interface that doesn't derive from IDispatch. Treat it like IUnknown interface but still emit dispid
                        m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForInterfaceType(ComInterfaceType.InterfaceIsIUnknown));
                    }                    
                }
                else
                {

                    // Handles [InterfaceType(...)] custom attribute
                    if (typeAttr.typekind == TypeLibTypes.Interop.TYPEKIND.TKIND_DISPATCH)
                    {
                        m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForInterfaceType(ComInterfaceType.InterfaceIsIDispatch));
                    }
                    else
                    {
                        // This is done to align with old TlbImp behavior
                        if (!ConvCommon.IsDerivedFromIDispatch(typeInfo))
                            m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForInterfaceType(ComInterfaceType.InterfaceIsIUnknown));

                        m_supportsDispatch = false;
                    }
                }

                m_info.AddToSymbolTable(RefTypeInfo, ConvType.Interface, this);
                m_info.RegisterType(m_typeBuilder, this);

                m_allMembers = InterfaceMemberInfo.BuildMemberList(m_info, typeInfo, this, m_implementsIEnumerable);
            }
        }

        #region IConvInterface Members

        public IEnumerable<InterfaceMemberInfo> AllMembers
        {
            get { return m_allMembers.ToArray(); }
        }

        public IConvEventInterface EventInterface
        {
            get { return m_convEventInterfaceLocal; }
        }

        public bool SupportsDispatch
        {
            get { return m_supportsDispatch; }
        }

        public bool ImplementsIEnumerable
        {
            get { return m_implementsIEnumerable; }
        }

        /// <summary>
        /// Get event interface instance for the current interface, if it is a source interface.
        /// However we cannot determine if the interface is a source interface or not, so we should make sure
        /// we don't call this function for non-source interfaces
        /// Note: after calling this function, the event interface is not created yet, you'll need to call IConvEventInterface->Create
        /// to create it
        /// </summary>
        public IConvEventInterface DefineEventInterface()
        {
            if (m_convEventInterfaceLocal == null)
            {
                m_convEventInterfaceLocal = new ConvEventInterfaceLocal(this, m_info);
            }

            return m_convEventInterfaceLocal;
        }

        public void AssociateWithExclusiveClassInterface(IConvClassInterface convClassInterface)
        {
            Debug.Assert(m_classInterface == null);
            m_classInterface = convClassInterface;
        }

        #endregion

        #region IConvBase Members

        public override Type ManagedType
        {
            get
            {
                if (m_classInterface != null)
                    return m_classInterface.ManagedType;
                else
                    return RealManagedType;
            }
        }

        public override ConvType ConvType
        {
            get { return ConvType.Interface; }
        }

        /// <summary>
        /// Creates the managed interface
        /// </summary>
        public override void OnCreate()
        {
            if (m_typeBuilder == null)
            {
                // Something went wrong in the conversion process. Probably the interface is not IUnknown derived
                return;
            }

            if (m_type != null)
                return;

            TypeInfo typeInfo = RefNonAliasedTypeInfo;
            using (TypeAttr typeAttr = typeInfo.GetTypeAttr())
            {
                // If this interface derives from another interface, the interface must be created
                // I create this interface first so that if anything fails in the next step we still would have something
                if (m_parentInterface != null)
                {
                    using (TypeAttr parentAttr = m_parentInterfaceTypeInfo.GetTypeAttr())
                    {
                        m_parentInterface.Create();
                    }
                }

                // Create interface
                InterfaceInfo interfaceInfo = new InterfaceInfo(m_info, m_typeBuilder, m_supportsDispatch, typeInfo, typeAttr, false, false);
                ConvCommon.CreateInterfaceCommon(interfaceInfo);

                //
                // Emit ComConversionLoss if necessary
                //
                if (interfaceInfo.IsConversionLoss)
                    m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForComConversionLoss());

                m_type = m_typeBuilder.CreateType();
            }
        }

        #endregion

        #region Private members

        bool m_supportsDispatch;                                    // Whether this interface supports IDispatch. Could be dual or dispinterface
        private IConvClassInterface m_classInterface;               // Reference to this interface will be replaced with class interface
                                                                    // Only happens when there are no other coclass exposing this interface as default
        private List<InterfaceMemberInfo> m_allMembers;             // List of members. Used for method creation
        private IConvInterface m_parentInterface;                   // ConvInterface for parent interface. Used to create parent interface
        private TypeInfo m_parentInterfaceTypeInfo;                 // TypeInfo for parent interface. Used to create parent interface
        private ConvEventInterfaceLocal m_convEventInterfaceLocal;  // Corresponding event interface. Null if doesn't exist
        private bool m_implementsIEnumerable;                       // Whether this interface explicitly implements IEnumerable

        #endregion
    }

    /// <summary>
    /// Represents external interface that is already been created
    /// </summary>
    class ConvInterfaceExternal : IConvInterface
    {
        public ConvInterfaceExternal(ConverterInfo info, TypeInfo typeInfo, Type managedType, ConverterAssemblyInfo converterAssemblyInfo)
        {
            m_info = info;
            m_typeInfo = typeInfo;

            m_nonAliasedTypeInfo = ConvCommon.GetAlias(typeInfo);

            using (TypeAttr typeAttr = m_nonAliasedTypeInfo.GetTypeAttr())
            {
                if (typeAttr.IsDual)
                {
                    if (typeAttr.IsDispatch)
                    {
                        // The passed interface is the original dual interface
                        TypeInfo refTypeInfo = typeInfo.GetRefTypeNoComThrow();
                        if (refTypeInfo != null)
                            m_nonAliasedTypeInfo = refTypeInfo;
                    }
                }
            }

            m_managedType = managedType;
            using (TypeAttr attr = m_nonAliasedTypeInfo.GetTypeAttr())
            {
                m_bSupportsDispatch = attr.IsDispatch;
                m_implementsIEnumerable = ConvCommon.ExplicitlyImplementsIEnumerable(m_nonAliasedTypeInfo, attr);
            }

            info.AddToSymbolTable(typeInfo, ConvType.Interface, this);
            info.RegisterType(managedType, this);

            //
            // Special support for external class interface
            // It is possible that this external interface is a "exclusive" default interface in a coclass
            // In this case we need to find the coclass and resolve the coclass so that default interface -> class interface
            // conversion can happen
            //             
            TypeInfo coclassTypeInfo;
            if (converterAssemblyInfo.ClassInterfaceMap.GetCoClassForExclusiveDefaultInterface(typeInfo, out coclassTypeInfo))
            {
                // Figure out the class interface managed type. It should be the same name as the TypeInfo, unless there is Guid_ManagedName
                string classInterfaceName = info.GetManagedName(coclassTypeInfo, converterAssemblyInfo.Namespace);
                Type classInterfaceType = managedType.Assembly.GetType(classInterfaceName);
                if (classInterfaceType == null)
                {
                    throw new TlbImpGeneralException(
                        Resource.FormatString("Err_CanotFindReferencedType", classInterfaceName, converterAssemblyInfo.Assembly.FullName),
                        ErrorCode.Err_CanotFindReferencedType);
                }
            
                new ConvClassInterfaceExternal(
                    info,
                    coclassTypeInfo,
                    classInterfaceType,
                    converterAssemblyInfo);
            }
        }

        #region IConvInterface Members

        public IConvEventInterface EventInterface
        {
            get
            {
                // CoClass could refer to a external source interface
                return m_convEventInterfaceLocal;
            }
        }

        public IEnumerable<InterfaceMemberInfo> AllMembers
        {
            get
            {
                if (m_allMembers == null)
                {
                    m_allMembers = InterfaceMemberInfo.BuildMemberList(
                        m_info,
                        m_nonAliasedTypeInfo,
                        this,
                        m_implementsIEnumerable
                        );
                }

                return m_allMembers;
            }
        }

        public void AssociateWithExclusiveClassInterface(IConvClassInterface convClassInterface)
        {
            m_classInterface = convClassInterface;
        }

        public bool SupportsDispatch
        {
            get
            {
                return m_bSupportsDispatch;
            }
        }

        public void Create()
        {
            // Do nothing
        }

        public IConvEventInterface DefineEventInterface()
        {
            // We need to create a new event interface for external interface if necessary, instead of referencing the existing one
            // TlbImpv1 already does that. So we don't need to change this behavior
            if (m_convEventInterfaceLocal == null)
            {
                m_convEventInterfaceLocal = new ConvEventInterfaceLocal(this, m_info);
            }

            return m_convEventInterfaceLocal;
        }

        public bool ImplementsIEnumerable
        {
            get { return m_implementsIEnumerable; }
        }

        #endregion

        #region IConvBase Members

        public TypeInfo RefTypeInfo
        {
            get { return m_typeInfo; }
        }

        public TypeInfo RefNonAliasedTypeInfo
        {
            get { return m_typeInfo; }
        }

        public Type ManagedType
        {
            get
            {
                if (m_classInterface != null)
                    return m_classInterface.ManagedType;
                else
                    return RealManagedType;
            }
        }

        public Type RealManagedType
        {
            get { return m_managedType; }
        }

        public ConvType ConvType
        {
            get { return ConvType.Interface; }
        }

        public string ManagedName
        {
            get { return m_managedType.FullName; }
        }

        public ConvScope ConvScope
        {
            get { return ConvScope.External; }
        }

        #endregion

        #region Private members

        private ConverterInfo m_info;                               
        private TypeInfo m_typeInfo;                                // Unmanaged type
        private TypeInfo m_nonAliasedTypeInfo;                      // Type info that is not an alias
        private Type m_managedType;                                 // Managed type
        private List<InterfaceMemberInfo> m_allMembers;             // Member list
        private bool m_bSupportsDispatch;                           // Whether this interface supports IDispatch. Could be dual or dispinterface
        private ConvEventInterfaceLocal m_convEventInterfaceLocal;  // Corresponding event interface. Always generated instead of refering to a external one
        private bool m_implementsIEnumerable;                       // Whether this interface explicitly implements IEnumerabe
        private IConvClassInterface m_classInterface;

        #endregion
    }
}
