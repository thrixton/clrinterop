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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using TypeLibTypes.Interop;
using CoreRuleEngine;
using TlbImpRuleEngine;
using FormattedOutput;

namespace tlbimp2
{
    /// <summary>
    /// What is the category of the type that needs to be converted?
    /// </summary>
    enum ConversionType
    {
        VarArgParameter,            // Type is a vararg parameter
        Parameter,                  // Type is a parameter
        ParamRetVal,                // Type is a paramter as [out, retval]
        Field,                      // Type is a field
        ReturnValue,                // Type is a return value (function or property)
        Element,                    // Type is a element
    }


    /// <summary>
    /// Convert unmanaged type (VT_XXX) to managed type
    /// The conversion is done at the time when you creates this object. After that this object is available
    /// for you to write custom attributes to the parameter and get the converted type
    /// </summary>
    class TypeConverter
    {
        #region Public methods

        public TypeConverter(ConverterInfo info, TypeInfo type, TypeDesc desc, ConversionType conversionType)
        {
            m_info = info;
            m_typeInfo = type;
            m_typeDesc = desc;
            m_conversionType = conversionType;

            m_paramDesc = null;
            m_attribute = null;
            m_conversionLoss = false;
            m_convertedType = null;
            m_nativeIndirections = 0;
            m_convertingNewEnumMember = false;

            ResetUnmanagedType();

            // Do the conversion
            _Convert();
        }

        public TypeConverter(ConverterInfo info, TypeInfo type, TypeDesc desc, bool convertingNewEnumMember, ConversionType conversionType)
        {
            m_info = info;
            m_typeInfo = type;
            m_typeDesc = desc;
            m_conversionType = conversionType;

            m_paramDesc = null;
            m_attribute = null;
            m_conversionLoss = false;
            m_convertedType = null;
            m_nativeIndirections = 0;
            m_convertingNewEnumMember = convertingNewEnumMember;

            ResetUnmanagedType();

            // Do the conversion
            _Convert();
        }

        public TypeConverter(ConverterInfo info, TypeInfo type, TypeDesc desc, ParamDesc paramDesc, bool convertingNewEnumMember, ConversionType conversionType)
        {
            m_info = info;
            m_typeInfo = type;
            m_typeDesc = desc;
            m_paramDesc = paramDesc;
            m_conversionType = conversionType;

            m_attribute = null;
            m_conversionLoss = false;
            m_convertedType = null;
            m_nativeIndirections = 0;
            m_convertingNewEnumMember = convertingNewEnumMember;

            ResetUnmanagedType();

            // Do the conversion
            _Convert();
        }

        /// <summary>
        /// Wrapper for a already converted type
        /// </summary>
        /// <param name="type"></param>
        public TypeConverter(Type type)
        {
            m_info = null;
            m_typeDesc = null;
            m_typeInfo = null;
            m_paramDesc = null;
            m_attribute = null;
            m_conversionLoss = false;
            m_convertedType = type;
            m_nativeIndirections = 0;

            ResetUnmanagedType();
        }

        /// <summary>
        /// Used in the customized type conversion
        /// </summary>
        /// <param name="type"></param>
        public TypeConverter(Type type, CustomAttributeBuilder attribute, ParameterAttributes parameterAttributesOverride)
        {
            m_info = null;
            m_typeDesc = null;
            m_typeInfo = null;
            m_paramDesc = null;
            m_attribute = attribute;
            m_conversionLoss = false;
            m_convertedType = type;
            m_nativeIndirections = 0;

            m_parameterAttributesOverride = parameterAttributesOverride;

            ResetUnmanagedType();
        }

        /// <summary>
        /// Apply the custom attribute to parameters
        /// </summary>
        public void ApplyAttributes(ParameterBuilder paramBuilder)
        {
            if (m_attribute != null)
                paramBuilder.SetCustomAttribute(m_attribute);

            if (m_typeInfo != null)
                ConvCommon.HandleAlias(m_info, m_typeInfo, m_typeDesc, paramBuilder);
        }

        /// <summary>
        /// Apply the custom attribute to fields
        /// </summary>
        public void ApplyAttributes(FieldBuilder fieldBuilder)
        {
            if (m_attribute != null)
                fieldBuilder.SetCustomAttribute(m_attribute);

            if (m_typeInfo != null)
                ConvCommon.HandleAlias(m_info, m_typeInfo, m_typeDesc, fieldBuilder);
        }

        /// <summary>
        /// Returns the converted type after the conversion is done
        /// </summary>
        public Type ConvertedType
        {
            get
            {
                return m_convertedType;
            }
        }

