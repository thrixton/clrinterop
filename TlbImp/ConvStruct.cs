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
    /// Interface for struct conversion
    /// </summary>
    interface IConvStruct : IConvBase
    {
    }
    
    /// <summary>
    /// Conversion from a local ITypeInfo to a managed struct
    /// </summary>
    class ConvStructLocal : ConvLocalBase, IConvStruct
    {
        public ConvStructLocal(ConverterInfo info, TypeInfo type)
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
                TypeAttributes.Public | TypeAttributes.BeforeFieldInit | TypeAttributes.SequentialLayout | TypeAttributes.Sealed,
                typeof(ValueType),
                ConvType.Struct);

            // Handle [TypeLibType(...)] if evaluate to non-0
            using (TypeAttr refTypeAttr = RefTypeInfo.GetTypeAttr())
            {
                if (refTypeAttr.wTypeFlags != 0)
                    m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForTypeLibType((TypeLibTypeFlags)refTypeAttr.wTypeFlags));
            }

            m_info.AddToSymbolTable(RefTypeInfo, ConvType.Struct, this);
            m_info.RegisterType(m_typeBuilder, this);
        }

        private void CreateField(TypeInfo type, TypeAttr attr, VarDesc var, ref bool isConversionLoss)
        {
            TypeDesc fieldTypeDesc = var.elemdescVar.tdesc;
            TypeConverter typeConverter = new TypeConverter(m_info, type, fieldTypeDesc, ConversionType.Field);
            Type fieldType = typeConverter.ConvertedType;
            string fieldName = type.GetDocumentation(var.memid);
            FieldBuilder field = m_typeBuilder.DefineField(fieldName, fieldType, FieldAttributes.Public);
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

            //
            // Emit TypeLibVarAttribute if necessary
            //
            if (var.wVarFlags != 0)
            {
                field.SetCustomAttribute(CustomAttributeHelper.GetBuilderForTypeLibVar((TypeLibVarFlags)var.wVarFlags));
            }

        }

        #region IConvBase Members

        public override void OnCreate()
        {
            if (m_type != null)
                return;

            TypeInfo typeInfo = RefNonAliasedTypeInfo;
            using (TypeAttr typeAttr = typeInfo.GetTypeAttr())
            {
                //
                // Create fields
                //
                bool isConversionLoss = false;
                int cVars = typeAttr.cVars;
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
                m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForStructLayout(LayoutKind.Sequential, typeAttr.cbAlignment));

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
            get { return ConvType.Struct; }
        }

        #endregion
    }

    /// <summary>
    /// Represents a external managed struct that is already created
    /// </summary>
    class ConvStructExternal : IConvStruct
    {
        public ConvStructExternal(ConverterInfo info, TypeInfo typeInfo, Type managedType)
        {
            m_typeInfo = typeInfo;
            m_managedType = managedType;

            info.AddToSymbolTable(typeInfo, ConvType.Struct, this);
            info.RegisterType(managedType, this);
        }

        #region IConvStruct Members

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
            get { return ConvType.Struct; }
        }

        public string ManagedName
        {
            get
            {
                return RealManagedType.FullName;
            }
        }

        public ConvScope ConvScope
        {
            get { return ConvScope.External; }
        }

        #endregion

        private TypeInfo m_typeInfo;        // Corresponding type info
        private Type m_managedType;         // Corresponding managed type. Already created in a different DLL
    }
}
