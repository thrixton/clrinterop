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
using System.ComponentModel;
using System.Diagnostics;
using CoreRuleEngine;
using TlbImpRuleEngine;
using TypeLibTypes.Interop;
using System.Runtime.InteropServices;
using FormattedOutput;

namespace tlbimp2
{
    /// <summary>
    /// Provide common functions for type lib conversion
    /// </summary>
    class ConvCommon
    {
        /// <summary>
        /// Define guid for a certain type. Prefer the GUID on aliased type, otherwise the aliased type
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <param name="nonAliasedTypeInfo"></param>
        /// <param name="typeBuilder"></param>
        static public void DefineGuid(TypeInfo typeInfo, TypeInfo nonAliasedTypeInfo, TypeBuilder typeBuilder)
        {
            // Prefer the GUID on the alias, otherwise the aliased type
            Guid guid = Guid.Empty;
            guid = typeInfo.GetTypeAttr().Guid;
            if (guid == Guid.Empty)
                guid = nonAliasedTypeInfo.GetTypeAttr().Guid;

            if (guid != Guid.Empty)
            {
                // Handle the [Guid(...)] attribute
                typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForGuid(guid));
            }
        }

        /// <summary>
        /// Common code shared by struct, enum, and union type creation 
        /// </summary>
        static public TypeBuilder DefineTypeHelper(ConverterInfo info, TypeInfo typeInfo, TypeInfo nonAliasedTypeInfo, TypeAttributes attributes, Type typeParent, ConvType convType)
        {
            using (TypeAttr typeAttr = nonAliasedTypeInfo.GetTypeAttr())
            {
                string name = info.GetUniqueManagedName(typeInfo, convType);

                TypeBuilder typeBuilder = info.ModuleBuilder.DefineType(name, attributes, typeParent);

                DefineGuid(typeInfo, nonAliasedTypeInfo, typeBuilder);

                // We only emit TypeLibType attribute for class/interface, according to MSDN
                // typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForTypeLibType((TypeLibTypeFlags)attr.wTypeFlags));

                return typeBuilder;
            }
        }

        /// <summary>
        /// Converts COM type library type to internal type mapping 
        /// </summary>
        static public ConvType TypeKindToConvType(TypeLibTypes.Interop.TYPEKIND typekind)
        {
            switch (typekind)
            {
                case TypeLibTypes.Interop.TYPEKIND.TKIND_DISPATCH:
                    return ConvType.Interface;
                case TypeLibTypes.Interop.TYPEKIND.TKIND_ENUM:
                    return ConvType.Enum;
                case TypeLibTypes.Interop.TYPEKIND.TKIND_INTERFACE:
                    return ConvType.Interface;
                case TypeLibTypes.Interop.TYPEKIND.TKIND_RECORD:
                    return ConvType.Struct;
                case TypeLibTypes.Interop.TYPEKIND.TKIND_UNION:
                    return ConvType.Union;
                case TypeLibTypes.Interop.TYPEKIND.TKIND_COCLASS:
                    return ConvType.CoClass;
                default:
                    Debug.Assert(false, "Should not get here!");
                    return ConvType.Interface;
            }
        }

        /// <summary>
        /// If the type is aliased, return the ultimated non-aliased type if the type is user-defined, otherwise, return
        /// the aliased type directly. So the result could still be aliased to a built-in type.
        /// If the type is not aliased, just return the type directly
        /// </summary>
        static public void ResolveAlias(TypeInfo type, TypeDesc typeDesc, out TypeInfo realType, out TypeAttr realAttr)
        {
            if ((VarEnum)typeDesc.vt != VarEnum.VT_USERDEFINED)
            {
                // Already resolved
                realType = type;
                realAttr = type.GetTypeAttr();
                return;
            }
            else
            {
                TypeInfo refType = type.GetRefTypeInfo(typeDesc.hreftype);
                TypeAttr refAttr = refType.GetTypeAttr();

                // If the userdefined typeinfo is not itself an alias, then it is what the alias aliases.
                // Also, if the userdefined typeinfo is an alias to a builtin type, then the builtin
                // type is what the alias aliases.
                if (refAttr.typekind != TypeLibTypes.Interop.TYPEKIND.TKIND_ALIAS || (VarEnum)refAttr.tdescAlias.vt != VarEnum.VT_USERDEFINED)
                {
                    // Resolved
                    realType = refType;
                    realAttr = refAttr;
                }
                else
                {
                    // Continue resolving the type
                    ResolveAlias(refType, refAttr.tdescAlias, out realType, out realAttr);
                }
            }
        }

        /// <summary>
        /// Internal DECIMAL type for decimal conversion
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct DECIMAL
        {
            short wReserved;
            public byte scale;
            public byte sign;
            public uint hi32;
            public uint low32;
            public uint mid32;
        }

        /// <summary>
        /// Sets the default value according to the ElemDesc on the ParameterBuilder
        /// </summary>
        static public void SetDefaultValue(ElemDesc elem, Type type, ParameterBuilder paramBuilder)
        {
            ParamDesc param = elem.paramdesc;
            IntPtr ipPARAMDESCEX = param.lpVarValue;
            // It is bad that Marshalling doesn't take alignment into account, so I had to use a hard code value here
            IntPtr ipVariant = Utils.MovePointer(ipPARAMDESCEX, 8);
            SetDefaultValueInternal(ipVariant, type, paramBuilder, null, (short)elem.tdesc.vt);
        }

        /// <summary>
        /// Sets the default value according to the ElemDesc on the FieldBuilder
        /// </summary>
        static public void SetDefaultValue(VarDesc var, Type type, FieldBuilder fieldBuilder)
        {
            SetDefaultValueInternal(var.lpvarValue, type, null, fieldBuilder, (short)var.elemdescVar.tdesc.vt);
        }

        /// <summary>
        /// FieldBuilder/ParameterBuilder.SetConstant is very picky about the object type, while type library doesn't care
        /// For example, you could write char a = 0 which will become VT_I1 a = VT_I2 (0). In this case we need to normalize
        /// VT_I2 to VT_I1
        /// </summary>
        /// <returns>Returns the "normalized" value</returns>
        static private object GetNormalizedDefaultValueFromVariant(IntPtr ipVariant, Type type, short defaultValeuVt, short typeVt)
        {
            object defaultValue = Marshal.GetObjectForNativeVariant(ipVariant);

            // Unfortunately, because defaultValue is boxed, we can only convert it to the underlying boxed type
            // I had to workaround this by converting to a string and then convert it back
            if (type == typeof(bool))
            {
                if (defaultValue.ToString() == "0")
                    defaultValue = false;
                else
                    defaultValue = true;
            }
            else if (type == typeof(sbyte))
            {
                defaultValue = (object)sbyte.Parse(defaultValue.ToString());
            }
            else if (type == typeof(byte))
            {
                defaultValue = (object)byte.Parse(defaultValue.ToString());
            }
            else if (type == typeof(short))
            {
                defaultValue = (object)short.Parse(defaultValue.ToString());
            }
            else if (type == typeof(ushort))
            {
                defaultValue = (object)ushort.Parse(defaultValue.ToString());
            }
            else if (type == typeof(float))
            {
                defaultValue = (object)float.Parse(defaultValue.ToString());
            }
            else if (type == typeof(long))
            {
                defaultValue = (object)long.Parse(defaultValue.ToString());
            }
            else if (type == typeof(ulong))
            {
                defaultValue = (object)ulong.Parse(defaultValue.ToString());
            }
            else if (type == typeof(double))
            {
                defaultValue = (object)double.Parse(defaultValue.ToString());
            }
            else if (type == typeof(decimal))
            {
                defaultValue = (object)decimal.Parse(defaultValue.ToString());
            }

            // Normalize to nullref
            if (typeVt == (short)VarEnum.VT_DISPATCH || typeVt == (short)VarEnum.VT_UNKNOWN)
            {
                if (defaultValue != null && defaultValue.ToString() == "0")
                    defaultValue = null;
            }

            return defaultValue;
        }