        /// <summary>
        /// Is some information lost during the conversion process?
        /// </summary>
        public bool IsConversionLoss
        {
            get
            {
                return m_conversionLoss;
            }
        }

        /// <summary>
        /// Whether we use default marshal. If true, you cannot use the public property UnmanagedType because it is invalid.
        /// </summary>
        public bool UseDefaultMarshal
        {
            get
            {
                return m_useDefaultMarshal;
            }
        }

        /// <summary>
        /// The corresponding unmanaged type. Only can be used when UseDefaultMarshal is false.
        /// </summary>
        public UnmanagedType UnmanagedType
        {
            get
            {
                return m_unmanagedType;
            }
        }

        #endregion

        #region Private functions

        private void SetUnmanagedType(UnmanagedType unmanagedType)
        {
            Debug.Assert(m_useDefaultMarshal);
            Debug.Assert(m_unmanagedType == (UnmanagedType)(-1));

            m_useDefaultMarshal = false;
            m_unmanagedType = unmanagedType;
        }

        private void ResetUnmanagedType()
        {
            m_useDefaultMarshal = true;
            m_unmanagedType = (UnmanagedType)(-1);
        }

        private void _Convert()
        {
            VarEnum vt = (VarEnum)m_typeDesc.vt;

            // Strip out VT_PTR
            while (vt == VarEnum.VT_PTR)
            {
                m_typeDesc = m_typeDesc.lptdesc;
                vt = (VarEnum)m_typeDesc.vt;
                m_nativeIndirections++;
            }

            // Strip out VT_BYREF
            if ((vt & VarEnum.VT_BYREF) != 0)
            {
                vt &= ~VarEnum.VT_BYREF;
                m_nativeIndirections++;
            }

            //
            // Find the corresponding type and save it in result and store the custom attribute in m_attribute
            //
            Type result = null;
            m_attribute = null;
            switch (vt)
            {
                case VarEnum.VT_HRESULT :
                    result = typeof(int);
                    m_attribute = CustomAttributeHelper.GetBuilderForMarshalAs(UnmanagedType.Error);
                    SetUnmanagedType(UnmanagedType.Error);
                    break;

                case VarEnum.VT_VOID:
                    result = typeof(void);
                    break;

                case VarEnum.VT_UINT:
                    result = typeof(uint);
                    break;

                case VarEnum.VT_INT:
                    result = typeof(int);
                    break;

                case VarEnum.VT_UI1:
                    result = typeof(byte);
                    break;

                case VarEnum.VT_UI2:
                    result = typeof(ushort);
                    break;

                case VarEnum.VT_UI4:
                    result = typeof(uint);
                    break;

                case VarEnum.VT_UI8:
                    result = typeof(ulong);
                    break;

                case VarEnum.VT_I1:
                    result = typeof(sbyte);
                    break;

                case VarEnum.VT_I2:
                    result = typeof(short);
                    break;

                case VarEnum.VT_I4:
                    result = typeof(int);
                    break;

                case VarEnum.VT_I8:
                    result = typeof(long);
                    break;

                case VarEnum.VT_R4:
                    result = typeof(float);
                    break;

                case VarEnum.VT_R8:
                    result = typeof(double);
                    break;

                case VarEnum.VT_ERROR :
                    result = typeof(int);
                    m_attribute = CustomAttributeHelper.GetBuilderForMarshalAs(UnmanagedType.Error);
                    SetUnmanagedType(UnmanagedType.Error);

                    break;

                case VarEnum.VT_BSTR:
                    result = typeof(string);
                    m_attribute = CustomAttributeHelper.GetBuilderForMarshalAs(UnmanagedType.BStr);
                    SetUnmanagedType(UnmanagedType.BStr);

                    // BSTR => string is special as BSTR are actually OLECHAR*, so we should add one indirection
                    m_nativeIndirections++;
                    break;

                case VarEnum.VT_DISPATCH:
                    if (m_convertingNewEnumMember)
                    {
                        // When we are creating a new enum member, convert IDispatch to IEnumVariant
                        TryUseCustomMarshaler(WellKnownGuids.IID_IEnumVARIANT, out result);
                    }
                    else
                    {
                        result = typeof(object);
                        m_attribute = CustomAttributeHelper.GetBuilderForMarshalAs(UnmanagedType.IDispatch);
                        SetUnmanagedType(UnmanagedType.IDispatch);
                    }

                    // VT_DISPATCH => IDispatch *
                    m_nativeIndirections++;

                    break;

                case VarEnum.VT_UNKNOWN:
                    if (m_convertingNewEnumMember)
                    {
                        // When we are creating a new enum member, convert IUnknown to IEnumVariant
                        TryUseCustomMarshaler(WellKnownGuids.IID_IEnumVARIANT, out result);
                    }
                    else
                    {
                        result = typeof(object);
                        m_attribute = CustomAttributeHelper.GetBuilderForMarshalAs(UnmanagedType.IUnknown);
                        SetUnmanagedType(UnmanagedType.IUnknown);
                    }

                    // VT_UNKNOWN => IUnknown *
                    m_nativeIndirections++;

                    break;

                case VarEnum.VT_LPSTR:
                    result = typeof(string);
                    m_attribute = CustomAttributeHelper.GetBuilderForMarshalAs(UnmanagedType.LPStr);
                    SetUnmanagedType(UnmanagedType.LPStr);
                    m_nativeIndirections++;
                    break;

                case VarEnum.VT_LPWSTR:
                    result = typeof(string);
                    m_attribute = CustomAttributeHelper.GetBuilderForMarshalAs(UnmanagedType.LPWStr);
                    SetUnmanagedType(UnmanagedType.LPWStr);
                    m_nativeIndirections++;
                    break;

                case VarEnum.VT_PTR:
                    Debug.Assert(false, "Should not get here");
                    break;

                case VarEnum.VT_SAFEARRAY:
                    {
                        TypeDesc arrayDesc = m_typeDesc.lpadesc.tdescElem;
                        VarEnum arrayVt = (VarEnum)arrayDesc.vt;
                        Type userDefinedType = null;
                       
                        TypeConverter elemTypeConverter = new TypeConverter(m_info, m_typeInfo, arrayDesc, ConversionType.Element);
                        Type elemType = elemTypeConverter.ConvertedType;

                        // Determine the right VT for MarshalAs attribute
                        bool pointerArray = false;
                        if (arrayVt == VarEnum.VT_PTR)
                        {
                            arrayDesc = arrayDesc.lptdesc;
                            arrayVt = (VarEnum)arrayDesc.vt;
                            pointerArray = true;

                            // We don't support marshalling pointers in array except UserType* & void*
                            if (arrayVt != VarEnum.VT_USERDEFINED && arrayVt != VarEnum.VT_VOID)
                            {
                                arrayVt = VarEnum.VT_INT;
                                m_conversionLoss = true;
                            }
                        }

                        //
                        // Emit UserDefinedSubType if necessary
                        //
                        if (arrayVt == VarEnum.VT_USERDEFINED)
                        {                           
                            if (elemType.IsEnum)
                            {
                                if (pointerArray)
                                {
                                    arrayVt = VarEnum.VT_INT;
                                    m_conversionLoss = true;
                                }
                                else
                                {
                                    // For enums, using VT_RECORD is better than VT_I4
                                    // Within the runtime, if you specify VT_I4 for enums in SafeArray, we treat it the same way as VT_RECORD
                                    // Reflection API also accepts VT_RECORD instead of VT_I4
                                    arrayVt = VarEnum.VT_RECORD;
                                }
                            }
                            else if(elemType.IsValueType)
                            {
                                if (pointerArray)
                                {
                                    arrayVt = VarEnum.VT_INT;
                                    m_conversionLoss = true;
                                }
                                else
                                    arrayVt = VarEnum.VT_RECORD;
                            }
                            else if (elemType.IsInterface)
                            {
                                if (pointerArray)
                                {
                                    // decide VT_UNKNOWN / VT_DISPATCH
                                    if (InterfaceSupportsDispatch(elemType))
                                        arrayVt = VarEnum.VT_DISPATCH;
                                    else
                                        arrayVt = VarEnum.VT_UNKNOWN;
                                }
                                else
                                {
                                    arrayVt = VarEnum.VT_INT;
                                    m_conversionLoss = true;
                                }
                            }
                            else if (elemType == typeof(object) && !elemTypeConverter.UseDefaultMarshal &&
                                     (elemTypeConverter.UnmanagedType == UnmanagedType.IUnknown))
                            {
                                // Special case for object that doesn't have default interface and will be marshalled as IUnknown
                                arrayVt = VarEnum.VT_UNKNOWN;
                            }

                            userDefinedType = elemType;
                        }

                        m_conversionLoss |= elemTypeConverter.IsConversionLoss;

                        // Transform to System.Array if /sysarray is set and not vararg
                        if (((m_info.Settings.m_flags & TypeLibImporterFlags.SafeArrayAsSystemArray) != 0) &&
                            m_conversionType != ConversionType.VarArgParameter)
                            result = typeof(System.Array);
                        else
                        {
                            result = elemType.MakeArrayType();

                            // Don't need SafeArrayUserDefinedSubType for non System.Array case
                            userDefinedType = null;
                        }

                        // TlbImp doesn't have this check for vt == VT_RECORD/VT_UNKNOWN/VT_DISPATCH therefore
                        // it will emit SafeArrayUserDefinedSubType even it is not necessary/not valid
                        // TlbImp2 will take this into account
                        if ((userDefinedType != null) && (arrayVt == VarEnum.VT_RECORD || arrayVt == VarEnum.VT_UNKNOWN || arrayVt == VarEnum.VT_DISPATCH))
                        {
                            // The name of the type would be full name in TlbImp2
                            m_attribute = CustomAttributeHelper.GetBuilderForMarshalAsSafeArrayAndUserDefinedSubType(arrayVt, userDefinedType);
                        }
                        else
                        {
                            // Use I4 for enums when SafeArrayUserDefinedSubType is not specified
                            if (elemType.IsEnum && arrayVt == VarEnum.VT_RECORD)
                                arrayVt = VarEnum.VT_I4;
                            m_attribute = CustomAttributeHelper.GetBuilderForMarshalAsSafeArray(arrayVt);
                        }

                        SetUnmanagedType(UnmanagedType.SafeArray);

                        // SafeArray <=> array is special because SafeArray is similar to Element*
                        m_nativeIndirections++;

                        break;
                    }

                case VarEnum.VT_RECORD:
                case VarEnum.VT_USERDEFINED:
                    {
                        // Handle structs, interfaces, enums, and unions

                        // Check if any ResolveTo Rule applied.
                        TypeInfo refType = m_typeInfo.GetRefTypeInfo(m_typeDesc.hreftype);
                        TypeAttr refAttr = refType.GetTypeAttr();
                        Type resolveToType;
                        if (RuleEngineResolveRedirection(m_info.Settings.m_ruleSet, refType,
                            out resolveToType))
                        {
                            result = resolveToType;
                            break;
                        }

                        // Support for aliasing
                        TypeInfo realType;
                        TypeAttr realAttr;
                        ConvCommon.ResolveAlias(m_typeInfo, m_typeDesc, out realType, out realAttr);

                        // Alias for a built-in type?
                        if (realAttr.typekind == TypeLibTypes.Interop.TYPEKIND.TKIND_ALIAS)
                        {
                            // Recurse to convert the built-in type
                            TypeConverter builtinType = new TypeConverter(m_info, realType, realAttr.tdescAlias, m_conversionType);
                            result = builtinType.ConvertedType;
                            m_attribute = builtinType.m_attribute;
                        }
                        else
                        {
                            // Otherwise, we must have a non-aliased type, and it is a user defined type
                            // We should use the TypeInfo that this TypeDesc refers to
                            realType = m_typeDesc.GetUserDefinedTypeInfo(m_typeInfo);
                            TypeLibTypes.Interop.TYPEKIND typeKind = realAttr.typekind;

                            using (realAttr = realType.GetTypeAttr())
                            {
                                TypeLib typeLib = realType.GetContainingTypeLib();

                                // Convert StdOle2.Guid to System.Guid
                                if (_IsStdOleGuid(realType))
                                {
                                    result = typeof(Guid);
                                    m_attribute = null;
                                    ResetUnmanagedType();
                                }
                                else if (realAttr.Guid == WellKnownGuids.IID_IUnknown)
                                {
                                    // Occasional goto makes sense
                                    // If VT_USERDEFINE *, and the VT_USERDEFINE is actually a VT_UNKNOWN => IUnknown *, we need to decrease the m_nativeIndirections
                                    // to compensate for the m_nativeIndirections++ in VT_UNKNOWN
                                    m_nativeIndirections--;
                                    goto case VarEnum.VT_UNKNOWN;
                                }
                                else if (realAttr.Guid == WellKnownGuids.IID_IDispatch)
                                {
                                    // Occasional goto makes sense
                                    // See the IID_IUnknown case for why we need to --
                                    m_nativeIndirections--;
                                    goto case VarEnum.VT_DISPATCH;
                                }
                                else
                                {
                                    // Need to use CustomMarshaler?
                                    Type customMarshalerResultType;
                                    if (TryUseCustomMarshaler(realAttr.Guid, out customMarshalerResultType))
                                    {
                                        result = customMarshalerResultType;
                                    }
                                    else
                                    {
                                        IConvBase ret = m_info.GetTypeRef(ConvCommon.TypeKindToConvType(typeKind), realType);

                                        if (m_conversionType == ConversionType.Field)
                                        {
                                            // Too bad. Reflection API requires that the field type must be created before creating
                                            // the struct/union type

                                            // Only process indirection = 0 case because > 1 case will be converted to IntPtr
                                            // Otherwise it will leads to a infinite recursion, if you consider the following scenario:
                                            // struct A
                                            // {
                                            //      struct B
                                            //      {
                                            //          struct A *a;
                                            //      } b;
                                            // }
                                            if (ret is ConvUnionLocal && m_nativeIndirections == 0)
                                            {
                                                ConvUnionLocal convUnion = ret as ConvUnionLocal;
                                                convUnion.Create();
                                            }
                                            else if (ret is ConvStructLocal && m_nativeIndirections == 0)
                                            {
                                                ConvStructLocal convStruct = ret as ConvStructLocal;
                                                convStruct.Create();
                                            }
                                            else if (ret is ConvEnumLocal && m_nativeIndirections == 0)
                                            {
                                                ConvEnumLocal convEnum = ret as ConvEnumLocal;
                                                convEnum.Create();
                                            }
                                        }

                                        result = ret.ManagedType;

                                        // Don't reply on result.IsInterface as we have some weird scenarios like refering to a exported type lib
                                        // which has interfaces that are class interfaces and have the same name as a class.
                                        // For example, manage class M has a class interface _M, and their managed name are both M
                                        if (ret.ConvType == ConvType.Interface || ret.ConvType == ConvType.EventInterface || ret.ConvType == ConvType.ClassInterface)
                                        {
                                            m_attribute = CustomAttributeHelper.GetBuilderForMarshalAs(UnmanagedType.Interface);
                                            SetUnmanagedType(UnmanagedType.Interface);
                                        }

                                        if (ret.ConvType == ConvType.CoClass)
                                        {
                                            // We need to convert CoClass to default interface (could be converted to class interface if it is exclusive) in signatures
                                            Debug.Assert(ret is IConvCoClass);
                                            IConvCoClass convCoClass = ret as IConvCoClass;
                                            if (convCoClass.DefaultInterface != null)
                                            {
                                                // Use the default interface
                                                result = convCoClass.DefaultInterface.ManagedType;
                                                m_attribute = CustomAttributeHelper.GetBuilderForMarshalAs(UnmanagedType.Interface);
                                                SetUnmanagedType(UnmanagedType.Interface);
                                            }
                                            else
                                            {
                                                // The coclass has no default interface (source interface excluded)
                                                // Marshal it as IUnknown
                                                result = typeof(object);
                                                m_attribute = CustomAttributeHelper.GetBuilderForMarshalAs(UnmanagedType.IUnknown);
                                                SetUnmanagedType(UnmanagedType.IUnknown);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    break;

                case VarEnum.VT_VARIANT:
                    result = typeof(object);
                    m_attribute = CustomAttributeHelper.GetBuilderForMarshalAs(UnmanagedType.Struct);
                    SetUnmanagedType(UnmanagedType.Struct);

                    // object is special that it will be marshaled to VARIANT
                    // because we'll think object as having one indirection, now we are one indirection less, 
                    // so we need add 1 to m_indirections
                    m_nativeIndirections++;
                    break;

                case VarEnum.VT_CY:
                    result = typeof(System.Decimal);
                    m_attribute = CustomAttributeHelper.GetBuilderForMarshalAs(UnmanagedType.Currency);
                    SetUnmanagedType(UnmanagedType.Currency);

                    break;

                case VarEnum.VT_DATE:
                    result = typeof(System.DateTime);
                    break;

                case VarEnum.VT_DECIMAL:
                    result = typeof(System.Decimal);
                    break;
                
                
                case VarEnum.VT_CARRAY:
                    {
                        TypeDesc elemTypeDesc = m_typeDesc.lptdesc;
                        TypeConverter elemTypeConverter = new TypeConverter(m_info, m_typeInfo, elemTypeDesc, ConversionType.Element);
                        Type elemType = elemTypeConverter.ConvertedType;
                        result = elemType.MakeArrayType();

                        m_conversionLoss |= elemTypeConverter.IsConversionLoss;

                        uint elements = 1;
                        SAFEARRAYBOUND[] bounds = m_typeDesc.lpadesc.Bounds;
                        foreach (SAFEARRAYBOUND bound in bounds)
                        {
                            elements *= bound.cElements;
                        }

                        // SizeConst can only hold Int32.MaxValue
                        if (elements <= Int32.MaxValue)
                        {
                            UnmanagedType arrayType;
                            if (m_conversionType == ConversionType.Field)
                                arrayType = UnmanagedType.ByValArray;
                            else
                                arrayType = UnmanagedType.LPArray;

                            if (elemTypeConverter.UseDefaultMarshal)
                                m_attribute = CustomAttributeHelper.GetBuilderForMarshalAsConstArray(arrayType, (int)elements);
                            else
                            {
                                if (elemTypeConverter.UnmanagedType == UnmanagedType.BStr   || 
                                    elemTypeConverter.UnmanagedType == UnmanagedType.LPStr  || 
                                    elemTypeConverter.UnmanagedType == UnmanagedType.LPWStr ||
                                    elemTypeConverter.UnmanagedType == UnmanagedType.VariantBool)
                                {
                                    m_attribute = CustomAttributeHelper.GetBuilderForMarshalAsConstArray(arrayType, (int)elements, elemTypeConverter.UnmanagedType);
                                }
                                else
                                {
                                    m_attribute = CustomAttributeHelper.GetBuilderForMarshalAsConstArray(arrayType, (int)elements);
                                }
                            }

                            SetUnmanagedType(arrayType);
                        }
                        else
                        {
                            m_nativeIndirections = 0;
                            result = typeof(IntPtr);
                            m_attribute = null;
                            ResetUnmanagedType();
                            m_conversionLoss = true;
                        }
                    }
                    break;

                case VarEnum.VT_BOOL:
                    // For VT_BOOL in fields, use short if v2 switch is not specified.
                    if (m_conversionType == ConversionType.Field)
                    {
                        if (m_info.Settings.m_isVersion2)
                        {
                            result = typeof(bool);
                            m_attribute = CustomAttributeHelper.GetBuilderForMarshalAs(UnmanagedType.VariantBool);
                            SetUnmanagedType(UnmanagedType.VariantBool);
                        }
                        else
                        {
                            result = typeof(short);
                        }
                    }
                    else
                    {
                        result = typeof(bool);
                        SetUnmanagedType(UnmanagedType.VariantBool);
                    }
                    break;

                default:
                    m_info.ReportEvent(
                        WarningCode.Wrn_BadVtType,
                        Resource.FormatString("Wrn_BadVtType", (int)vt, m_typeInfo.GetDocumentation()));
                    result = typeof(IntPtr);
                    m_conversionLoss = true;
                    break;
            }

            //
            // String -> StringBuilder special case
            //
            if (result == typeof(string))
            {
                if (_IsParamOut() && m_nativeIndirections == 1 && (m_conversionType == ConversionType.Parameter || m_conversionType == ConversionType.VarArgParameter))
                {
                    // [out] or [in, out] LPSTR/LPWSTR scenario
                    if (vt != VarEnum.VT_BSTR)
                    {
                        // String is immutable and cannot be [out]/[in, out]. We can convert to StringBuilder
                        result = typeof(StringBuilder);
                    }
                    else // VT_BSTR
                    {
                        // VT_BSTR is also immutable. So conversion loss here
                        m_conversionLoss = true;
                        result = typeof(IntPtr);
                        m_attribute = null;
                        m_nativeIndirections = 0;
                        ResetUnmanagedType();
                    }
                }
            }

            // Special rule for void* => IntPtr
            if (result == typeof(void))
            {
                result = typeof(IntPtr);

                switch (m_conversionType)
                {
                    case ConversionType.Element:
                        m_nativeIndirections = 0;
                        break;

                    case ConversionType.Field:
                        m_nativeIndirections = 0;
                        break;

                    default:
                        if (m_nativeIndirections > 1)
                            m_nativeIndirections = 1;
                        else
                            m_nativeIndirections = 0;
                        break;
                }
            }

            //
            // If the type is already a byref type, remove the byref and add extra indirection(s).
            // This is necessary to avoid trying to call MakeByRef on the byref type
            //
            if (result.IsByRef)
            {
                result = result.GetElementType();
                if (result.IsValueType)
                    m_nativeIndirections++;     // Value& = Value *
                else
                    m_nativeIndirections += 2;  // RefType& = RefType**
            }

            //
            // Process indirection
            //
            if (m_nativeIndirections > 0)
            {
                if (result.IsValueType)
                {
                    switch (m_conversionType)
                    {
                        case ConversionType.VarArgParameter:
                        case ConversionType.Parameter:
                            // Decimal/Guid can support extra level of indirection using LpStruct in parameters
                            // LpStruct has no effect in other places and for other types
                            // Only use LpStruct for scenarios like GUID **                            
                            // This is different from old TlbImp. Old TlbImp will use IntPtr
                            if ((result == typeof(Decimal) || result == typeof(Guid)) && m_nativeIndirections == 2)
                            {
                                m_nativeIndirections--;
                                m_attribute = CustomAttributeHelper.GetBuilderForMarshalAs(UnmanagedType.LPStruct);
                                ResetUnmanagedType();
                                SetUnmanagedType(UnmanagedType.LPStruct);
                            }

                            if (m_nativeIndirections >= 2)
                            {
                                m_conversionLoss = true;
                                result = typeof(IntPtr);
                                m_attribute = null;
                                ResetUnmanagedType();
                            }
                            else if (m_nativeIndirections > 0)
                            {
                                result = result.MakeByRefType();
                            }

                            break;

                        case ConversionType.Field:
                            m_conversionLoss = true;
                            result = typeof(IntPtr);
                            m_attribute = null;
                            ResetUnmanagedType();
                            break;

                        case ConversionType.ParamRetVal:
                            m_nativeIndirections--;
                            goto case ConversionType.ReturnValue;
                            // Fall through to ConversionType.ReturnValue

                        case ConversionType.ReturnValue:
                            if (m_nativeIndirections >= 1)
                            {
                                m_conversionLoss = true;
                                result = typeof(IntPtr);
                                m_attribute = null;
                                ResetUnmanagedType();
                            }
                            break;

                        case ConversionType.Element :
                            m_conversionLoss = true;
                            result = typeof(IntPtr);
                            m_attribute = null;
                            ResetUnmanagedType();
                            break;
                    }
                }
                else
                {
                    switch (m_conversionType)
                    {

                        case ConversionType.Field:
                            // ** => IntPtr, ConversionLoss
                            if (m_nativeIndirections > 1)
                            {
                                result = typeof(IntPtr);
                                m_conversionLoss = true;
                                m_attribute = null;
                                ResetUnmanagedType();
                            }
                            break;

                        case ConversionType.VarArgParameter:
                        case ConversionType.Parameter:
                            if (m_nativeIndirections > 2)
                            {
                                result = typeof(IntPtr);
                                m_conversionLoss = true;
                                m_attribute = null;
                                ResetUnmanagedType();
                            }
                            else if (m_nativeIndirections == 2)
                                result = result.MakeByRefType();
                            break;

                        case ConversionType.ParamRetVal:
                            m_nativeIndirections--;
                            goto case ConversionType.ReturnValue;
                            // Fall through to ConversionType.ReturnValue

                        case ConversionType.ReturnValue:
                            if (m_nativeIndirections > 1)
                            {
                                result = typeof(IntPtr);
                                m_conversionLoss = true;
                                m_attribute = null;
                                ResetUnmanagedType();
                            }
                            break;

                        case ConversionType.Element:
                            if (m_nativeIndirections > 1)
                            {
                                m_conversionLoss = true;
                                result = typeof(IntPtr);
                                m_attribute = null;
                                ResetUnmanagedType();
                            }
                            break;
                    }

                }
            }

            m_convertedType = result;
        }

        /// <summary>
        /// Is the parameter a [out]?
        /// </summary>
        private bool _IsParamOut()
        {
            if (m_paramDesc != null && m_paramDesc.IsOut)
                return true;

            return false;
        }

        /// <summary>
        /// Is this type a StdOle2.Guid? The test is done using the GUID of type library
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>True if this type is a StdOle2.Guid</returns>
        private bool _IsStdOleGuid(TypeInfo type)
        {
            TypeLib typeLib = type.GetContainingTypeLib();
            using (TypeLibAttr typeLibAttr = typeLib.GetLibAttr())
            {
                if (type.GetDocumentation() == "GUID" &&
                    typeLibAttr.guid == WellKnownGuids.TYPELIBID_STDOLE2)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Does the type need custom marshaler for marshalling?
        /// </summary>
        /// <returns>Whether we need to use custom marshaler</returns>
        private bool TryUseCustomMarshaler(Guid iid, out Type result)
        {
            result = null;

            if (iid == WellKnownGuids.IID_IDispatchEx)
            {
                Type type = typeof(System.Runtime.InteropServices.CustomMarshalers.ExpandoToDispatchExMarshaler);
                m_attribute = CustomAttributeHelper.GetBuilderForMarshalAsCustomMarshaler(type, "IExpando");
                SetUnmanagedType(UnmanagedType.CustomMarshaler);
                result = typeof(System.Runtime.InteropServices.Expando.IExpando);
                return true;
            }
            else if (iid == WellKnownGuids.IID_IEnumVARIANT)
            {
                Type type = typeof(System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler);
                m_attribute = CustomAttributeHelper.GetBuilderForMarshalAsCustomMarshaler(type, null);
                SetUnmanagedType(UnmanagedType.CustomMarshaler);
                result = typeof(System.Collections.IEnumerator);
                return true;
            }
            else if (iid == WellKnownGuids.IID_ITypeInfo)
            {
                Type type = typeof(System.Runtime.InteropServices.CustomMarshalers.TypeToTypeInfoMarshaler);
                m_attribute = CustomAttributeHelper.GetBuilderForMarshalAsCustomMarshaler(type, null);
                SetUnmanagedType(UnmanagedType.CustomMarshaler);
                result = typeof(Type);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether the interface supports calling by IDispatch
        /// </summary>
        /// <param name="interfaceType">The interface</param>
        /// <returns>True if the interface supports calling by IDispatch, false otherwise</returns>
        bool InterfaceSupportsDispatch(Type type)
        {
            IConvBase convBase = m_info.LookupType(type);
            if (convBase == null)
                return false;

            if (convBase as IConvInterface != null)
            {
                IConvInterface convInterface = convBase as IConvInterface;

                // dispinterface?
                if (convInterface.RefTypeInfo.GetTypeAttr().IsDispatch)
                    return true;
                else
                    return ConvCommon.IsDerivedFromIDispatch(convInterface.RefTypeInfo);
            }
            else if (convBase as IConvClassInterface != null || convBase as IConvEventInterface != null)
                return true;

            return false;
        }

        public ParameterAttributes ParameterAttributesOverride
        {
            get
            {
                return m_parameterAttributesOverride;
            }
            set
            {
                m_parameterAttributesOverride = value;
            }
        }

        private bool RuleEngineResolveRedirection(RuleSet ruleSet, TypeInfo typeInfo,
            out Type convertedType)
        {
            convertedType = null;
            if (ruleSet != null)
            {
                ICategory category = TypeCategory.GetInstance();
                TypeLibTypes.Interop.TYPEKIND typeKind;
                using (TypeAttr attr = typeInfo.GetTypeAttr())
                {
                    typeKind = attr.typekind;
                }
                TypeInfoMatchTarget target = new TypeInfoMatchTarget(typeInfo.GetContainingTypeLib(),
                    typeInfo, typeKind);
                AbstractActionManager actionManager = RuleEngine.GetActionManager();
                List<Rule> resolveToRules = ruleSet.GetRule(
                    category, ResolveToActionDef.GetInstance(), target);
                if (resolveToRules.Count != 0)
                {
                    if (resolveToRules.Count > 1)
                    {
                        Output.WriteWarning(Resource.FormatString("Wrn_RuleMultipleMatch",
                                            ResolveToActionDef.GetInstance().GetActionName()),
                            WarningCode.Wrn_RuleMultipleMatch);
                    }
                    Rule resolveToRule =
                        resolveToRules[resolveToRules.Count - 1];

                    ResolveToAction action =
                        resolveToRule.Action as ResolveToAction;
                    try
                    {
                        Assembly assembly = Assembly.ReflectionOnlyLoad(action.AssemblyName);
                        convertedType = assembly.GetType(action.ManagedTypeFullName);
                        return true;
                    }
                    catch (Exception)
                    {
                        Output.WriteWarning(Resource.FormatString("Wrn_CannotLoadResolveToType",
                                            action.ManagedTypeFullName, action.AssemblyName),
                            WarningCode.Wrn_CannotLoadResolveToType);
                    }
                }
            }
            return false;
        }

        #endregion 
        #region Private data members

        private ConverterInfo m_info;                       // ConverterInfo
        private int m_nativeIndirections;                         // level of indirections
        private TypeInfo m_typeInfo;                        // TypeInfo owner
        private TypeDesc m_typeDesc;                        // Type description
        private ConversionType m_conversionType;            // Conversion Type - parameter, return value, field
        private bool m_conversionLoss;                      // Conversion loss?
        private CustomAttributeBuilder m_attribute;         // MarshalAsAttribute for the type
        private Type m_convertedType;                       // Converted type
        private ParamDesc m_paramDesc;                      // Parameter information
        private bool m_convertingNewEnumMember;             // Are we creating the new enum member?
        private UnmanagedType m_unmanagedType;              // Corresponding unmanaged type. 
                                                            // Unfortunately we cannot get it from CustomAttributeBuilder
        private bool m_useDefaultMarshal;                   // Whether we marshal by default or marshal by UnmanagedType
        private ParameterAttributes m_parameterAttributesOverride = ParameterAttributes.None;
        #endregion
    }
}