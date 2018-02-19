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

namespace tlbimp2
{
    /// <summary>
    /// Interface for union conversion
    /// </summary>
    interface IConvUnion : IConvBase
    {
    }

    /// <summary>
    /// Conversion from a local ITypeInfo to a managed union
    /// </summary>
    class ConvUnionLocal : ConvLocalBase, IConvUnion
    {
        public ConvUnionLocal(ConverterInfo info, TypeInfo type)
        {
            DefineType(info, type, true);
        }

        protected override void OnDefineType()
        {
            TypeInfo typeInfo = RefNonAliasedTypeInfo;

            m_typeBuilder = ConvCommon.DefineTypeHelper(
                m_info,
                RefTypeInfo,
                RefNonAliasedTypeInfo,
                TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.ExplicitLayout,
                typeof(ValueType),
                ConvType.Union
                );

            // Handle [TypeLibType(...)] if evaluate to non-0
            using (TypeAttr refTypeAttr = RefTypeInfo.GetTypeAttr())
            {
                if (refTypeAttr.wTypeFlags != 0)
                    m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForTypeLibType((TypeLibTypeFlags)refTypeAttr.wTypeFlags));
            }

            m_info.AddToSymbolTable(RefTypeInfo, ConvType.Union, this);
            m_info.RegisterType(m_typeBuilder, this);
        }

