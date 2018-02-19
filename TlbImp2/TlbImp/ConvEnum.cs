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

namespace tlbimp2
{
    /// <summary>
    /// Interface for enum conversion                                                                        o
    /// </summary>
    interface IConvEnum : IConvBase
    {
    }

    /// <summary>
    /// Conversion from a local ITypeInfo for enum to a managed enum
    /// </summary>
    class ConvEnumLocal : ConvLocalBase, IConvEnum
    {
        public ConvEnumLocal(ConverterInfo info, TypeInfo type)
        {
            DefineType(info, type, true);
        }

        protected override void OnDefineType()
        {
            TypeInfo typeInfo = RefNonAliasedTypeInfo;

            // Creates the enum type
            m_typeBuilder = ConvCommon.DefineTypeHelper(
                m_info, 
                RefTypeInfo,
                RefNonAliasedTypeInfo,
                TypeAttributes.Public | TypeAttributes.Sealed,
                typeof(Enum),
                ConvType.Enum
                );

            // The field must be created here so that TypeBuilder can calculate a hash...
            // Need to create a special field to hold the enum data
            FieldBuilder field = m_typeBuilder.DefineField(
                "value__",
                typeof(Int32),
                FieldAttributes.Public | FieldAttributes.SpecialName);

            // Handle [TypeLibType(...)] if evaluate to non-0
            using (TypeAttr refTypeAttr = RefTypeInfo.GetTypeAttr())
            {
                if (refTypeAttr.wTypeFlags != 0)
                    m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForTypeLibType((TypeLibTypeFlags)refTypeAttr.wTypeFlags));
            }

            m_info.AddToSymbolTable(RefTypeInfo, ConvType.Enum, this);
            m_info.RegisterType(m_typeBuilder, this);
        }

        #region IConvBase Members

        /// <summary>
        /// Creates the enum
        /// </summary>
        public override void OnCreate()
        {
            if (m_type != null)
                return;

            // Create constant fields for the enum
            ConvCommon.CreateConstantFields(m_info, RefNonAliasedTypeInfo, m_typeBuilder, ConvType.Enum);

            m_type = m_typeBuilder.CreateType();
        }

        public override ConvType ConvType 
        { 
            get { return ConvType.Enum; } 
        }

        #endregion
    }

    /// <summary>
    /// Conversion from a external ITypeInfo for enum to a managed enum
    /// </summary>
    class ConvEnumExternal : IConvEnum
    {
        public ConvEnumExternal(ConverterInfo info, TypeInfo typeInfo, Type managedType)
        {
            m_typeInfo = typeInfo;
            m_managedType = managedType;

            info.AddToSymbolTable(typeInfo, ConvType.Enum, this);
            info.RegisterType(managedType, this);
        }

        #region IConvEnum Members

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
            get { return ConvType.Enum; }
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

        private TypeInfo m_typeInfo;        // Corresponding type info
        private Type m_managedType;         // Corresponding managed type. Already created in a different DLL
    }
}