        /// <summary>
        /// Provide implementation for setting default value 
        /// </summary>
        static private void SetDefaultValueInternal(IntPtr ipVariant, Type type, ParameterBuilder paramBuilder, FieldBuilder fieldBuilder, short typeVt)
        {
            // Use the element type for normalization if the type is a ByRef
            while (type.IsByRef)
                type = type.GetElementType();

            short defaultValueVt = Marshal.ReadInt16(ipVariant);
            object defaultValue = GetNormalizedDefaultValueFromVariant(ipVariant, type, defaultValueVt, typeVt);

            if (type.IsEnum && (defaultValueVt == (short)VarEnum.VT_I4 || defaultValueVt == (short)VarEnum.VT_UI4))
            {
                // 1) I can't dynamically create the enum in a ReflectionOnlyContext
                // 2) In pre-4.0, SetConstant(1) won't work for enum types.
                // After talking with team members we decided that this is a minor functionality that
                // we can live without in pre-4.0
                // return;
            }

            // Currently ParameterBuilder.SetConstant doesn't emit the custom attributes so certain types of constants
            // so we need to do that by ourselves
            CustomAttributeBuilder builder = null;
            if (type == typeof(Decimal))
            {
                DECIMAL realDecimal;
                IntPtr pDecimal = IntPtr.Zero;

                // Unfortunate we cannot directly access the fields in the decimal struct and we need to rely on the fact
                // that the internal representation of decimal & DECIMAL are the same, which will most likely remain true in the future
                // because CLR internally does the same thing
                Debug.Assert(sizeof(decimal) == Marshal.SizeOf(typeof(DECIMAL)));

                try
                {
                    pDecimal = Marshal.AllocCoTaskMem(sizeof(decimal));

                    // We convert it to unmanaged then back to managed to avoid unsafe code, so that we won't crash immediately in partial trust
                    Marshal.StructureToPtr(defaultValue, pDecimal, false);
                    realDecimal = (DECIMAL)Marshal.PtrToStructure(pDecimal, typeof(DECIMAL));
                }
                finally
                {
                    if (pDecimal != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(pDecimal);
                }

                builder = CustomAttributeHelper.GetBuilderForDecimalConstant(
                    realDecimal.scale,
                    realDecimal.sign,
                    realDecimal.hi32,
                    realDecimal.mid32,
                    realDecimal.low32);
            }
            else if (type == typeof(DateTime) && defaultValueVt == (short)VarEnum.VT_DATE)
            {
                builder = CustomAttributeHelper.GetBuilderForDateTimeConstant(((DateTime)defaultValue).Ticks);
            }
            else if (defaultValueVt == (short)VarEnum.VT_UNKNOWN)
            {
                // Currently ParameterBuilder.SetConstant doesn't emit the IUnknownConstantAttribute
                // for IUnknown so we need to do that by ourselves
                builder = CustomAttributeHelper.GetBuilderForIUnknownConstant();
            }
            else if (defaultValueVt == (short)VarEnum.VT_DISPATCH)
            {
                // Currently ParameterBuilder.SetConstant doesn't emit the IDispatchConstantAttribute
                // for IDispatch so we need to do that by ourselves
                builder = CustomAttributeHelper.GetBuilderForIDispatchConstant();
            }

            if (builder != null)
            {
                if (paramBuilder != null)
                    paramBuilder.SetCustomAttribute(builder);
                if (fieldBuilder != null)
                    fieldBuilder.SetCustomAttribute(builder);
            }
            else
            {
                try
                {
                    if (paramBuilder != null)
                        paramBuilder.SetConstant(defaultValue);
                    if (fieldBuilder != null)
                        fieldBuilder.SetConstant(defaultValue);
                }
                catch (Exception)
                {
                    // Debug.Assert(type.IsEnum, "We should avoid failing for non-Enum default values");
                }
            }
        }

        static private bool IsRetVal(ConverterInfo info, ParamDesc paramDesc, FuncDesc funcDesc)
        {
            if (paramDesc.IsRetval)
            {
                // Don't consider it is a RetVal for dispatch functions unless TransformDispRetVal is true
                if (funcDesc.funckind == TypeLibTypes.Interop.FUNCKIND.FUNC_DISPATCH && !info.TransformDispRetVal)
                    return false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Process parameters and apply attributes
        /// </summary>
        static private void ProcessParam(InterfaceInfo info, InterfaceMemberInfo memInfo, ElemDesc desc,
            TypeConverter converter, MethodBuilder methodBuilder, int index, string strname,
            bool isVarArg, bool isOptionalArg)
        {
            ParamDesc param = desc.paramdesc;
            ParameterAttributes attributes = ParameterAttributes.None;
            if (converter.ParameterAttributesOverride != ParameterAttributes.None)
            {
                attributes = converter.ParameterAttributesOverride;
            }
            else
            {
                //
                // Determine in/out/opt
                // Don't emit [In], [Out] for return type
                //
                if (index != 0)
                {
                    // Always emit in/out information according to type library
                    if (param.IsIn)
                    {
                        attributes |= ParameterAttributes.In;
                    }

                    if (param.IsOut)
                    {
                        attributes |= ParameterAttributes.Out;
                    }

                    if (param.IsOpt || isOptionalArg)
                    {
                        attributes |= ParameterAttributes.Optional;
                    }
                }
            }
            
            //
            // Define the parameter
            //
            ParameterBuilder parameterBuilder = methodBuilder.DefineParameter(index, attributes, strname);

            //
            // Apply MarshalAs attribute
            //
            converter.ApplyAttributes(parameterBuilder);

            //
            // Support for vararg
            //
            if (isVarArg)
            {
                parameterBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForParamArray());
            }

            //
            // Support for default value
            //
            if (desc.paramdesc.HasDefault && desc.paramdesc.lpVarValue != IntPtr.Zero)
                SetDefaultValue(desc, converter.ConvertedType, parameterBuilder);
        }

        /// <summary>
        /// Kind of the return parameter
        /// </summary>
        public enum ReturnKind
        {
            RetValParameter,            // return as [retval]
            ReturnValue,                // return as real function return value
            NoReturn                    // no return (void)
        }

        /// <summary>
        /// TypeConverter instance for void
        /// </summary>
        static private readonly TypeConverter s_voidTypeConverter = new TypeConverter(typeof(void));

        /// <summary>
        /// Get the return type converter for the function
        /// </summary>
        static public TypeConverter GetReturnType(InterfaceInfo info, InterfaceMemberInfo memInfo,
            FuncDesc func, bool isNewEnumMember, TypeConverter returnConverterOverride,
            List<TypeConverter> parameterConverterOverrideList, out ReturnKind kind,
            out bool isStandardOleCall, out int returnArgId)
        {
            TypeConverter retType = null;
            returnArgId = -1;
            kind = ReturnKind.ReturnValue;
            isStandardOleCall = false;

            TypeDesc retTypeDesc = func.elemdescFunc.tdesc;

            // If /preservesig is specified, we perform the PreserveSig transformation, except for dispatch functions.
            // Because they are not affected by PreserveSig attribute.
            if ((!func.IsDispatch) &&
                (info.ConverterInfo.Settings.m_isPreserveSig ||
                    IsPreserveSigOverride(info, memInfo.Index)))
            {
                if (returnConverterOverride != null)
                {
                    retType = returnConverterOverride;
                    kind = ReturnKind.ReturnValue;
                }
                else if (retTypeDesc.vt == (int)VarEnum.VT_VOID)
                {
                    retType = s_voidTypeConverter;
                    kind = ReturnKind.NoReturn;
                }
                else
                {
                    retType = new TypeConverter(info.ConverterInfo, info.RefTypeInfo, func.elemdescFunc.tdesc, isNewEnumMember, ConversionType.ReturnValue);
                    kind = ReturnKind.ReturnValue;
                }

                return retType;
            }

            // Inspect the return value
            // If the return is HRESULT and PreserveSig switch is not specified, we will do the HRESULT transformation
            // Otherwise, stick to the original return value and use PreserveSigAttribute
            if (retTypeDesc.vt == (int)VarEnum.VT_HRESULT)
            {
                isStandardOleCall = true;
            }
            else
            {
                if (retTypeDesc.vt != (int)VarEnum.VT_VOID && retTypeDesc.vt != (int)VarEnum.VT_HRESULT)
                {
                    if (returnConverterOverride != null)
                        retType = returnConverterOverride;
                    else
                        retType = new TypeConverter(info.ConverterInfo, info.RefTypeInfo,
                            func.elemdescFunc.tdesc, isNewEnumMember, ConversionType.ReturnValue);
                    kind = ReturnKind.ReturnValue;
                }
            }

            if (func.funckind == TypeLibTypes.Interop.FUNCKIND.FUNC_DISPATCH &&
                (!info.ConverterInfo.TransformDispRetVal))
            {
                // Skip RetVal => return value transformation for dispatch interface when TransformDispRetVal is not set
            }
            else
            {
                for (int i = 0; i < func.cParams; ++i)
                {
                    ElemDesc ret = func.GetElemDesc(i);

                    // In a COM type library, the return parameter is logical and is the last parameter
                    // whereas it is an explicit separate item in managed.
                    if (IsRetVal(info.ConverterInfo, ret.paramdesc, func))
                    {
                        // Convert last parameter to return value
                        if (retType == null)
                        {
                            returnArgId = i;
                            if (parameterConverterOverrideList[returnArgId] != null)
                                retType = parameterConverterOverrideList[returnArgId];
                            else
                                retType = new TypeConverter(info.ConverterInfo, info.RefTypeInfo,
                                    ret.tdesc, isNewEnumMember, ConversionType.ParamRetVal);
                            kind = ReturnKind.RetValParameter;
                        }
                        else
                        {
                            info.ConverterInfo.ReportEvent(
                                WarningCode.Wrn_AmbiguousReturn,
                                Resource.FormatString("Wrn_AmbiguousReturn", info.RefTypeInfo.GetDocumentation(), info.RefTypeInfo.GetDocumentation(func.memid)));
                        }
                        break;
                    }
                }
            }

            if (retType == null)
            {
                if (returnConverterOverride != null)
                {
                    retType = returnConverterOverride;
                    kind = ReturnKind.ReturnValue;
                }
                else
                {
                    retType = s_voidTypeConverter;
                    kind = ReturnKind.NoReturn;
                }
            }

            return retType;
        }

        private static bool IsPreserveSigOverride(InterfaceInfo interfaceInfo, int index)
        {
            // Check rule engine
            if (interfaceInfo.ConverterInfo.Settings.m_ruleSet != null)
            {
                ICategory functionCategory = FunctionCategory.GetInstance();
                FunctionInfoMatchTarget target =
                    new FunctionInfoMatchTarget(interfaceInfo.RefTypeInfo, index);
                AbstractActionManager actionManager = RuleEngine.GetActionManager();
                List<Rule> preserveSigRules = interfaceInfo.ConverterInfo.Settings.m_ruleSet.GetRule(
                    functionCategory, PreserveSigActionDef.GetInstance(),
                    target);
                if (preserveSigRules.Count != 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Figure out types for parameters and return a parameter array
        /// </summary>
        /// <returns>TypeConverter[] array for all parameters</returns>
        static public TypeConverter[] GenerateParameterTypes(InterfaceInfo info,
            InterfaceMemberInfo memInfo, FuncDesc func, bool isNewEnumMember,
            int vararg, out int lcidArg, out bool isStandardOleCall,
            out TypeConverter retTypeConverter, out ReturnKind kind, out int retArgId)
        {
            //
            // Check Lcid Parameter
            //
            lcidArg = -1;
            for (int i = 0; i < func.cParams; ++i)
            {
                if (func.GetElemDesc(i).paramdesc.IsLCID)
                {
                    if (lcidArg >= 0)
                    {
                        info.ConverterInfo.ReportEvent(
                            WarningCode.Wrn_MultipleLcids,
                            Resource.FormatString("Wrn_MultipleLcids",
                                info.RefTypeInfo.GetDocumentation(),
                                info.RefTypeInfo.GetDocumentation(func.memid)));
                    }

                    lcidArg = i;
                }
            }

            // Pre-process for the rule engine
            // For return value
            TypeConverter convertToReturnConverter = null;
            GetRuledReturnTypeConverter(info, memInfo, func, out convertToReturnConverter);

            // For parameters
            List<TypeConverter> convertToParameterConverterList;
            GetRuledParameterTypeConverters(info, memInfo, func, out convertToParameterConverterList);

            retTypeConverter = GetReturnType(info, memInfo, func, isNewEnumMember, convertToReturnConverter,
                convertToParameterConverterList, out kind, out isStandardOleCall, out retArgId);

            if ((retTypeConverter == convertToReturnConverter ||
                    convertToParameterConverterList.Contains(retTypeConverter)))
            {
                // From convertTo Action.
                if ((retTypeConverter.ParameterAttributesOverride & ParameterAttributes.In) !=
                    ParameterAttributes.None)
                {
                    Output.WriteWarning(Resource.FormatString("Wrn_ConvertReturnValueToNoneOut",
                                                              info.RefTypeInfo.GetDocumentation(func.memid)),
                                WarningCode.Wrn_ConvertReturnValueToNoneOut);
                }
                retTypeConverter.ParameterAttributesOverride = ParameterAttributes.None;
            }

            int cParams = func.cParams;
            List<TypeConverter> typeConverterList = new List<TypeConverter>();
            short n;
            for (n = 0; n < cParams; ++n)
            {
                ElemDesc elem = func.GetElemDesc(n);
                ParamDesc paramDesc = elem.paramdesc;

                // Skip LCID
                if (paramDesc.IsLCID)
                    continue;

                // Skip return parameter
                if (kind == ReturnKind.RetValParameter && n == retArgId)
                    continue;

                try
                {
                    ConversionType conversionType;
                    if (n == vararg)
                        conversionType = ConversionType.VarArgParameter;
                    else
                        conversionType = ConversionType.Parameter;

                    if (convertToParameterConverterList[n] != null)
                    {
                        typeConverterList.Add(convertToParameterConverterList[n]);
                    }
                    else
                    {
                        typeConverterList.Add(new TypeConverter(
                            info.ConverterInfo, info.RefTypeInfo, elem.tdesc,
                            paramDesc, false, conversionType));
                    }
                }
                catch (COMException ex)
                {
                    if ((uint)ex.ErrorCode == HResults.TYPE_E_CANTLOADLIBRARY)
                    {
                        string[] names = info.RefTypeInfo.GetNames(func.memid, func.cParams + 1);
                        string name = names[n + 1];

                        if (name != String.Empty)
                        {
                            string msg = Resource.FormatString("Wrn_ParamErrorNamed",
                                new object[] { info.RefTypeInfo.GetDocumentation(), name,
                                    memInfo.UniqueName });
                            info.ConverterInfo.ReportEvent(WarningCode.Wrn_ParamErrorNamed, msg);
                        }
                        else
                        {
                            string msg = Resource.FormatString("Wrn_ParamErrorUnnamed",
                                new object[] { info.RefTypeInfo.GetDocumentation(), n, memInfo.UniqueName });
                            info.ConverterInfo.ReportEvent(WarningCode.Wrn_ParamErrorUnnamed, msg);
                        }
                    }

                    throw;
                }
            }

            return typeConverterList.ToArray();
        }

        private static void GetRuledParameterTypeConverters(InterfaceInfo info, InterfaceMemberInfo memInfo,
            FuncDesc func, out List<TypeConverter> convertToParameterConverterList)
        {
            int cParams = func.cParams;
            convertToParameterConverterList = new List<TypeConverter>();

            short n;
            for (n = 0; n < cParams; ++n)
            {
                ElemDesc elem = func.GetElemDesc(n);
                ParamDesc paramDesc = elem.paramdesc;

                // "ConvertTo" for basic types and array
                // Rule Engine
                Rule convertToRule = null;
                if (memInfo.Index != -1)
                {
                    // Is not event delegate.
                    SignatureInfoMatchTarget target = new SignatureInfoMatchTarget(
                            info.RefTypeInfo, memInfo.Index, elem, n + 1);
                    if (info.ConverterInfo.Settings.m_ruleSet != null)
                    {
                        ICategory signatureCategory = SignatureCategory.GetInstance();
                        AbstractActionManager actionManager = RuleEngine.GetActionManager();
                        List<Rule> convertToRules = info.ConverterInfo.Settings.m_ruleSet.GetRule(
                            signatureCategory, ConvertToActionDef.GetInstance(), target);
                        if (convertToRules.Count != 0)
                        {
                            if (convertToRules.Count > 1)
                            {
                                Output.WriteWarning(
                                    Resource.FormatString("Wrn_RuleMultipleMatch",
                                                          ConvertToActionDef.GetInstance().GetActionName()),
                                    WarningCode.Wrn_RuleMultipleMatch);
                            }
                            convertToRule = convertToRules[convertToRules.Count - 1];
                        }
                    }
                    if (convertToRule != null)
                    {
                        ConvertToAction convertToAction = convertToRule.Action as ConvertToAction;
                        Type typeReturn;
                        CustomAttributeBuilder customAttribute;
                        ParameterAttributes fixedParameterAttributes;
                        GetTypeFromConvertToAction(target, convertToAction, info.ConverterInfo,
                            info.RefTypeInfo, elem.tdesc, out typeReturn, out customAttribute,
                            out fixedParameterAttributes);
                        convertToParameterConverterList.Add(
                            new TypeConverter(typeReturn, customAttribute, fixedParameterAttributes));
                    }
                    else
                    {
                        convertToParameterConverterList.Add(null);
                    }
                }
                else
                {
                    convertToParameterConverterList.Add(null);
                }
            }
        }

        private static void GetRuledReturnTypeConverter(InterfaceInfo info,
            InterfaceMemberInfo memInfo, FuncDesc func, out TypeConverter convertToReturnConverter)
        {
            convertToReturnConverter = null;
            Rule convertToRule = null;
            if (memInfo.Index != -1)
            {
                // Is not event delegate.
                SignatureInfoMatchTarget returnTarget = new SignatureInfoMatchTarget(
                            info.RefTypeInfo, memInfo.Index, func.elemdescFunc, 0);
                if (info.ConverterInfo.Settings.m_ruleSet != null)
                {
                    ICategory signatureCategory = SignatureCategory.GetInstance();
                    AbstractActionManager actionManager = RuleEngine.GetActionManager();
                    List<Rule> convertToRules = info.ConverterInfo.Settings.m_ruleSet.GetRule(
                        signatureCategory, ConvertToActionDef.GetInstance(), returnTarget);
                    if (convertToRules.Count != 0)
                    {
                        if (convertToRules.Count > 1)
                        {
                            Output.WriteWarning(
                                Resource.FormatString("Wrn_RuleMultipleMatch",
                                                      ConvertToActionDef.GetInstance().GetActionName()),
                                WarningCode.Wrn_RuleMultipleMatch);
                        }
                        convertToRule = convertToRules[convertToRules.Count - 1];
                    }
                }
                if (convertToRule != null)
                {
                    ConvertToAction convertToAction = convertToRule.Action as ConvertToAction;
                    Type typeReturn;
                    CustomAttributeBuilder customAttribute;
                    ParameterAttributes fixedParameterAttributes;
                    GetTypeFromConvertToAction(returnTarget, convertToAction, info.ConverterInfo,
                        info.RefTypeInfo, func.elemdescFunc.tdesc, out typeReturn, out customAttribute,
                        out fixedParameterAttributes);
                    convertToReturnConverter =
                        new TypeConverter(typeReturn, customAttribute, fixedParameterAttributes);
                }
            }
        }

        private static void GetTypeFromConvertToAction(SignatureInfoMatchTarget target,
            ConvertToAction convertToAction, ConverterInfo converterInfo, TypeInfo typeInfo,
            TypeDesc typeDesc, out Type typeReturn, out CustomAttributeBuilder customAttribute,
            out ParameterAttributes fixedParameterAttributes)
        {
            typeReturn = null;
            customAttribute = null;
            fixedParameterAttributes = ParameterAttributes.None;
            ParameterDirection direction =
                ConvertToActionConstants.GetParameterDirection(convertToAction.Direction);
            switch (direction)
            {
                case ParameterDirection.IN:
                    fixedParameterAttributes |= ParameterAttributes.In;
                    break;
                case ParameterDirection.OUT:
                    fixedParameterAttributes |= ParameterAttributes.Out;
                    break;
                case ParameterDirection.INOUT:
                    fixedParameterAttributes |= ParameterAttributes.In;
                    fixedParameterAttributes |= ParameterAttributes.Out;
                    break;
            }
            Dictionary<string, string> attributePairDictionary =
                ConvertToAction.GetConvertToAttributeDictionary(convertToAction.Attributes);
            ManagedTypeConvertTo managedType =
                ConvertToActionConstants.GetManagedTypeConvertTo(convertToAction.ManagedTypeConvertTo);
            UnmanagedType marshalAs =
                ConvertToActionConstants.GetMarshalAs(convertToAction.UnmanagedTypeMarshalAs);

            switch (managedType)
            {
                case ManagedTypeConvertTo.LPARRAY:
                    if ((VarEnum)typeDesc.vt == VarEnum.VT_CARRAY || (VarEnum)typeDesc.vt == VarEnum.VT_PTR)
                    {
                        TypeConverter elemTypeConverter = new TypeConverter(converterInfo, typeInfo,
                            typeDesc.lptdesc, ConversionType.Element);
                        Type elemType = elemTypeConverter.ConvertedType;
                        typeReturn = elemType.MakeArrayType();
                        if (attributePairDictionary.ContainsKey(ConvertToActionDef.SizeConst))
                        {
                            customAttribute = CustomAttributeHelper.GetBuilderForMarshalAsConstArray(UnmanagedType.LPArray,
                                Int32.Parse(attributePairDictionary[ConvertToActionDef.SizeConst].ToString()));
                        }
                        else if (attributePairDictionary.ContainsKey(ConvertToActionDef.SizeParamIndex))
                        {
                            customAttribute = CustomAttributeHelper.GetBuilderForMarshalAsConstArrayBySizeParamIndex(
                                UnmanagedType.LPArray,
                                Int16.Parse(attributePairDictionary[ConvertToActionDef.SizeParamIndex].ToString()));
                        }
                        else if (attributePairDictionary.ContainsKey(ConvertToActionDef.SizeParamIndexOffset))
                        {
                            customAttribute = CustomAttributeHelper.GetBuilderForMarshalAsConstArrayBySizeParamIndex(
                                UnmanagedType.LPArray,
                                (short)((target.NativeParameterIndex) - 1 +
                                         Int16.Parse(attributePairDictionary[ConvertToActionDef.SizeParamIndexOffset].ToString())));
                        }
                        else
                        {
                            customAttribute = CustomAttributeHelper.GetBuilderForMarshalAs(UnmanagedType.LPArray);
                        }
                    }
                    else
                    {
                        string targetTypeString = new TlbType2String(typeInfo, typeDesc).GetTypeString();
                        throw new TlbImpGeneralException(Resource.FormatString("Err_ConvertNonArrayToArray", targetTypeString),
                            ErrorCode.Err_ConvertNonArrayToArray);
                    }
                    break;
                case ManagedTypeConvertTo.DECIMAL:
                    typeReturn = typeof(Decimal);
                    if (marshalAs != (UnmanagedType)(-1))
                        customAttribute = CustomAttributeHelper.GetBuilderForMarshalAs(marshalAs);
                    break;
                case ManagedTypeConvertTo.INT:
                    typeReturn = typeof(int);
                    if (marshalAs != (UnmanagedType)(-1))
                        customAttribute = CustomAttributeHelper.GetBuilderForMarshalAs(marshalAs);
                    break;
                case ManagedTypeConvertTo.OBJECT:
                    typeReturn = typeof(object);
                    if (marshalAs != (UnmanagedType)(-1))
                        customAttribute = CustomAttributeHelper.GetBuilderForMarshalAs(marshalAs);
                    break;
                case ManagedTypeConvertTo.STRING:
                    typeReturn = typeof(string);
                    if (marshalAs != (UnmanagedType)(-1))
                        customAttribute = CustomAttributeHelper.GetBuilderForMarshalAs(marshalAs);
                    break;
                case ManagedTypeConvertTo.STRINGBUILDER:
                    typeReturn = typeof(StringBuilder);
                    if (marshalAs != (UnmanagedType)(-1))
                        customAttribute = CustomAttributeHelper.GetBuilderForMarshalAs(marshalAs);
                    break;
            }
            if (convertToAction.ByRef)
            {
                typeReturn = typeReturn.MakeByRefType();
            }
        }

        /// <summary>
        /// For every method in the interface, we can either create a corresponding method or create a event delegate
        /// The reason why we need this for event delegate is because event delegate has the exact method signature
        /// as the method in the interface
        /// </summary>
        public enum CreateMethodMode
        {
            InterfaceMethodMode,                            // For interface method creation, create a new method
            EventDelegateMode                               // For event delegate creation, create a delegate
        }

        private const string INVOKE_METHOD = "Invoke";          // For Delegate.Invoke
        private const string VTBL_GAP_FORMAT_1 = "_VtblGap{0}";      // Vtbl gap function name for 1
        private const string VTBL_GAP_FORMAT_N = "_VtblGap{0}_{1}";  // Vtbl gap function name for n

        static private MethodBuilder CreateMethodCore(InterfaceInfo info, InterfaceMemberInfo memberInfo, bool isNewEnumMember, CreateMethodMode mode, bool isStandardOleCall, Type retType, Type[] paramTypes)
        {
            //
            // vtbl gap support. We only emit vtbl gap for non dispinterfaces
            //
            if (!info.IsCoClass && !info.RefTypeAttr.IsDispatch && mode != CreateMethodMode.EventDelegateMode)
            {
                int pointerSize = GetPointerSize(info.RefTypeInfo);

                int slot = memberInfo.RefFuncDesc.oVft / pointerSize;
                if (slot != info.CurrentSlot)
                {
                    // Make sure slot numbers are monotonically increasing.
                    if (slot < info.CurrentSlot)
                    {
                        info.ConverterInfo.ReportEvent(
                            WarningCode.Wrn_BadVTable,
                            Resource.FormatString("Wrn_BadVTable",
                                new object[] { memberInfo.UniqueName, info.TypeBuilder.FullName, info.ConverterInfo.ModuleBuilder.Name })
                                );

                        throw new TlbImpInvalidTypeConversionException(info.RefTypeInfo);
                    }

                    int gap = slot - info.CurrentSlot;
                    string vtblGapFuncName;
                    if (gap == 1)
                        vtblGapFuncName = string.Format(VTBL_GAP_FORMAT_1, info.CurrentSlot);
                    else
                        vtblGapFuncName = string.Format(VTBL_GAP_FORMAT_N, info.CurrentSlot, gap);

                    MethodBuilder vtblFunc = info.TypeBuilder.DefineMethod(
                        vtblGapFuncName,
                        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.Abstract);
                    vtblFunc.SetImplementationFlags(MethodImplAttributes.Runtime);

                    info.CurrentSlot = slot;
                }

                ++info.CurrentSlot;
            }

            MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.Virtual;

            string methodName;

            // Determine the name & attributes
            if (mode == CreateMethodMode.EventDelegateMode)
            {
                methodName = INVOKE_METHOD;
            }
            else
            {
                methodName = info.GenerateUniqueMemberName(memberInfo.RecommendedName, paramTypes, MemberTypes.Method);

                // Update the name so that we can know the unique member name in the interface later in override
                if (!info.IsCoClass)
                {
                    memberInfo.UpdateUniqueName(methodName);
                }

                attributes |= System.Reflection.MethodAttributes.HideBySig | System.Reflection.MethodAttributes.NewSlot;
                if (!info.IsCoClass)
                {
                    attributes |= System.Reflection.MethodAttributes.Abstract;
                }
            }

            // Is property? If so, add SpecialName to attributes
            if (!isNewEnumMember && memberInfo.InvokeKind != TypeLibTypes.Interop.INVOKEKIND.INVOKE_FUNC)
                attributes |= MethodAttributes.SpecialName;

            MethodBuilder methodBuilder = info.TypeBuilder.DefineMethod(
                methodName,
                attributes,
                CallingConventions.ExplicitThis | CallingConventions.HasThis,
                retType,
                paramTypes);

            // Set implementation flags
            MethodImplAttributes implAttributes;
            if (mode == CreateMethodMode.EventDelegateMode)
                implAttributes = MethodImplAttributes.Managed | MethodImplAttributes.Runtime;
            else
                implAttributes = MethodImplAttributes.InternalCall | MethodImplAttributes.Runtime;

            if (!isStandardOleCall)
                implAttributes |= MethodImplAttributes.PreserveSig;

            methodBuilder.SetImplementationFlags(implAttributes);

            // Add handling for [id(...)] if necessary
            if (info.EmitDispId || memberInfo.DispIdIsOverridden)
            {
                if (info.IsCoClass && !info.IsDefaultInterface)
                {
                    // Skip non-default interfaces in coclass
                }
                else
                {
                    methodBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForDispId(memberInfo.DispId));
                }
            }

            if (memberInfo.PropertyInfo != null && memberInfo.PropertyInfo.HasInvalidGetter)
            {
                info.ConverterInfo.ReportEvent(
                    WarningCode.Wrn_PropgetWithoutReturn,
                    Resource.FormatString("Wrn_PropgetWithoutReturn",
                        memberInfo.PropertyInfo.RecommendedName,
                        info.TypeBuilder.FullName
                        )
                );
            }

            if (memberInfo.IsProperty && !isNewEnumMember)
            {
                info.PropertyInfo.SetPropertyInfo(memberInfo, methodBuilder);
            }


            // Add a .override instruction for the method for coclass
            if (info.IsCoClass)
            {
                Debug.Assert(info.CurrentImplementingInterface != null);

                using (TypeAttr implementingInterfaceAttr = info.CurrentImplementingInterface.GetTypeAttr())
                {
                    IConvBase convBase = info.ConverterInfo.GetInterface(info.CurrentImplementingInterface, implementingInterfaceAttr);
                    Type interfaceType = convBase.RealManagedType;

                    // Type.GetMethod(Name, ParamList) actually requires all the parameters be loaded (thus created).
                    // Type.GetMethod(Name) won't have this problem. 
                    // We can workaround this limitation by having the coclass created after all the other types

                    // Must use UniqueName because it is the right name on the interface.
                    // We should use exact match here.
                    MethodInfo methodInfo = interfaceType.GetMethod(memberInfo.UniqueName,
                        BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Instance, null, paramTypes, null);

                    if (methodInfo == null)
                    {
                        string expectedPrototypeString = FormatMethodPrototype(retType, memberInfo.UniqueName, paramTypes);
                        string msg = Resource.FormatString("Err_OverridedMethodNotFoundInImplementedInterface",
                                        new object[] { expectedPrototypeString, interfaceType.FullName,
                                            info.TypeBuilder.FullName, info.TypeBuilder.Assembly.FullName });
                        throw new TlbImpGeneralException(msg, ErrorCode.Err_OverridedMethodNotFoundInImplementedInterface);
                    }

                    info.TypeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
                }
            }

            return methodBuilder;
        }

        private static string FormatMethodPrototype(Type retType, string funcName, Type[] paramTypes)
        {
            string prototypeString = retType + " " + funcName + "(";
            for (int i = 0; i < paramTypes.Length; i++)
            {
                prototypeString += paramTypes[i];
                if (i != paramTypes.Length - 1)
                {
                    prototypeString += ", ";
                }
            }
            prototypeString += ")";
            return prototypeString;
        }

        #region DISPID_NEWENUM Support

        static public bool HasForceIEnumerableCustomAttribute(TypeInfo typeInfo)
        {
            return (typeInfo.GetCustData(CustomAttributeGuids.GUID_ForceIEnumerable) != null);
        }

        static public bool HasNewEnumMember(ConverterInfo info, TypeInfo typeInfo, string fullName)
        {
            bool hasNewEnumMember = false;
            bool hasDuplicateNewEnumMember = false;
            int firstNewEnum = -1;

            using (TypeAttr attr = typeInfo.GetTypeAttr())
            {
                if (attr.IsDispatch ||
                    (attr.IsInterface && ConvCommon.IsDerivedFromIDispatch(typeInfo)))
                {
                    // Check to see if the interface has a function with a DISPID of DISPID_NEWENUM.
                    for (int i = 0; i < attr.cFuncs; ++i)
                    {
                        using(FuncDesc func = typeInfo.GetFuncDesc(i))
                        {
                            if (IsNewEnumFunc(info, typeInfo, func, i))
                            {
                                if (!hasNewEnumMember)
                                    firstNewEnum = func.memid;

                                if (hasNewEnumMember)
                                    hasDuplicateNewEnumMember = true;

                                // The interface has a function with a DISPID of DISPID_NEWENUM.
                                hasNewEnumMember = true;
                            }
                        }
                    }

                    // Check to see if the interface as a property with a DISPID of DISPID_NEWENUM.
                    for (int i = 0; i < attr.cVars; ++i)
                    {
                        using (VarDesc varDesc = typeInfo.GetVarDesc(i))
                        {
                            if (IsNewEnumDispatchProperty(info, typeInfo, varDesc, i))
                            {
                                if (!hasNewEnumMember)
                                    firstNewEnum = varDesc.memid;

                                if (hasNewEnumMember)
                                    hasDuplicateNewEnumMember = true;

                                // The interface has a property with a DISPID of DISPID_NEWENUM.
                                hasNewEnumMember = true;
                            }
                        }
                    }

                    // Check to see if the ForceIEnumerable custom value exists on the type
                    if (HasForceIEnumerableCustomAttribute(typeInfo))
                        hasNewEnumMember = true;

                    if (hasDuplicateNewEnumMember)
                    {
                        info.ReportEvent(
                            WarningCode.Wrn_MultiNewEnum,
                            Resource.FormatString("Wrn_MultiNewEnum", fullName, typeInfo.GetDocumentation(firstNewEnum)));
                    }
                }
                else
                {
                    // Check to see if the ForceIEnumerable custom value exists on the type
                    //  If it does, spit out a warning.
                    if (HasForceIEnumerableCustomAttribute(typeInfo))
                    {
                        string msg = Resource.FormatString(
                            "Wrn_IEnumCustomAttributeOnIUnknown",
                            CustomAttributeGuids.GUID_ForceIEnumerable.ToString().ToUpper(),
                            typeInfo.GetDocumentation());

                        info.ReportEvent(WarningCode.Wrn_IEnumCustomAttributeOnIUnknown, msg);
                    }
                }
            }

            return hasNewEnumMember;
        }

        /// <summary>
        /// Override the dispid if Guid_DispIdOverride is present
        /// </summary>
        /// <param name="index">The index of the func/var, not the disp id</param>
        /// <returns>Whether we have Guid_DispIdOverride or not</returns>
        static public bool GetOverrideDispId(ConverterInfo info, TypeInfo typeInfo, int index, InterfaceMemberType memberType, ref int dispid, bool isSet)
        {
            bool hasOverride = false;
            object data;

            if (memberType == InterfaceMemberType.Method)
                data = typeInfo.GetFuncCustData(index, CustomAttributeGuids.GUID_DispIdOverride);
            else
            {
                Debug.Assert(memberType == InterfaceMemberType.Variable);
                data = typeInfo.GetVarCustData(index, CustomAttributeGuids.GUID_DispIdOverride);
            }

            if (data is short)
            {
                dispid = (short)data;
                hasOverride = true;
            }
            else if (data is int)
            {
                dispid = (int)data;
                hasOverride = true;
            }
            else if (data != null)
            {
                // We only emit Wrn_NonIntegralCustomAttributeType when we set the id
                if (isSet)
                {
                    //
                    // Emit Wrn_NonIntegralCustomAttributeType warning
                    //
                    info.ReportEvent(
                        WarningCode.Wrn_NonIntegralCustomAttributeType,
                        Resource.FormatString("Wrn_NonIntegralCustomAttributeType", "{" + CustomAttributeGuids.GUID_DispIdOverride.ToString().ToUpper() + "}", typeInfo.GetDocumentation(dispid)));
                }
            }

            return hasOverride;
        }

        /// <summary>
        /// Is this function a NewEnum function with the right parameters and DISPID?
        /// </summary>
        static public bool IsNewEnumFunc(ConverterInfo info, TypeInfo typeInfo, FuncDesc func, int index)
        {
            //
            // Support GUID_DispIdOverride
            //
            int dispid = func.memid;
            GetOverrideDispId(info, typeInfo, index, InterfaceMemberType.Method, ref dispid, false);

            if (dispid == WellKnownDispId.DISPID_NEWENUM)
            {
                TypeDesc typeDesc = null;

                if (func.funckind == TypeLibTypes.Interop.FUNCKIND.FUNC_DISPATCH)
                {
                    if (func.IsPropertyGet || func.IsFunc)
                    {
                        if (func.cParams == 0)
                        {
                            typeDesc = func.elemdescFunc.tdesc;
                        }
                        else if (info.TransformDispRetVal && func.cParams == 1 && func.GetElemDesc(0).paramdesc.IsRetval)
                        {
                            typeDesc = func.GetElemDesc(0).tdesc.lptdesc;
                        }
                    }
                }
                else if (func.funckind == TypeLibTypes.Interop.FUNCKIND.FUNC_PUREVIRTUAL)
                {
                    if ((func.cParams == 1) &&
                       (func.IsPropertyGet || func.IsFunc) &&
                       (func.GetElemDesc(0).paramdesc.IsRetval) &&
                       (func.GetElemDesc(0).tdesc.vt == (int)VarEnum.VT_PTR))
                    {
                        typeDesc = func.GetElemDesc(0).tdesc.lptdesc;
                    }
                }

                if (typeDesc != null)
                {
                    if (typeDesc.vt == (int)VarEnum.VT_UNKNOWN || typeDesc.vt == (int)VarEnum.VT_DISPATCH)
                    {
                        // The member returns an IUnknown* or an IDispatch* which is valid.
                        return true;
                    }
                    else if (typeDesc.vt == (int)VarEnum.VT_PTR)
                    {
                        typeDesc = typeDesc.lptdesc;
                        if (typeDesc.vt == (int)VarEnum.VT_USERDEFINED)
                        {
                            TypeInfo type = typeInfo.GetRefTypeInfo(typeDesc.hreftype);
                            using (TypeAttr attr = type.GetTypeAttr())
                            {
                                if (attr.Guid == WellKnownGuids.IID_IUnknown ||
                                   attr.Guid == WellKnownGuids.IID_IDispatch ||
                                   attr.Guid == WellKnownGuids.IID_IEnumVARIANT)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Is this dispatch property a NewEnum property with the right type and DISPID?
        /// </summary>
        static public bool IsNewEnumDispatchProperty(ConverterInfo info, TypeInfo typeInfo, VarDesc var, int index)
        {
            //
            // Support GUID_DispIdOverride
            //
            int dispid = var.memid;
            GetOverrideDispId(info, typeInfo, index, InterfaceMemberType.Variable, ref dispid, false);

            if (dispid == WellKnownDispId.DISPID_NEWENUM &&
                var.elemdescVar.paramdesc.IsRetval &&
                var.IsReadOnly
                )
            {
                TypeDesc typeDesc = var.elemdescVar.tdesc;

                if (typeDesc.vt == (int)VarEnum.VT_UNKNOWN || typeDesc.vt == (int)VarEnum.VT_DISPATCH)
                {
                    return true;
                }
                else if (typeDesc.vt == (int)VarEnum.VT_PTR)
                {
                    typeDesc = typeDesc.lptdesc;
                    if (typeDesc.vt == (int)VarEnum.VT_USERDEFINED)
                    {
                        TypeInfo type = typeInfo.GetRefTypeInfo(typeDesc.hreftype);
                        using (TypeAttr attr = type.GetTypeAttr())
                        {
                            if (attr.Guid == WellKnownGuids.IID_IUnknown ||
                               attr.Guid == WellKnownGuids.IID_IDispatch ||
                               attr.Guid == WellKnownGuids.IID_IEnumVARIANT)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Is this interface explicitly derives from IEnumerable (from mscorlib.tlb)?
        /// </summary>
        static public bool ExplicitlyImplementsIEnumerable(TypeInfo typeInfo, TypeAttr typeAttr)
        {
            return ExplicitlyImplementsIEnumerable(typeInfo, typeAttr, true);
        }

        /// <summary>
        /// Is this interface explicitly derives from IEnumerable (from mscorlib.tlb)?
        /// </summary>
        /// <param name="lookupPartner">Whether we look at the partner interface</param>
        static public bool ExplicitlyImplementsIEnumerable(TypeInfo typeInfo, TypeAttr typeAttr, bool lookupPartner)
        {
            // Look through each of the implemented/inherited interfaces
            for (int i = 0; i < typeAttr.cImplTypes; ++i)
            {
                TypeInfo interfaceTypeInfo = typeInfo.GetRefType(i);

                using (TypeAttr interfaceTypeAttr = interfaceTypeInfo.GetTypeAttr())
                {
                    if ((typeInfo.GetImplTypeFlags(i) & TypeLibTypes.Interop.IMPLTYPEFLAGS.IMPLTYPEFLAG_FSOURCE) == 0)
                    {
                        if (interfaceTypeAttr.Guid == WellKnownGuids.IID_IEnumerable)
                            return true;
                        if (ExplicitlyImplementsIEnumerable(interfaceTypeInfo, interfaceTypeAttr))
                            return true;
                    }
                }
            }

            if (lookupPartner)
            {
                TypeInfo partnerTypeInfo = typeInfo.GetRefTypeNoComThrow();
                if (partnerTypeInfo != null)
                {
                    using (TypeAttr partnerTypeAttr = partnerTypeInfo.GetTypeAttr())
                    {
                        if (partnerTypeAttr.Guid == WellKnownGuids.IID_IEnumerable)
                            return true;

                        if (ExplicitlyImplementsIEnumerable(partnerTypeInfo, partnerTypeAttr, false))
                            return true;
                    }
                }
            }

            return false;
        }

        #endregion

        static public void CreateMethodForInterface(InterfaceInfo info, InterfaceMemberInfo memberInfo)
        {
            CreateMethod(info, memberInfo, CreateMethodMode.InterfaceMethodMode);
        }

        static public void CreateMethodForDelegate(InterfaceInfo info, FuncDesc funcDesc, int index)
        {
            InterfaceMemberInfo memberInfo = new InterfaceMemberInfo(info.ConverterInfo, info.RefTypeInfo, index, INVOKE_METHOD, INVOKE_METHOD, InterfaceMemberType.Method, TypeLibTypes.Interop.INVOKEKIND.INVOKE_FUNC, funcDesc.memid, funcDesc, null);
            CreateMethod(info, memberInfo, CreateMethodMode.EventDelegateMode);
        }

        static private void CreateMethod(InterfaceInfo info, InterfaceMemberInfo memberInfo, CreateMethodMode mode)
        {
            MethodBuilder method = null;
            bool isConversionLoss = false;
            switch (memberInfo.MemberType)
            {
                // Handle normal methods as well as accesors for properties
                case InterfaceMemberType.Method:
                    method = CreateMethodInternal(info, memberInfo.RefFuncDesc, memberInfo, mode, ref isConversionLoss);
                    break;

                // Handle properties in dispatch interfaces defined as variables
                case InterfaceMemberType.Variable:
                    method = CreateMethodInternal(info, memberInfo.RefVarDesc, memberInfo, mode, ref isConversionLoss);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }

            if (method == null)
                return;

            if (isConversionLoss)
            {
                string msg = Resource.FormatString(
                    "Wrn_UnconvertableArgs", 
                    info.TypeBuilder.FullName,
                    method.Name);

                info.ConverterInfo.ReportEvent(WarningCode.Wrn_UnconvertableArgs, msg);
            }

            info.IsConversionLoss |= isConversionLoss;
        }

        /// <summary>
        /// Check for optional arguments
        /// </summary>
        static public void CheckForOptionalArguments(ConverterInfo info, FuncDesc func, out int varArg, out int firstOptArg, out int lastOptArg)
        {
            int numOfParams = func.cParams;
            varArg = firstOptArg = lastOptArg = numOfParams + 1;

            if (func.cParamsOpt == -1)
            {
                //
                // [vararg] scenario
                //

                // For propput/propputref, the last parameter will be the value to be set
                bool skipPropPutArg = false;
                if ((func.invkind & (TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYPUT | TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYPUTREF)) != 0)
                    skipPropPutArg = true;

                // vararg will be the last argument except for lcid arguments & retval arguments
                for (int target = numOfParams - 1; target >= 0; --target)
                {
                    // skip lcid/retval
                    ParamDesc paramDesc = func.GetElemDesc(target).paramdesc;
                    if (IsRetVal(info, paramDesc, func) || paramDesc.IsLCID)
                        continue;

                    // skip for property
                    if (skipPropPutArg)
                    {
                        skipPropPutArg = false;
                        continue;
                    }

                    varArg = target;
                    break;
                }

                Debug.Assert(varArg < numOfParams);
            }
            else if (func.cParamsOpt > 0)
            {
                //
                // [optional] scenario
                //
                int countOfOptionals = func.cParamsOpt;
                firstOptArg = 0;

                // find the first optional arg
                for (int target = func.cParams - 1; target >= 0; --target)
                {
                    // The count of optional params does not include any lcid params, nor does
                    // it include the return value, so skip those.
                    ParamDesc param = func.GetElemDesc(target).paramdesc;
                    if (!(param.IsRetval || param.IsRetval))
                    {
                        if (--countOfOptionals == 0)
                        {
                            firstOptArg = target;
                            break;
                        }
                    }
                }

                lastOptArg = firstOptArg + func.cParamsOpt;
            }
        }

        static private MethodBuilder CreateMethodInternal(InterfaceInfo info, FuncDesc func, InterfaceMemberInfo memberInfo, CreateMethodMode mode, ref bool isConversionLoss)
        {
            bool isNewEnumMember = false;

            if (info.AllowNewEnum && IsNewEnumFunc(info.ConverterInfo, info.RefTypeInfo, func, memberInfo.Index))
            {
                info.AllowNewEnum = false;
                isNewEnumMember = true;

                if (mode == CreateMethodMode.EventDelegateMode)
                {
                    info.ConverterInfo.ReportEvent(
                        WarningCode.Wrn_EventWithNewEnum,
                        Resource.FormatString("Wrn_EventWithNewEnum", info.RefTypeInfo.GetDocumentation()));
                }
            }

            //
            // Optional Arguments
            //
            int varArg;             // index of the vararg argument
            int firstOptArg;        // index of the first optional argument
            int lastOptArg;         // index of the last optional argument
            CheckForOptionalArguments(info.ConverterInfo, func, out varArg, out firstOptArg, out lastOptArg);

            //
            // Figure out types
            //
            ReturnKind returnKind;
            TypeConverter retTypeConverter;
            Type retType;
            bool isStandardOleCall;
            int lcidArg;
            int retArgId;
            TypeConverter[] paramTypeConverters = GenerateParameterTypes(info, memberInfo, func, isNewEnumMember, varArg, out lcidArg, out isStandardOleCall, out retTypeConverter, out returnKind, out retArgId);

            Type[] paramTypes = new Type[paramTypeConverters.Length];
            for (int i = 0; i < paramTypeConverters.Length; ++i)
                paramTypes[i] = paramTypeConverters[i].ConvertedType;

            if (retTypeConverter != null)
                retType = retTypeConverter.ConvertedType;
            else
                retType = null;

            MethodBuilder methodBuilder = CreateMethodCore(info, memberInfo, isNewEnumMember, mode, isStandardOleCall, retType, paramTypes);

            //
            // Emit LCIDConversionAttribute
            //
            if (lcidArg >= 0)
                methodBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForLCIDConversion(lcidArg));

            int cParams = func.cParams;
            int cParamsIter = cParams;

            // If there is a return value, skip the last param
            if (returnKind == ReturnKind.RetValParameter)
            {
                ProcessParam(info, memberInfo, func.GetElemDesc(retArgId), retTypeConverter, methodBuilder, 0, "", false, false);

                isConversionLoss |= retTypeConverter.IsConversionLoss;
            }
            else if (returnKind == ReturnKind.ReturnValue)
            {
                ProcessParam(info, memberInfo, func.elemdescFunc, retTypeConverter, methodBuilder, 0, "", false, false);

                isConversionLoss |= retTypeConverter.IsConversionLoss;
            }

            // First string is the method name so offset by one
            String[] saNames = info.RefTypeInfo.GetNames(func.memid, cParams + 1);

            bool isVarArg;
            bool isOptionalArg;

            //
            // Process parameters
            //
            int paramIndex = 0;
            for (int n = 0; n < cParamsIter; ++n)
            {
                ElemDesc elem = func.GetElemDesc(n);

                // Skip LCID
                if (elem.paramdesc.IsLCID)
                    continue;

                // Skip the return parameter
                if (returnKind == ReturnKind.RetValParameter && n == retArgId)
                    continue;

                isOptionalArg = (n >= firstOptArg && n <= lastOptArg);
                isVarArg = (n == varArg);

                ProcessParam(info, memberInfo, elem, paramTypeConverters[paramIndex], methodBuilder, paramIndex + 1, saNames[n + 1], isVarArg, isOptionalArg);

                isConversionLoss |= paramTypeConverters[paramIndex].IsConversionLoss;
                paramIndex++;
            }

            //
            // Emit TypeLibFuncAttribute if necessary
            //
            if (func.wFuncFlags != 0)
                methodBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForTypeLibFunc((TypeLibFuncFlags)func.wFuncFlags));

            //
            // Handle DefaultMemberAttribute
            //
            if (!memberInfo.IsProperty && memberInfo.DispId == WellKnownDispId.DISPID_VALUE)
            {
                // DIFF: TlbImpv1 use the type library name while we use the unique name
                info.ConverterInfo.SetDefaultMember(info.TypeBuilder, methodBuilder.Name);
            }

            return methodBuilder;
        }

        static private MethodBuilder CreateMethodInternal(InterfaceInfo info, VarDesc var, InterfaceMemberInfo memberInfo, CreateMethodMode mode, ref bool isConversionLoss)
        {
            bool convertingNewEnumMember = IsNewEnumDispatchProperty(info.ConverterInfo, info.RefTypeInfo, var, memberInfo.Index);

            Type retType = null;
            Type[] paramTypes = null;
            TypeConverter propTypeConverter = new TypeConverter(info.ConverterInfo, info.RefTypeInfo, var.elemdescVar.tdesc, ConversionType.ReturnValue);

            isConversionLoss |= propTypeConverter.IsConversionLoss;
            Type propType = propTypeConverter.ConvertedType;

            int propTypeParamIndex = 0;  // The index of the function parameter that represents the property type
            if (memberInfo.InvokeKind == TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYGET)
            {
                retType = propType;
                paramTypes = new Type[] { };
                propTypeParamIndex = 0;     // for Type get_XXX(). Index = 0
            }
            else if (memberInfo.InvokeKind == TypeLibTypes.Interop.INVOKEKIND.INVOKE_PROPERTYPUT)
            {
                retType = typeof(void);
                paramTypes = new Type[] { propType };
                propTypeParamIndex = 1;     // for void set_XXX(Type arg). Index = 1
            }
            else
            {
                Debug.Assert(false, "Should not get here!");
            }

            MethodBuilder methodBuilder = CreateMethodCore(info, memberInfo, convertingNewEnumMember, mode, true, retType, paramTypes);
            ProcessParam(info, memberInfo, var.elemdescVar, propTypeConverter, methodBuilder, propTypeParamIndex, "", false, false);

            return methodBuilder;
        }

        static public int GetIndexOfFirstMethod(InterfaceInfo info)
        {
            TypeInfo type = info.RefTypeInfo;
            TypeAttr attr = info.RefTypeAttr;
            return GetIndexOfFirstMethod(type, attr);
        }

        /// <summary>
        /// This function is used to workaround around the fact that the TypeInfo might return IUnknown/IDispatch methods (in the case of dual interfaces)
        /// So we should always call this function to get the first index for different TypeInfo and never save the id
        /// </summary>
        static public int GetIndexOfFirstMethod(TypeInfo type, TypeAttr attr)
        {
            if (attr.typekind != TypeLibTypes.Interop.TYPEKIND.TKIND_DISPATCH) return 0;

            int nIndex = 0;
            if (attr.cFuncs >= 3)
            {
                // Check for IUnknown first
                using (FuncDesc func = type.GetFuncDesc(0))
                {
                    if (func.memid == 0x60000000 &&
                       func.elemdescFunc.tdesc.vt == (int)VarEnum.VT_VOID &&
                       func.cParams == 2 &&
                       func.GetElemDesc(0).tdesc.vt == (int)VarEnum.VT_PTR &&
                       func.GetElemDesc(1).tdesc.vt == (int)VarEnum.VT_PTR &&
                       "QueryInterface" == type.GetDocumentation(func.memid))
                    {
                        nIndex = 3;
                    }
                }

                if (attr.cFuncs >= 7)
                {
                    using (FuncDesc func = type.GetFuncDesc(3))
                    {
                        // Check IDispatch
                        if (func.memid == 0x60010000 &&
                            func.elemdescFunc.tdesc.vt == (int)VarEnum.VT_VOID &&
                            func.cParams == 1 &&
                            func.GetElemDesc(0).tdesc.vt == (int)VarEnum.VT_PTR &&
                            "GetTypeInfoCount" == type.GetDocumentation(func.memid))
                        {
                            nIndex = 7;
                        }
                    }
                }
            }
            return nIndex;
        }

        /// <summary>
        /// Create methods using the InterfaceInfo
        /// </summary>
        static private void CreateMethods(InterfaceInfo info)
        {
            //
            // Stop if info is already IUnknown. Doesn't stop for IDispatch because we want to convert the members of IDispatch
            //
            if (WellKnownGuids.IID_IUnknown == info.RefTypeAttr.Guid)
                return;

            //
            // Create methods for parent interface. We need to duplicate them.
            //
            if (info.RefTypeAttr.cImplTypes == 1)
            {
                TypeInfo parent = info.RefTypeInfo.GetRefType(0);
                using (TypeAttr parentAttr = parent.GetTypeAttr())
                {
                    if (WellKnownGuids.IID_IUnknown != parentAttr.Guid && WellKnownGuids.IID_IDispatch != parentAttr.Guid)
                    {
                        InterfaceInfo parentInterfaceInfo = new InterfaceInfo(info.ConverterInfo, info.TypeBuilder, info.EmitDispId, parent, parentAttr, info.IsCoClass, info.IsSource, info.CurrentImplementingInterface);
                        parentInterfaceInfo.IsDefaultInterface = info.IsDefaultInterface;
                        parentInterfaceInfo.CurrentSlot = info.CurrentSlot;
                        ConvCommon.CreateInterfaceCommon(parentInterfaceInfo);
                        info.CurrentSlot = parentInterfaceInfo.CurrentSlot;

                        /*
                        info.PushType(parent, parentAttr);
                        CreateMethods(info);
                        info.PopType();
                         */
                    }
                    else
                    {
                        // Initialize v-table slot for IUnknown/IDispatch
                        info.CurrentSlot = parentAttr.cbSizeVft / parentAttr.cbSizeInstance;
                    }
                }
            }

            //
            // Create methods for normal methods & property accessors
            //
            IConvInterface convInterface = (IConvInterface)info.ConverterInfo.GetTypeRef(ConvType.Interface, info.RefTypeInfo);

            info.AllowNewEnum = !convInterface.ImplementsIEnumerable;

            foreach (InterfaceMemberInfo memberInfo in convInterface.AllMembers)
            {
                CreateMethodForInterface(info, memberInfo);
            }
            
            //
            // Generate the properties
            //
            info.PropertyInfo.GenerateProperties();

        }

        /// <summary>
        /// This one should be the entry point as it deals with the funky partner interfaces
        /// </summary>
        /// <param name="info"></param>
        static public void CreateInterfaceCommon(InterfaceInfo info)
        {
            CreateInterfaceCommonInternal(info, false);
        }

        /// <summary>
        /// Create methods for a event interface
        /// </summary>
        /// <param name="eventInterfaceInfo">
        /// InterfaceInfo for the source interface, including the type builder for the event interface. 
        /// Note that you can pass whatever type builder you want.
        /// </param>
        public static void CreateEventInterfaceCommon(InterfaceInfo eventInterfaceInfo)
        {
            CreateInterfaceCommonInternal(eventInterfaceInfo, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info">The interfaceInfo</param>
        /// <param name="isCreateEventInterface">
        /// True if we are creating event interface, false if creating a normal interface
        /// </param>
        static public void CreateInterfaceCommonInternal(InterfaceInfo info, bool isCreateEventInterface)
        {
            bool bIsIDispatchBasedOnInterface = false;

            // I finally figured this part out
            // For dual interfaces, it has a "funky" TKIND_DISPATCH|TKIND_DUAL interface with a parter of TKIND_INTERFACE|TKIND_DUAL interface
            // The first one is pretty bad and has duplicated all the interface members of its parent, which is not we want
            // We want the second v-table interface
            // So, if we indeed has seen this kind of interface, prefer its partner
            // However, we should not blindly get the partner because those two interfaces partners with each other
            // So we need to first test to see if the interface is both dispatch & dual, and then get its partner interface
            if (info.RefTypeAttr.IsDual && info.RefTypeAttr.IsDispatch)
            {
                TypeInfo typeReferencedType = info.RefTypeInfo.GetRefTypeNoComThrow();
                if (typeReferencedType != null)
                {
                    using (TypeAttr attrReferencedType = typeReferencedType.GetTypeAttr())
                    {
                        // Either eliminate the type stack stuff or put PropertyInfo stuff elsewhere
                        // We cannot keep properties on the same interface info as it will cause problems
                        info.PushType(typeReferencedType, attrReferencedType);
                        if (isCreateEventInterface)
                            CreateEventMethods(info);
                        else
                            CreateMethods(info);
                        info.PopType();
                        bIsIDispatchBasedOnInterface = true;
                    }
                }
            }

            if (!bIsIDispatchBasedOnInterface)
            {
                if (isCreateEventInterface)
                    CreateEventMethods(info);
                else
                    CreateMethods(info);
            }
        }

        /// <summary>
        /// Create methods on event interface recursively
        /// </summary>
        /// <param name="eventInterfaceInfo">InterfaceInfo of the event interface</param>
        public static void CreateEventMethods(InterfaceInfo eventInterfaceInfo)
        {
            IConvInterface convInterface = (IConvInterface)eventInterfaceInfo.ConverterInfo.GetInterface(eventInterfaceInfo.RefTypeInfo, eventInterfaceInfo.RefTypeAttr);
            if (convInterface.EventInterface == null)
            {
                convInterface.DefineEventInterface();
            }

            //
            // If we are creating the co-class, create the real event interface first in case it doesn't exist yet
            // Note that as event interface doesn't have inheritance, we'll have to do it outside of CreateEventMethodsInternal
            //
            if (eventInterfaceInfo.IsCoClass)
                convInterface.EventInterface.Create();

            // Then create the methods
            CreateEventMethodsInternal(convInterface.EventInterface, eventInterfaceInfo);
        }

        public static void CreateEventMethodsInternal(IConvEventInterface convEventInterface, InterfaceInfo eventInterfaceInfo)
        {
            //
            // Stop if info is already IUnknown. Doesn't stop for IDispatch because we want to convert the members of IDispatch
            //
            if (WellKnownGuids.IID_IUnknown == eventInterfaceInfo.RefTypeAttr.Guid)
                return;

            //
            // Create methods for parent interface. We need to duplicate them.
            //
            if (eventInterfaceInfo.RefTypeAttr.cImplTypes == 1)
            {
                TypeInfo parent = eventInterfaceInfo.RefTypeInfo.GetRefType(0);
                using (TypeAttr parentAttr = parent.GetTypeAttr())
                {
                    if (WellKnownGuids.IID_IUnknown != parentAttr.Guid && WellKnownGuids.IID_IDispatch != parentAttr.Guid)
                    {
                        InterfaceInfo parentInterfaceInfo = new InterfaceInfo(eventInterfaceInfo.ConverterInfo, eventInterfaceInfo.TypeBuilder, eventInterfaceInfo.EmitDispId, parent, parentAttr, eventInterfaceInfo.IsCoClass, eventInterfaceInfo.IsSource, eventInterfaceInfo.CurrentImplementingInterface);
                        parentInterfaceInfo.IsDefaultInterface = eventInterfaceInfo.IsDefaultInterface;
                        parentInterfaceInfo.CurrentSlot = eventInterfaceInfo.CurrentSlot;
                        ConvCommon.CreateEventMethodsInternal(convEventInterface, parentInterfaceInfo);
                        eventInterfaceInfo.CurrentSlot = parentInterfaceInfo.CurrentSlot;

                        /*
                        info.PushType(parent, parentAttr);
                        CreateMethods(info);
                        info.PopType();
                         */
                    }
                    else
                    {
                        // Initialize v-table slot for IUnknown/IDispatch
                        eventInterfaceInfo.CurrentSlot = parentAttr.cbSizeVft / parentAttr.cbSizeInstance;
                    }
                }
            }

            ConverterInfo converterInfo = eventInterfaceInfo.ConverterInfo;
            TypeInfo type = eventInterfaceInfo.RefTypeInfo;

            ModuleBuilder moduleBuilder = eventInterfaceInfo.ConverterInfo.ModuleBuilder;

            TypeAttr attr = eventInterfaceInfo.RefTypeAttr;

            // If we are creating the co-class, create the real event interface first in case it doesn't exist yet
            IConvInterface convInterface = (IConvInterface)converterInfo.GetInterface(type, attr);

            // For every method in the source interface
            foreach (InterfaceMemberInfo memInfo in convInterface.AllMembers)
            {
                if (memInfo.IsProperty) continue;

                FuncDesc func = memInfo.RefFuncDesc;
                
                // Get/Create the event delegate. 
                Type delegateType = convEventInterface.GetEventDelegate(memInfo);
                
                // Need to use the same name as the source interface, otherwise TCEAdapterGenerator will fail
                string eventName = memInfo.UniqueName;

                MethodAttributes methodAttributes =
                    MethodAttributes.Public | MethodAttributes.HideBySig |
                    MethodAttributes.NewSlot | MethodAttributes.Virtual;
                if (!eventInterfaceInfo.IsCoClass)
                {
                    methodAttributes |= System.Reflection.MethodAttributes.Abstract;
                }

                TypeBuilder eventInterfaceType = eventInterfaceInfo.TypeBuilder;

                Type[] paramTypes = new Type[] { delegateType };

                //
                // Defined add_<Event> method
                // Note: We are passing null into GenerateUniqueMemberName so that add_XXX/remove_XXX doesn't take overloading into
                // account. The reason is that we want to keep the add_XXX/remove_XXX methods aligned with the name of the event,
                // which also doesn't consider overloading
                //
                MethodBuilder addMethodBuilder = eventInterfaceType.DefineMethod(
                    // Generate unique name
                    eventInterfaceInfo.GenerateUniqueMemberName("add_" + eventName, null, MemberTypes.Method),
                    methodAttributes,
                    typeof(void),
                    paramTypes
                    );

                addMethodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed | MethodImplAttributes.InternalCall);

                // Do a member override for add_<Event> method if we are creating the method for coclass
                if (eventInterfaceInfo.IsCoClass)
                {
                    Debug.Assert(eventInterfaceInfo.CurrentImplementingInterface != null);

                    Type implementingInterfaceType = convEventInterface.RealManagedType;

                    // It is not possible to have a name conflict in event interface, so always use "add_XXX"
                    MethodInfo methodInfo = implementingInterfaceType.GetMethod("add_" + eventName, paramTypes);
                    eventInterfaceInfo.TypeBuilder.DefineMethodOverride(addMethodBuilder, methodInfo);
                }

                //
                // Define remove_<Event> method
                // Note: We are passing null into GenerateUniqueMemberName so that add_XXX/remove_XXX doesn't take overloading into
                // account. The reason is that we want to keep the add_XXX/remove_XXX methods aligned with the name of the event,
                // which also doesn't consider overloading
                //
                MethodBuilder removeMethodBuilder = eventInterfaceType.DefineMethod(
                    eventInterfaceInfo.GenerateUniqueMemberName("remove_" + eventName, null, MemberTypes.Method),
                    methodAttributes,
                    typeof(void),
                    paramTypes
                    );

                removeMethodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed | MethodImplAttributes.InternalCall);

                // Do a member override for remove_<Event> method if we are creating the method for coclass
                if (eventInterfaceInfo.IsCoClass)
                {
                    Debug.Assert(eventInterfaceInfo.CurrentImplementingInterface != null);

                    Type implementingInterfaceType = convEventInterface.RealManagedType;

                    // It is not possible to have a name conflict in event interface, so always use "remove_XXX"
                    MethodInfo methodInfo = implementingInterfaceType.GetMethod("remove_" + eventName, paramTypes);
                    eventInterfaceType.DefineMethodOverride(removeMethodBuilder, methodInfo);
                }

                //
                // Define event
                //
                EventBuilder eventBuilder = eventInterfaceType.DefineEvent(
                    eventInterfaceInfo.GenerateUniqueMemberName(eventName, null, MemberTypes.Event),
                    EventAttributes.None,
                    delegateType);
                eventBuilder.SetAddOnMethod(addMethodBuilder);
                eventBuilder.SetRemoveOnMethod(removeMethodBuilder);
            }
        }

        /// <summary>
        /// Whether the ITypeInfo is derived from IDispatch
        /// </summary>
        public static bool IsDerivedFromIDispatch(TypeInfo typeInfo)
        {
            return IsDerivedFromIID(typeInfo, WellKnownGuids.IID_IDispatch);
        }

        /// <summary>
        /// Whether the ITypeInfo is derived from IDispatch
        /// </summary>
        public static bool IsDerivedFromIUnknown(TypeInfo typeInfo)
        {
            return IsDerivedFromIID(typeInfo, WellKnownGuids.IID_IUnknown);
        }

        /// <summary>
        /// Whether the ITypeInfo is derived from IDispatch. 
        /// </summary>
        /// <return>True if ITypeInfo is derive from IID, false otherwise (including when ITypeInfo is IID)</return>
        public static bool IsDerivedFromIID(TypeInfo typeInfo, Guid iid)
        {
            using (TypeAttr attr = typeInfo.GetTypeAttr())
            {
                // Return false if the ITypeInfo is IID
                if (attr.Guid == iid)
                    return false;

                return IsDerivedFromIIDInternal(typeInfo, iid);
            }
        }

        private static bool IsDerivedFromIIDInternal(TypeInfo typeInfo, Guid iid)
        {
            using (TypeAttr attr = typeInfo.GetTypeAttr())
            {
                if (attr.Guid == iid)
                    return true;

                // If we've seen a IUnknown (note that if the iid is IUnknown, we've tested that already)),
                // we've recused far enough
                if (attr.Guid == WellKnownGuids.IID_IUnknown)
                    return false;

                if (attr.cImplTypes == 1)
                {
                    TypeInfo parent = typeInfo.GetRefType(0);
                    return IsDerivedFromIIDInternal(parent, iid);
                }
            }

            return false;
        }

        public static bool InterfaceSupportsDispatch(TypeInfo typeInfo, TypeAttr attr)
        {
            return attr.IsDual || attr.typekind == TypeLibTypes.Interop.TYPEKIND.TKIND_DISPATCH;
        }

        public static int GetPointerSize(TypeInfo typeInfo)
        {
            using (TypeAttr attr = typeInfo.GetTypeAttr())
            {
                if (attr.Guid == WellKnownGuids.IID_IUnknown || attr.Guid == WellKnownGuids.IID_IDispatch)
                {
                    return attr.cbSizeInstance;
                }

                if (attr.cImplTypes == 1)
                {
                    TypeInfo parent = typeInfo.GetRefType(0);
                    return GetPointerSize(parent);
                }

                return attr.cbSizeInstance;
            }
        }

        /// <summary>
        /// Create the constant fields on the TypeBuilder according to the VarDesc in the type
        /// </summary>
        public static void CreateConstantFields(ConverterInfo info, TypeInfo type, TypeBuilder typeBuilder, ConvType convType)
        {
            using (TypeAttr attr = type.GetTypeAttr())
            {
                int cVars = attr.cVars;
                for (int n = 0; n < cVars; ++n)
                {
                    using (VarDesc var = type.GetVarDesc(n))
                    {
                        string fieldName = type.GetDocumentation(var.memid);

                        // We don't want the same conversion rules as Field for VT_BOOL and VT_ARRAY, so use Element instead
                        // Basically Element is the same as Field except that it doesn't have special rules for VT_BOOL/VT_ARRAY
                        TypeConverter typeConverter = new TypeConverter(info, type, var.elemdescVar.tdesc, ConversionType.Element);
                        if (typeConverter.ConvertedType == typeof(DateTime))
                            typeConverter = new TypeConverter(typeof(float));

                        Type fieldType = typeConverter.ConvertedType;
                        
                        if (typeConverter.IsConversionLoss)
                        {
                            //
                            // Emit Wrn_UnconvertableField warning
                            //
                            info.ReportEvent(
                                WarningCode.Wrn_UnconvertableField,
                                Resource.FormatString("Wrn_UnconvertableField", typeBuilder.FullName, fieldName));
                        }


                        Type targetType;
                        if (convType == ConvType.Enum)
                            targetType = typeBuilder;   // use enum type as the field type for enum
                        else
                            targetType = fieldType;     // use the real type as the field type

                        FieldBuilder fieldBuilder = typeBuilder.DefineField(fieldName, targetType, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
                        typeConverter.ApplyAttributes(fieldBuilder);

                        //
                        // Emit TypeLibVarAttribute if necessary
                        //
                        if (var.wVarFlags != 0)
                        {
                            fieldBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForTypeLibVar((TypeLibVarFlags)var.wVarFlags));
                        }

                        // Emit constant for static fields
                        ConvCommon.SetDefaultValue(var, fieldType, fieldBuilder);
                    }
                }
            }
        }

        private static string GetAliasName(ConverterInfo info, TypeInfo typeInfo, TypeDesc typeDesc)
        {
            // Drill down to the actual type that is pointed to.
            while (typeDesc.vt == (int)VarEnum.VT_PTR)
                typeDesc = typeDesc.lptdesc;

            // If the parameter is an alias then we need to add a custom attribute to the 
            // parameter that describes the alias.
            if (typeDesc.vt == (int)VarEnum.VT_USERDEFINED)
            {
                TypeInfo refTypeInfo = typeInfo.GetRefTypeInfo(typeDesc.hreftype);
                using (TypeAttr refTypeAttr = refTypeInfo.GetTypeAttr())
                {
                    if (refTypeAttr.typekind == TypeLibTypes.Interop.TYPEKIND.TKIND_ALIAS)
                    {
                        return info.GetManagedName(refTypeInfo);
                    }
                }
            }

            return null;
        }

        public static void HandleAlias(ConverterInfo info, TypeInfo typeInfo, TypeDesc typeDesc, ParameterBuilder builder)
        {
            string aliasName = GetAliasName(info, typeInfo, typeDesc);
            if (aliasName != null)
                builder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForComAliasName(aliasName));
        }

        public static void HandleAlias(ConverterInfo info, TypeInfo typeInfo, TypeDesc typeDesc, PropertyBuilder builder)
        {
            string aliasName = GetAliasName(info, typeInfo, typeDesc);
            if (aliasName != null)
                builder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForComAliasName(aliasName));
        }

        public static void HandleAlias(ConverterInfo info, TypeInfo typeInfo, TypeDesc typeDesc, FieldBuilder builder)
        {
            string aliasName = GetAliasName(info, typeInfo, typeDesc);
            if (aliasName != null)
                builder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForComAliasName(aliasName));
        }

        /// <summary>
        /// Gets the non-aliased type if it is a alias
        /// </summary>
        public static TypeInfo GetAlias(TypeInfo typeInfo)
        {
            //
            // Support for alias
            // If it is an alias, then it must be an alias to a user defined type, which we'll duplicate
            // all the definitions under the alias' name
            //
            using (TypeAttr typeAttr = typeInfo.GetTypeAttr())
            {
                if (typeAttr.IsAlias)
                {
                    TypeInfo resolvedTypeInfo;
                    TypeAttr resolvedTypeAttr;

                    ConvCommon.ResolveAlias(typeInfo, typeAttr.tdescAlias, out resolvedTypeInfo, out resolvedTypeAttr);

                    // If we resolved to an alias, this means this is an alias of built-in type and should never be 
                    // passed in
                    Debug.Assert(!resolvedTypeAttr.IsAlias);

                    return resolvedTypeInfo;
                }
                else
                    return typeInfo;
            }
        }

        public static TypeInfo GetDefaultInterface(TypeInfo coclass)
        {
            TypeInfo typeInfo = coclass;
            TypeAttr typeAttr = coclass.GetTypeAttr();

            TypeInfo defaultInterface = null;

            for (int i = 0; i < typeAttr.cImplTypes; ++i)
            {
                TypeLibTypes.Interop.IMPLTYPEFLAGS flags = typeInfo.GetImplTypeFlags(i);

                // Non-source default interface?
                if ((flags & (TypeLibTypes.Interop.IMPLTYPEFLAGS.IMPLTYPEFLAG_FSOURCE | TypeLibTypes.Interop.IMPLTYPEFLAGS.IMPLTYPEFLAG_FDEFAULT)) == TypeLibTypes.Interop.IMPLTYPEFLAGS.IMPLTYPEFLAG_FDEFAULT)
                {
                    return typeInfo.GetRefType(i);
                }
                else if ((flags & TypeLibTypes.Interop.IMPLTYPEFLAGS.IMPLTYPEFLAG_FSOURCE) == 0 && defaultInterface == null)
                {
                    defaultInterface = typeInfo.GetRefType(i);
                }
            }

            return defaultInterface;
        }

        /// <summary>
        /// Check whether the interface, which the type "extendedType" wants to implement, is a class interface
        /// exported by TlbExp.
        /// We do not support this scenario, and an exception will be thrown.
        /// </summary>
        internal static void ThrowIfImplementingExportedClassInterface(
            TypeInfo extendedType, IConvInterface parentInterface)
        {
            TypeInfo parentType = parentInterface.RefTypeInfo;
            TypeLib parentTypeLib = parentType.GetContainingTypeLib();
            TypeLib thisTypeLib = extendedType.GetContainingTypeLib();

            string asmName = parentTypeLib.GetCustData(CustomAttributeGuids.GUID_ExportedFromComPlus) as string;
            if (asmName != null)
            {
                string parentName = parentType.GetCustData(CustomAttributeGuids.GUID_ManagedName) as string;
                Type parentManagedType = parentInterface.RealManagedType;
                if (parentName != null && parentManagedType != null &&
                    parentManagedType.IsClass)
                {
                    string msg = Resource.FormatString("Err_ImplementExportedClassInterface",
                        new object[] { extendedType.GetDocumentation(), thisTypeLib.GetDocumentation(),
                                parentType.GetDocumentation(), parentTypeLib.GetDocumentation() });
                    throw new TlbImpGeneralException(msg, ErrorCode.Err_ImplementExportedClassInterface);
                }
            }
        }
    }
}
