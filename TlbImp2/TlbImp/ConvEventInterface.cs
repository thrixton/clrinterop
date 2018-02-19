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
using TypeLibTypes.Interop;

namespace tlbimp2
{
    /// <summary>
    /// Interface for event interface creation
    /// </summary>
    interface IConvEventInterface : IConvBase
    {
        /// <summary>
        /// The corresponding source interface
        /// </summary>
        IConvInterface SourceInterface
        {
            get;
        }

        /// <summary>
        /// Get the event delegate
        /// </summary>
        /// <param name="memberInfo">The MemberInfo of a source interface that is used to create this delegate</param>
        /// <returns>The delegate type</returns>
        Type GetEventDelegate(InterfaceMemberInfo memberInfo);
    }

    /// <summary>
    /// Converts a local ITypeInfo (source interface) to the event interface
    /// </summary>
    class ConvEventInterfaceLocal : ConvLocalBase, IConvEventInterface
    {
        public ConvEventInterfaceLocal(IConvInterface convInterface, ConverterInfo info)
        {
            m_convInterface = convInterface;

            DefineType(info, convInterface.RefTypeInfo, false);
        }

        protected override void OnDefineType()
        {
            // Create event interface type
            m_typeBuilder = m_info.ModuleBuilder.DefineType(
                m_info.GetUniqueManagedName(m_convInterface.RefTypeInfo, ConvType.EventInterface),
                TypeAttributes.Interface | TypeAttributes.Public |
                TypeAttributes.Abstract | TypeAttributes.AutoLayout
                );

            m_info.RegisterType(m_typeBuilder, this);
            m_info.AddToSymbolTable(m_convInterface.RefTypeInfo, ConvType.EventInterface, this);
        }

        /// <summary>
        /// Create the event interface
        /// </summary>
        public override void OnCreate()
        {
            if (m_type != null) return;

            string name = m_convInterface.ManagedName;

            m_convInterface.Create();

            using (TypeAttr attr = m_convInterface.RefTypeInfo.GetTypeAttr())
            {

                //
                // Emit attributes
                //

                //
                // Emit [ComEventInterfaceAttribute(...)]
                //
                ConstructorInfo ctorComEventInterface = typeof(ComEventInterfaceAttribute).GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(Type), typeof(Type) },
                    null);

                //
                // Build the blob manually before creating the event interface / provider types. 
                // We only need to give the name of the types, in order to simplify creation logic and avoid dependency
                //
                CustomAttributeBlobBuilder blobBuilder = new CustomAttributeBlobBuilder();

                string eventInterfaceFullyQualifiedName = name;
                if (m_convInterface.ConvScope == ConvScope.External)
                    eventInterfaceFullyQualifiedName = m_convInterface.ManagedType.AssemblyQualifiedName;

                blobBuilder.AddFixedArg(eventInterfaceFullyQualifiedName);                      // source interface

                // Handle event provider name generation collision scenario
                m_eventProviderName = m_info.GetUniqueManagedName(
                    m_info.GetRecommendedManagedName(m_convInterface.RefTypeInfo, ConvType.Interface, true) + "_EventProvider");

                blobBuilder.AddFixedArg(m_eventProviderName);   // corresponding event provider
                m_typeBuilder.SetCustomAttribute(ctorComEventInterface, blobBuilder.GetBlob());

                //
                // Emit ComVisibleAttribute(false)
                //
                m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForComVisible(false));