        /// <summary>
        /// Test whether the specified VT_RECORD contains any field that can be converted to a managed reference type
        /// </summary>
        private bool HasObjectFields(TypeInfo typeInfo)
        {
            TypeAttr typeAttr = typeInfo.GetTypeAttr();
            for (int i = 0; i < typeAttr.cVars; ++i)
            {
                VarDesc var = typeInfo.GetVarDesc(i);
                if (IsObjectType(typeInfo, var.elemdescVar.tdesc))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Test whether the typeDesc corresponds to a managed reference type
        /// </summary>
        private bool IsObjectType(TypeInfo typeInfo, TypeDesc typeDesc)
        {
            int nativeIndirection = 0;
            int vt = typeDesc.vt;

            // Strip off leading VT_PTR and VT_BYREF
            while (vt == (int)VarEnum.VT_PTR)
            {
                typeDesc = typeDesc.lptdesc;
                vt = typeDesc.vt;
                nativeIndirection++;
            }

            if ((vt & (int)VarEnum.VT_BYREF) != 0)
            {
                vt &= ~(int)VarEnum.VT_BYREF;
                nativeIndirection++;
            }

            // Determine if the field is/has object type.
            Debug.Assert(vt != (int)VarEnum.VT_PTR);

            switch ((VarEnum)vt)
            {
                // These are object types.
                case VarEnum.VT_BSTR:
                case VarEnum.VT_DISPATCH:
                case VarEnum.VT_VARIANT:
                case VarEnum.VT_UNKNOWN:
                case VarEnum.VT_SAFEARRAY:
                case VarEnum.VT_LPSTR:
                case VarEnum.VT_LPWSTR:
                    return true;

                // A user-defined may or may not be/contain Object type.
                case VarEnum.VT_USERDEFINED:
                    // User defined type.  Get the TypeInfo.
                    TypeInfo refTypeInfo = typeInfo.GetRefTypeInfo(typeDesc.hreftype);
                    TypeAttr refTypeAttr = refTypeInfo.GetTypeAttr();

                    // Some user defined class.  Is it a value class, or a VOS class?
                    switch (refTypeAttr.typekind)
                    {
                        // Alias -- Is the aliased thing an Object type?
                        case TYPEKIND.TKIND_ALIAS:
                            return IsObjectType(refTypeInfo, refTypeAttr.tdescAlias);

                        // Record/Enum/Union -- Does it contain an Object type?
                        case TYPEKIND.TKIND_RECORD:
                        case TYPEKIND.TKIND_ENUM:
                        case TYPEKIND.TKIND_UNION:
                            // Byref/Ptrto record is Object.  Contained record might be.
                            if (nativeIndirection > 0)
                                return true;
                            else
                                return HasObjectFields(refTypeInfo);

                        // Class/Interface -- An Object Type.
                        case TYPEKIND.TKIND_INTERFACE:
                        case TYPEKIND.TKIND_DISPATCH:
                        case TYPEKIND.TKIND_COCLASS:
                            return true;

                        default:
                            return true;

                    } // switch (psAttrAlias->typekind)

                case VarEnum.VT_CY:
                case VarEnum.VT_DATE:
                case VarEnum.VT_DECIMAL:
                    // Pointer to the value type is an object.  Contained one isn't.
                    if (nativeIndirection > 0)
                        return true;
                    else
                        return false;

                // A fixed array is an Object type.
                case VarEnum.VT_CARRAY:
                    return true;

                // Other types I4, etc., are not Object types.
                default:
                    return false;
            } // switch (vt=pType->vt)
        }

        private void CreateField(TypeInfo type, TypeAttr attr, VarDesc var, ref bool isConversionLoss)
        {
            if (IsObjectType(type, var.elemdescVar.tdesc))
            {
                isConversionLoss = true;
            }
            else
            {

                TypeConverter typeConverter = new TypeConverter(m_info, type, var.elemdescVar.tdesc, ConversionType.Field);
                Type fieldType = typeConverter.ConvertedType;

                // TlbImp2 will only skip reference types, instead of skipping every field
                // Also, TlbImp1 will skip ValueType *, which doesn't make any sense. TlbImp2 will keep ValueType * as IntPtr

                string fieldName = type.GetDocumentation(var.memid);
                // Generates the [FieldOffset(0)] layout declarations that approximate unions in managed code
                FieldBuilder field = m_typeBuilder.DefineField(fieldName, fieldType, FieldAttributes.Public);
                field.SetCustomAttribute(CustomAttributeHelper.GetBuilderForFieldOffset(0));
                typeConverter.ApplyAttributes(field);

                isConversionLoss |= typeConverter.IsConversionLoss;

                //
                // Emit ComConversionLossAttribute for fields if necessary
                //
                if (typeConverter.IsConversionLoss)
                {
                    field.SetCustomAttribute(CustomAttributeHelper.GetBuilderForComConversionLoss());

                    //
                    // Emit Wrn_UnconvertableField warning
                    //
                    m_info.ReportEvent(
                        WarningCode.Wrn_UnconvertableField,
                        Resource.FormatString("Wrn_UnconvertableField", type.GetDocumentation(), fieldName));
                }
            }
        }

        #region IConvBase Members

        public override void OnCreate()
        {
            string name = ManagedName;
            if (m_type != null)
                return;

            TypeInfo typeInfo = RefNonAliasedTypeInfo;
            using (TypeAttr typeAttr = typeInfo.GetTypeAttr())
            {
                //
                // Create fields
                //
                int cVars = typeAttr.cVars;
                bool isConversionLoss = false;

                for (int n = 0; n < cVars; ++n)
                {
                    using (VarDesc var = typeInfo.GetVarDesc(n))
                    {
                        CreateField(typeInfo, typeAttr, var, ref isConversionLoss);
                    }
                }

                //
                // Emit StructLayout(LayoutKind.Sequential, Pack=cbAlignment)
                //
                m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForStructLayout(LayoutKind.Explicit, typeAttr.cbAlignment, typeAttr.cbSizeInstance));

                //
                // Emit ComConversionLossAttribute if necessary
                //
                if (isConversionLoss)
                    m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForComConversionLoss());

                //
                // Emit SerializableAttribute for /transform:serializablevalueclasses
                //
                if ((m_info.Settings.m_flags & TypeLibImporterFlags.SerializableValueClasses) != 0)
                    m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForSerializable());

                m_type = m_typeBuilder.CreateType();
            }
        }

        public override ConvType ConvType
        {
            get { return ConvType.Union; }
        }

        #endregion
    }

    /// <summary>
    /// Represents a managed union that is already created
    /// </summary>
    class ConvUnionExternal : IConvUnion
    {
        public ConvUnionExternal(ConverterInfo info, TypeInfo typeInfo, Type managedType)
        {
            m_typeInfo = typeInfo;
            m_managedType = managedType;

            info.AddToSymbolTable(typeInfo, ConvType.Union, this);
            info.RegisterType(managedType, this);
        }

        #region IConvBase Members

        public void Create()
        {
            // Do nothing as the type is already created
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
            get { return m_managedType; }
        }

        public Type RealManagedType
        {
            get { return m_managedType; }
        }

        public ConvType ConvType
        {
            get { return ConvType.Union; }
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

        private TypeInfo m_typeInfo;        // Corresponding type info
        private Type m_managedType;         // Corresponding managed type. Already created in a different DLL

        #endregion
    }

}
