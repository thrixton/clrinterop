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
    /// Interface for conversion from ITypeInfo of a coclass to managed class
    /// </summary>
    interface IConvCoClass : IConvBase
    {
        IConvInterface DefaultInterface
        {
            get;
        }
    }

    /// <summary>
    /// Conversion a local ITypeInfo to class    
    /// </summary>
    class ConvCoClassLocal : ConvLocalBase, IConvCoClass
    {
        public ConvCoClassLocal(ConverterInfo info, TypeInfo type)
        {
            DefineType(info, type, true);
        }

        protected override void OnDefineType()
        {
            TypeInfo typeInfo = RefNonAliasedTypeInfo;
            using (TypeAttr attr = typeInfo.GetTypeAttr())
            {
                string name = m_info.GetUniqueManagedName(RefTypeInfo, ConvType.CoClass);

                //
                // Collect information for a list of interfaces & event interface types
                //
                List<Type> intfList = new List<Type>();         // interface list
                List<Type> eventIntfList = new List<Type>();    // event interface list
                TypeInfo defaultInterfaceTypeInfo = null;

                int nCount = attr.cImplTypes;

                string sourceInterfaceNames = String.Empty;
                bool hasDefaultInterface = false;
                bool implementsIEnumerable = ConvCommon.ExplicitlyImplementsIEnumerable(typeInfo, attr);

                for (int n = 0; n < nCount; ++n)
                {
                    IMPLTYPEFLAGS flags = typeInfo.GetImplTypeFlags(n);
                    bool isDefault = (flags & IMPLTYPEFLAGS.IMPLTYPEFLAG_FDEFAULT) != 0;
                    bool isSource = (flags & IMPLTYPEFLAGS.IMPLTYPEFLAG_FSOURCE) != 0;

                    TypeInfo typeImpl = typeInfo.GetRefType(n);

                    using (TypeAttr attrImpl = typeImpl.GetTypeAttr())
                    {
                        // Skip IUnknown & IDispatch
                        if (attrImpl.Guid == WellKnownGuids.IID_IDispatch || attrImpl.Guid == WellKnownGuids.IID_IUnknown)
                            continue;

                        // Skip non-dispatch interfaces that doesn't derive from IUnknown
                        if (!attrImpl.IsDispatch && !ConvCommon.IsDerivedFromIUnknown(typeImpl))
                            continue;

                        IConvInterface convInterface = (IConvInterface)m_info.GetTypeRef(ConvType.Interface, typeImpl);

                        // For source interfaces, try create the event interface
                        // Could be already created if it is the default source interface
                        if (isSource)
                        {
                            convInterface.DefineEventInterface();
                        }

                        // Use the RealManagedType (avoid getting the class interface)
                        Type typeRef = convInterface.RealManagedType;

                        // Append the source interface name to the list for the ComSourceInterfacesAttribute
                        if (isSource)
                        {
                            string interfaceName;
                            if (convInterface.ConvScope == ConvScope.External)
                                interfaceName = typeRef.AssemblyQualifiedName + "\0";
                            else
                                interfaceName = typeRef.FullName + "\0";

                            // Insert default source interface to the beginning
                            if (isDefault)
                                sourceInterfaceNames = interfaceName + sourceInterfaceNames;
                            else
                                sourceInterfaceNames = sourceInterfaceNames + interfaceName;
                        }

                        if (isDefault)
                        {
                            // Add the real interface first
                            if (isSource)
                            {
                                // For source interface, use the event interface instead
                                // Insert to the beginning
                                eventIntfList.Insert(0, convInterface.EventInterface.ManagedType);
                            }
                            else
                            {
                                m_defaultInterface = convInterface;

                                // Insert to the beginning
                                intfList.Insert(0, typeRef);
                                hasDefaultInterface = true;
                                defaultInterfaceTypeInfo = typeImpl;
                            }
                        }
                        else
                        {
                            if (isSource)
                            {
                                // For source interface, add the event interface instead
                                eventIntfList.Add(convInterface.EventInterface.ManagedType);
                            }
                            else
                            {
                                if (m_defaultInterface == null)
                                {
                                    m_defaultInterface = convInterface;
                                    defaultInterfaceTypeInfo = typeImpl;
                                }

                                intfList.Add(typeRef);
                            }
                        }
                    }
                }

                //
                // Get class interface
                //
                m_classInterface = m_info.GetTypeRef(ConvType.ClassInterface, RefTypeInfo) as IConvClassInterface;
                if (m_classInterface == null) throw new TlbImpInvalidTypeConversionException(RefTypeInfo);
                
                //
                // Build implemented type list in a specific order
                //
                List<Type> implTypeList = new List<Type>();
                if (hasDefaultInterface)
                {
                    implTypeList.Add(intfList[0]);
                    intfList.RemoveAt(0);

                    implTypeList.Add(m_classInterface.ManagedType);
                }
                else
                {
                    implTypeList.Add(m_classInterface.ManagedType);
                    if (intfList.Count > 0)
                    {
                        implTypeList.Add(intfList[0]);
                        intfList.RemoveAt(0);
                    }
                }

                if (eventIntfList.Count > 0)
                {
                    implTypeList.Add(eventIntfList[0]);
                    eventIntfList.RemoveAt(0);
                }

                implTypeList.AddRange(intfList);
                implTypeList.AddRange(eventIntfList);

                // Check to see if the default interface has a member with a DISPID of DISPID_NEWENUM.
                if (defaultInterfaceTypeInfo != null)
                    if (!implementsIEnumerable && ConvCommon.HasNewEnumMember(m_info, defaultInterfaceTypeInfo))
                        implTypeList.Add(typeof(System.Collections.IEnumerable));

                // Check to see if the IEnumerable Custom Value exists on the CoClass if doesn't inherit from IEnumerable yet
                if (!implTypeList.Contains(typeof(System.Collections.IEnumerable)))
                {
                    if (ConvCommon.HasForceIEnumerableCustomAttribute(typeInfo))
                        implTypeList.Add(typeof(System.Collections.IEnumerable));
                }

                // Define the type
                m_typeBuilder = m_info.ModuleBuilder.DefineType(name,
                    TypeAttributes.Public | TypeAttributes.Import,
                    typeof(Object),
                    implTypeList.ToArray());

                // Handle [Guid(...)] custom attribute
                ConvCommon.DefineGuid(RefTypeInfo, RefNonAliasedTypeInfo, m_typeBuilder);

                // Handle [ClassInterface(ClassInterfaceType.None)]
                m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForClassInterface(ClassInterfaceType.None));

                // Handle [TypeLibType(...)] if evaluate to non-0
                using (TypeAttr refTypeAttr = RefTypeInfo.GetTypeAttr())
                {
                    if (refTypeAttr.wTypeFlags != 0)
                        m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForTypeLibType((TypeLibTypeFlags)refTypeAttr.wTypeFlags));
                }

                // Handle [ComSourceInterfacesAttribute]
                if (sourceInterfaceNames != String.Empty)
                {
                    sourceInterfaceNames += "\0";
                    m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForComSourceInterfaces(sourceInterfaceNames));
                }

                // Add to symbol table automatically
                m_info.AddToSymbolTable(RefTypeInfo, ConvType.CoClass, this);

                // Register type
                m_info.RegisterType(m_typeBuilder, this);
            }
        }
        
        /// <summary>
        /// Implement methods in parent interfaces
        /// </summary>
        private void HandleParentInterface(TypeInfo type, bool bSource, ref bool isConversionLoss, bool isDefault)
        {
            using (TypeAttr attr = type.GetTypeAttr())
            {
                InterfaceInfo interfaceInfo = new InterfaceInfo(m_info, m_typeBuilder, ConvCommon.InterfaceSupportsDispatch(type, attr), type, attr, true, bSource, type);
                interfaceInfo.IsDefaultInterface = isDefault;

                if (bSource)
                    // When adding override methods to the interface, we need to use the event interface for source interfaces
                    ConvCommon.CreateEventInterfaceCommon(interfaceInfo);
                else
                    ConvCommon.CreateInterfaceCommon(interfaceInfo);

                isConversionLoss |= interfaceInfo.IsConversionLoss;
            }
        }        

        /// <summary>
        /// Generate the special internal call constructor for RCW's 
        /// </summary>
        private void CreateConstructor()
        {
            MethodAttributes methodAttributes;

            TypeInfo typeInfo = RefNonAliasedTypeInfo;
            using (TypeAttr typeAttr = typeInfo.GetTypeAttr())
            {
                if (typeAttr.IsCanCreate)
                    methodAttributes = MethodAttributes.Public;
                else
                    methodAttributes = MethodAttributes.Assembly;                
            }

            ConstructorBuilder method = m_typeBuilder.DefineDefaultConstructor(methodAttributes);
            ConstructorInfo ctorMethodImpl = typeof(MethodImplAttribute).GetConstructor(new Type[] { typeof(MethodImplOptions) });
            method.SetImplementationFlags(MethodImplAttributes.InternalCall | MethodImplAttributes.Runtime);
        }

        #region IConvCoClass members

        public IConvClassInterface ClassInterface
        {
            get
            {
                return m_classInterface;
            }
        }

        public IConvInterface DefaultInterface
        {
            get
            {
                return m_defaultInterface;
            }
        }

        #endregion

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

            //
            // Create constructor
            // This is created before creating other methods because if anything fails, the constructor would still be valid
            // Otherwise reflection API will try to create one for us which will have incorrect setting (such as no internalcall flag)
            CreateConstructor();

            bool isConversionLoss = false;

            TypeInfo typeInfo = RefNonAliasedTypeInfo;
            if ((m_info.Settings.m_flags & TypeLibImporterFlags.PreventClassMembers) == 0)
            {
                using (TypeAttr attr = typeInfo.GetTypeAttr())
                {
                    Dictionary<Guid, bool> processedInterfaces = new Dictionary<Guid, bool>();

                    // Iterate through every interface and override the methods
                    // Process the default interface first
                    for (int m = 0; m < 2; ++m)
                    {
                        int nCount = attr.cImplTypes;
                        for (int n = 0; n < nCount; ++n)
                        {
                            IMPLTYPEFLAGS flags = typeInfo.GetImplTypeFlags(n);
                            bool bDefault = (flags & IMPLTYPEFLAGS.IMPLTYPEFLAG_FDEFAULT) != 0;
                            bool bSource = (flags & IMPLTYPEFLAGS.IMPLTYPEFLAG_FSOURCE) != 0;

                            // Use exclusive or to process just the default interface on the first pass
                            if (bDefault ^ m == 1)
                            {
                                TypeInfo typeImpl = typeInfo.GetRefType(n);
                                using (TypeAttr attrImpl = typeImpl.GetTypeAttr())
                                {
                                    if (attrImpl.Guid == WellKnownGuids.IID_IUnknown || attrImpl.Guid == WellKnownGuids.IID_IDispatch)
                                        continue;

                                    // Skip non-dispatch interfaces that doesn't derive from IUnknown
                                    if (!attrImpl.IsDispatch && !ConvCommon.IsDerivedFromIUnknown(typeImpl))
                                        continue;

                                    // Skip duplicate interfaces in type library
                                    // In .IDL you can actually write:
                                    // coclass A
                                    // {
                                    //     interface IA; 
                                    //     interface IA;
                                    //     ...
                                    // }
                                    //
                                    if (!processedInterfaces.ContainsKey(attrImpl.Guid))
                                    {
                                        HandleParentInterface(typeImpl, bSource, ref isConversionLoss, bDefault);
                                        processedInterfaces.Add(attrImpl.Guid, true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
                
            //
            // Emit ComConversionLoss attribute if necessary
            //
            if (isConversionLoss)
                m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForComConversionLoss());

            m_type = m_typeBuilder.CreateType();
        }

        public override ConvType ConvType
        {
            get
            {
                return ConvType.CoClass;
            }
        }

        #endregion

        #region Private members

        private IConvClassInterface m_classInterface;   // Class interface
        private IConvInterface m_defaultInterface;      // The default interface

        #endregion
    }


    //
    // In type library you can refer to the coclass (which is weird...) and will be converted to class interface
    // So we should support external ConvCoClass
    //
    class ConvCoClassExternal : IConvCoClass
    {
        public ConvCoClassExternal(ConverterInfo info, TypeInfo typeInfo, Type managedType, ConverterAssemblyInfo converterAssemblyInfo)
        {
            m_typeInfo = typeInfo;
            m_managedType = managedType;

            info.RegisterType(managedType, this);

            TypeInfo defaultTypeInfo = ConvCommon.GetDefaultInterface(ConvCommon.GetAlias(typeInfo));
            if (defaultTypeInfo != null)
                m_defaultInterface = info.GetTypeRef(ConvType.Interface, defaultTypeInfo) as IConvInterface; 
        }

        #region IConvCoClass Members

        public IConvInterface DefaultInterface
        {
            get
            {
                return m_defaultInterface;
            }
        }

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
            get { return ConvType.CoClass; }
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
        private IConvInterface m_defaultInterface;      // Default interface
    }
}