                //
                // Emit TypeLibTypeAttribute for TYPEFLAG_FHIDDEN
                //
                m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForTypeLibType(TypeLibTypeFlags.FHidden));

                bool isConversionLoss = false;

                //
                // Verify if the type has any properties
                //
                Type interfaceType = m_convInterface.RealManagedType;
                if (interfaceType.GetProperties().Length > 0)
                {
                    // Emit a warning and we'll skip the properties
                    m_info.ReportEvent(
                        WarningCode.Wrn_NoPropsInEvents,
                        Resource.FormatString("Wrn_NoPropsInEvents", RefTypeInfo.GetDocumentation()));

                    isConversionLoss = true;
                }

                //
                // Create event interface
                //

                InterfaceInfo eventInterfaceInfo = new InterfaceInfo(m_info, m_typeBuilder, false, m_convInterface.RefTypeInfo, attr, false, true);

                ConvCommon.CreateEventInterfaceCommon(eventInterfaceInfo);
                isConversionLoss |= eventInterfaceInfo.IsConversionLoss;

                //
                // Emit ComConversionLossAttribute if necessary
                //
                if (eventInterfaceInfo.IsConversionLoss)
                {
                    m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForComConversionLoss());
                }
            }
            
            m_type = m_typeBuilder.CreateType();
        }

        /// <summary>
        /// Returns the source interface
        /// </summary>
        public IConvInterface SourceInterface
        {
            get
            {
                return m_convInterface;
            }
        }

        /// <summary>
        /// Get the event delegate for specified method in the source interface. Create a new one if necessary
        /// </summary>
        /// <param name="index">Function index</param>
        /// <returns>The delegate type</returns>
        public Type GetEventDelegate(InterfaceMemberInfo memberInfo)
        {
            TypeInfo type = memberInfo.RefTypeInfo;
            using (TypeAttr attr = type.GetTypeAttr())
            {
                // Create m_delegateTypes on demand
                if (m_delegateTypes == null)
                {
                    m_delegateTypes = new Dictionary<InterfaceMemberInfo, Type>();
                }

                //
                // Check if we already have a delegate type for method n
                //
                if (!m_delegateTypes.ContainsKey(memberInfo))
                {
                    //
                    // If not, create a new delegate
                    //
                    FuncDesc func = type.GetFuncDesc(memberInfo.Index);

                    string eventName = type.GetDocumentation(func.memid);
                    string delegateName = m_info.GetRecommendedManagedName(m_convInterface.RefTypeInfo, ConvType.Interface, true) + "_" + type.GetDocumentation(func.memid) + "EventHandler";

                    // Deal with name collisions
                    delegateName = m_info.GetUniqueManagedName(delegateName);

                    TypeBuilder delegateTypeBuilder = m_info.ModuleBuilder.DefineType(
                        delegateName,
                        TypeAttributes.Public | TypeAttributes.Sealed,
                        typeof(MulticastDelegate)
                    );

                    // Create constructor for the delegate
                    ConstructorBuilder delegateCtorBuilder = delegateTypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new Type[] { typeof(Object), typeof(UIntPtr) });
                    delegateCtorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

                    // Create methods for the delegate
                    InterfaceInfo interfaceInfoForDelegate = new InterfaceInfo(m_info, delegateTypeBuilder, false, type, attr, false, true);
                    interfaceInfoForDelegate.AllowNewEnum = !m_convInterface.ImplementsIEnumerable;
                    ConvCommon.CreateMethodForDelegate(interfaceInfoForDelegate, func, memberInfo.Index);

                    // Emit ComVisibleAttribute(false)
                    delegateTypeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForComVisible(false));

                    // Emit TypeLibTypeAttribute(TypeLibTypeFlags.FHidden) to hide it from object browser in VB
                    delegateTypeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForTypeLibType(TypeLibTypeFlags.FHidden));

                    // Create the delegate
                    m_delegateTypes[memberInfo] = delegateTypeBuilder.CreateType();
                }
            }

            return m_delegateTypes[memberInfo];
        }

        #region IConvBase Members

        public override ConvType ConvType
        {
            get { return ConvType.EventInterface; }
        }

        #endregion

        public string EventProviderName
        {
            get
            {
                return m_eventProviderName;
            }
        }

        #region Private members

        private IConvInterface m_convInterface;
        private Dictionary<InterfaceMemberInfo, Type> m_delegateTypes;                     // Keeps a mapping of delegate types & function id. Created on-demand.
        private string m_eventProviderName;
        #endregion 
    }

}