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
    /// Interface for doing creation of class interface
    /// </summary>
    interface IConvClassInterface : IConvBase
    {
    }

    /// <summary>
    /// Create class interface in local type lib
    /// There is no ConvClassInterfaceExternal
    /// </summary>
    class ConvClassInterfaceLocal : ConvLocalBase, IConvClassInterface
    {
        public ConvClassInterfaceLocal(
            ConverterInfo info,
            TypeInfo coclassTypeInfo,
            TypeInfo defaultInterfaceTypeInfo,
            TypeInfo defaultSourceInterfaceTypeInfo,
            bool isExclusive)
        {
            m_info = info;
            m_coclassTypeInfo = coclassTypeInfo;
            m_defaultInterfaceTypeInfo = defaultInterfaceTypeInfo;
            m_defaultSourceInterfaceTypeInfo = defaultSourceInterfaceTypeInfo;
            m_isExclusive = isExclusive;

            DefineType(info, m_coclassTypeInfo, false);
        }

        protected override void OnDefineType()
        {
            string classInterfaceName = m_info.GetUniqueManagedName(m_coclassTypeInfo, ConvType.ClassInterface);

            Type defaultInterfaceType = null;
            Type defaultSourceInterfaceType = null;

            m_convInterface = null;
            m_convSourceInterface = null;

            //
            // Convert default interface
            //
            if (m_defaultInterfaceTypeInfo != null)
            {
                m_convInterface = (IConvInterface)m_info.GetTypeRef(ConvType.Interface, m_defaultInterfaceTypeInfo);
                // Don't create the interface because we haven't associated the default interface with the class interface yet
                // We don't want to create anything in the "Define" stage
                //m_convInterface.Create();
                defaultInterfaceType = m_convInterface.ManagedType;
            }

            //
            // Convert default source interface
            //
            if (m_defaultSourceInterfaceTypeInfo != null)
            {
                m_convSourceInterface = (IConvInterface)m_info.GetTypeRef(ConvType.Interface, m_defaultSourceInterfaceTypeInfo);
                // Don't create the interface because we haven't associated the default interface with the class interface yet
                // We don't want to create anything in the "Define" stage
                // m_convSourceInterface.Create();
                Type sourceInterfaceType = m_convSourceInterface.RealManagedType;
                IConvEventInterface convEventInterface = m_convSourceInterface.DefineEventInterface();
                // Don't create the interface because we haven't associated the default interface with the class interface yet
                // We don't want to create anything in the "Define" stage
                // convEventInterface.Create();
                defaultSourceInterfaceType = m_convSourceInterface.EventInterface.ManagedType;
            }

            //
            // Prepare list of implemented interfaces
            //
            List<Type> implTypes = new List<Type>();
            if (defaultInterfaceType != null)
                implTypes.Add(defaultInterfaceType);
            if (defaultSourceInterfaceType != null)
                implTypes.Add(defaultSourceInterfaceType);

            // Create the class interface
            m_typeBuilder = m_info.ModuleBuilder.DefineType(
                    classInterfaceName,
                    TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract | TypeAttributes.Import,
                    null,
                    implTypes.ToArray());

            // Link to it so that ManagedType will return the class interface while GetWrappedInterfaceType will return the 
            // real interface
            // This must be done before creating the coclass because coclass needs this information
            // Only do so when the default interface is exclusively belongs to one coclass
            if (m_convInterface != null && m_isExclusive)
                m_convInterface.AssociateWithExclusiveClassInterface(this as IConvClassInterface);

            // Emit GuidAttribute, which is the same as the default interface, if it exists
            if (defaultInterfaceType != null)
            {
                ConvCommon.DefineGuid(m_convInterface.RefTypeInfo, m_convInterface.RefNonAliasedTypeInfo, m_typeBuilder);
            }

            // Make sure we know about the class interface before we go to define the coclass in the next statement
            m_info.RegisterType(m_typeBuilder, this);
            m_info.AddToSymbolTable(m_coclassTypeInfo, ConvType.ClassInterface, this);

            // Handle [CoClass(typeof(...))]
            Type typeRefCoClass = m_info.GetTypeRef(ConvType.CoClass, m_coclassTypeInfo).ManagedType;
            m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForCoClass(typeRefCoClass));

        }
                   
        /// <summary>
        /// Create the class interface
        /// If the default interface is exclusively owned/used by this co-class, isExclusive should be set to true,
        /// meaning that we should replace every occurrence of the default interface with the class interface 
        /// </param>
        public override void OnCreate()
        {
            if (m_type != null)
                return;

            if (m_convInterface != null)
                m_convInterface.Create();

            if (m_convSourceInterface != null)
            {
                m_convSourceInterface.Create();
                m_convSourceInterface.EventInterface.Create();
            }

            // Create the type and add it to the list of created types
            m_type = m_typeBuilder.CreateType();
        }

        #region IConvBase Members

        public override ConvType ConvType
        {
            get { return ConvType.ClassInterface; }
        }

        #endregion

        #region Private members

        private TypeInfo m_coclassTypeInfo;                         // TypeInfo of the coclass

        private TypeInfo m_defaultInterfaceTypeInfo;                // TypeInfo of the default interface
        private TypeInfo m_defaultSourceInterfaceTypeInfo;          // TypeInfo of the default source interface
        private IConvInterface m_convInterface;                     // IConvInterface for the default interface
        private IConvInterface m_convSourceInterface;               // IConvInterface for the default source interface

        private bool m_isExclusive;                                 // see IConvClassInterface.IsExclusive for details
  
        #endregion
    }

    /// <summary>
    /// We need to replace external CoClass with external class interface in the signature
    /// therefore we need to support ConvClassInterfaceExternal. However, we should not create 
    /// it in the resolve process, but create it when we create ConvCoClassExternal
    /// </summary>
    class ConvClassInterfaceExternal : IConvClassInterface
    {
        public ConvClassInterfaceExternal(ConverterInfo info, TypeInfo typeInfo, Type managedType, ConverterAssemblyInfo converterAssemblyInfo)
        {
            m_typeInfo = typeInfo;
            m_managedType = managedType;

            info.RegisterType(managedType, this);

            //
            // Associate the default interface with the class interface
            //
            TypeInfo defaultInterfaceTypeInfo;
            if (converterAssemblyInfo.ClassInterfaceMap.GetExclusiveDefaultInterfaceForCoclass(typeInfo, out defaultInterfaceTypeInfo))
            {
                IConvInterface convInterface = info.GetTypeRef(ConvType.Interface, defaultInterfaceTypeInfo) as IConvInterface;
                convInterface.AssociateWithExclusiveClassInterface(this);
            }
        }

        #region IConvClassInterface Members

        public void Create()
        {
            // Do nothing
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