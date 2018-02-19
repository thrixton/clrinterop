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
    /// Context information used to create interface methods on managed type. Could be a interface, or a coclass.
    /// </summary>
    class InterfaceInfo
    {
        public InterfaceInfo(ConverterInfo info, TypeBuilder typeBuilder, bool supportsDispatch, TypeInfo type, TypeAttr attr, bool bCoClass)
        {
            Init(info, typeBuilder, supportsDispatch, type, attr, bCoClass, false, null);
        }

        public InterfaceInfo(ConverterInfo info, TypeBuilder typeBuilder, bool supportsDispatch, TypeInfo type, TypeAttr attr, bool bCoClass, bool bSource)
        {
            Init(info, typeBuilder, supportsDispatch, type, attr, bCoClass, bSource, null);
        }

        public InterfaceInfo(ConverterInfo info, TypeBuilder typeBuilder, bool supportsDispatch, TypeInfo type, TypeAttr attr, bool bCoClass, TypeInfo implementingInterface)
        {
            Init(info, typeBuilder, supportsDispatch, type, attr, bCoClass, false, implementingInterface);
        }

        public InterfaceInfo(ConverterInfo info, TypeBuilder typeBuilder, bool supportsDispatch, TypeInfo type, TypeAttr attr, bool bCoClass, bool bSource, TypeInfo implementingInterface)
        {
            Init(info, typeBuilder, supportsDispatch, type, attr, bCoClass, bSource, implementingInterface);
        }

        private void Init(ConverterInfo info, TypeBuilder typeBuilder, bool emitDispId, TypeInfo type, TypeAttr attr, bool bCoClass, bool bSource, TypeInfo implementingInterface)
        {
            m_typeStack = new Stack<TypeInfo>();

            m_attrStack = new Stack<TypeAttr>();
            PushType(type, attr);

            m_info = info;
            m_typeBuilder = typeBuilder;
            m_emitDispId = emitDispId;
            m_bCoClass = bCoClass;
            m_propertyInfo = new PropertyInfo(this);
            m_bSource = bSource;
            IsConversionLoss = false;
            IsDefaultInterface = false;
            m_currentSlot = 0;
            m_currentImplementingInterface = implementingInterface;
        }

        public void PushType(TypeInfo type, TypeAttr attr)
        {
            m_typeStack.Push(type);
            m_attrStack.Push(attr);
        }
        public void PopType()
        {
            m_typeStack.Pop();
            m_attrStack.Pop();
        }

        /// <summary>
        /// Remove namespace of a name
        /// </summary>
        private string RemoveNamespace(string name)
        {
            int index = name.LastIndexOf('.');
            if (index < 0) return name;
            if (index >= name.Length - 1) return String.Empty;

            return name.Substring(index + 1);
        }

        /// <summary>
        /// Generate a unique member name according to prefix & suffix
        /// Will try prefix first if prefix is not null, otherwise try suffix (_2, _3, ...)
        /// </summary>
        /// <param name="strName">Original name</param>
        /// <param name="strPrefix">Optional prefix</param>
        /// <param name="paramTypes">Parameter types</param>
        /// <returns>The unique member name</returns>
        public string GenerateUniqueMemberName(string name, Type[] paramTypes, MemberTypes memberType)
        {
            // TypeBuilder.GetMethod/GetEvent/GetProperty doesn't work before the type is created. 
            // ConverterInfo maintains a global TypeBuilder -> (Name, Type[]) mapping
            // So ask ConverterInfo if we already have that
            String newName = name;
            int PostFix = 2;
            
            if (!m_info.HasDuplicateMemberName(m_typeBuilder, newName, paramTypes, memberType))
                return newName;

            // If we are creating a coclass, try prefix first
            if (IsCoClass)
            {
                // Use the unique interface name instead of the type info name 
                // (but TlbImpv1 actually use the type info name, which is incorrect)

                // Use the current implementing interface instead of the active interface we are implementing
                // When coclass A implements IA2 which derives from IA1. 
                // m_currentImplementingInterface will always be IA2, while the active interface (RefTypeInfo) will be IA2 then IA1
                string prefix;
                if (IsSource)
                    prefix = m_info.GetTypeRef(ConvType.EventInterface, m_currentImplementingInterface).ManagedName;
                else
                    prefix = m_info.GetTypeRef(ConvType.Interface, m_currentImplementingInterface).ManagedName;
 
                // Remove the namespace of prefix and try the new name
                newName = RemoveNamespace(prefix) + "_" + name;
                if (!m_info.HasDuplicateMemberName(m_typeBuilder, newName, paramTypes, memberType))
                    return newName;

                // Now use the prefixed name as starting point
                name = newName;
            }

            // OK. Prefix doesn't work. Let's try suffix
            // Find the first unique name for the type.
            do
            {
                newName = name + "_" + PostFix;
                PostFix++;
            }
            while (m_info.HasDuplicateMemberName(m_typeBuilder, newName, paramTypes, memberType));

            return newName;
        }

        /// <summary>
        /// Whether this interface is a default interface of a coclass
        /// </summary>
        public bool IsDefaultInterface
        {
            get
            {
                return m_isDefaultInterface;
            }

            set
            {
                m_isDefaultInterface = value;
            }
        }

        /// <summary>
        /// This one deserves some explanation
        /// if coclass A implements IA2 : IA1
        /// when we go to IA2 and implement all the methods on A for IA1, we need to override the method in IA2, not in IA1
        /// because it is possible for A to both implement IA1 & IA2.
        /// So in this case, we are emitting methods in IA1, but the current implementing interface is actually IA2
        /// </summary>
        public TypeInfo CurrentImplementingInterface
        {
            get
            {
                return m_currentImplementingInterface;
            }
        }

        public ConverterInfo ConverterInfo { get { return m_info; } }
        public PropertyInfo PropertyInfo { get { return m_propertyInfo; } }
        public TypeBuilder TypeBuilder { get { return m_typeBuilder; } }
        public bool EmitDispId { get { return m_emitDispId; } }
        public TypeInfo RefTypeInfo { get { return m_typeStack.Peek(); } }
        public TypeAttr RefTypeAttr { get { return m_attrStack.Peek(); } }
        public bool IsCoClass { get { return m_bCoClass; } }
        public bool IsSource { get { return m_bSource; } }
        public bool IsConversionLoss 
        {
            get
            {
                return m_isConversionLoss;
            }
            set
            {
                m_isConversionLoss = value;
            }
        }

        /// <summary>
        /// Whether we allow DISPID_NEWENUM members in the interface anymore
        /// Will be changed to false if we have already created a new enum member
        /// </summary>
        public bool AllowNewEnum
        {
            get
            {
                return m_allowNewEnum;
            }
            set
            {
                m_allowNewEnum = value;
            }
        }

        public int CurrentSlot
        {
            get
            {
                return m_currentSlot;
            }
            set
            {
                m_currentSlot = value;
            }
        }

        private ConverterInfo m_info;
        private PropertyInfo m_propertyInfo;
        private TypeBuilder m_typeBuilder;
        private bool m_emitDispId;
        private Stack<TypeInfo> m_typeStack;
        private Stack<TypeAttr> m_attrStack;
        private bool m_bCoClass;
        private bool m_bSource;
        private bool m_isDefaultInterface;
        private bool m_isConversionLoss;
        private bool m_allowNewEnum;
        private int m_currentSlot;

        /// <summary>
        /// This one deserves some explanation
        /// if coclass A implements IA2 : IA1
        /// when we go to IA2 and implement all the methods on A for IA1, we need to override the method in IA2, not in IA1
        /// because it is possible for A to both implement IA1 & IA2
        /// </summary>
        private TypeInfo m_currentImplementingInterface;
    }
}
