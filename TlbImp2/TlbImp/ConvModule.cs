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
    /// Interface for conversion from ITypeInfo of a module to managed class
    /// </summary>
    interface IConvModule : IConvBase
    {
    }

    /// <summary>
    /// Conversion a local ITypeInfo to module    
    /// </summary>
    class ConvModuleLocal : ConvLocalBase, IConvModule
    {
        public ConvModuleLocal(ConverterInfo info, TypeInfo type)
        {
            DefineType(info, type, true);
        }

        protected override void OnDefineType()
        {
            TypeInfo typeInfo = RefNonAliasedTypeInfo;
            using (TypeAttr typeAttr = typeInfo.GetTypeAttr())
            {
                string name = m_info.GetUniqueManagedName(RefTypeInfo, ConvType.Module);

                // Create the managed type for the module
                // It should be abstract & sealed, which is the same as a static class in C#
                // Also, reflection will create a default constructor for you if the class has no constructor,
                // except if the class is interface, valuetype, enum, or a static class, so this works pretty well
                // except that this is slightly different than tlbimpv1, as tlbimpv1 the class is not sealed
                m_typeBuilder = m_info.ModuleBuilder.DefineType(name,
                    TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed,
                    typeof(Object));

                // Handle [Guid(...)] custom attribute
                ConvCommon.DefineGuid(RefTypeInfo, RefNonAliasedTypeInfo, m_typeBuilder);

                // Handle [TypeLibType(...)] if evaluate to non-0
                using (TypeAttr refTypeAttr = RefTypeInfo.GetTypeAttr())
                {
                    if (refTypeAttr.wTypeFlags != 0)
                        m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForTypeLibType((TypeLibTypeFlags)refTypeAttr.wTypeFlags));
                }
            }

            // Add to symbol table automatically
            m_info.AddToSymbolTable(RefTypeInfo, ConvType.Module, this);

            // Register type
            m_info.RegisterType(m_typeBuilder, this);
        }

        #region IConvBase Members

        /// <summary>
        /// Create the type for coclass
        /// </summary>
        public override void OnCreate()
        {
            //
            // Avoid duplicate creation
            //
            if (m_type != null)
                return;

            // Create constant fields for the module
            ConvCommon.CreateConstantFields(m_info, RefNonAliasedTypeInfo, m_typeBuilder, ConvType.Module);

            m_type = m_typeBuilder.CreateType();
        }

        public override ConvType ConvType
        {
            get
            {
                return ConvType.Module;
            }
        }

        #endregion
    }
}
