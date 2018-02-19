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

namespace TlbImpRuleEngine
{
    /// <summary>
    /// Convert unmanaged type (VT_XXX) to managed type
    /// The conversion is done at the time when you creates this object. After that this object is available
    /// for you to write custom attributes to the parameter and get the converted type
    /// </summary>
    public class TlbType2String
    {
        private static readonly List<string> s_tlbSimpleTypes = new List<string>
        {
            "INT", "UINT", "BYTE", "USHORT", "UINT", "ULONG", "SBYTE", "SHORT", "LONG",
            "FLOAT", "DOUBLE", "SCODE", "HRESULT", "void",
            "BSTR", "IDispatch *", "IUnknown *", "LPSTR", "LPWSTR", "VARIANT",
            "CURRENCY", "DATE", "DECIMAL", "VARIANT_BOOL",
        };

        private static List<string> s_tlbUserDefinedTypes = new List<string>();

        #region Public methods

        public TlbType2String(TypeInfo interfaceType, TypeDesc desc)
        {
            m_interfaceType = interfaceType;
            m_typeDesc = desc;
            m_typeStringBuilder = new StringBuilder();

            // Do the conversion
            _Convert();
        }

        public static List<string> TlbUserDefinedTypes
        {
            get
            {
                return s_tlbUserDefinedTypes;
            }
        }

        public static void AddTlbUserDefinedType(string userDefinedType)
        {
            if (!s_tlbUserDefinedTypes.Contains(userDefinedType))
                s_tlbUserDefinedTypes.Add(userDefinedType);
        }

        public static List<string> TlbSimpleTypes
        {
            get
            {
                return s_tlbSimpleTypes;
            }
        }

        public string GetTypeString()
        {
            return m_typeStringBuilder.ToString();
        }

        #endregion

        #region Private functions

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
            switch (vt)
            {
                case VarEnum.VT_INT:
                    m_typeStringBuilder.Append("INT");
                    break;

                case VarEnum.VT_UINT:
                    m_typeStringBuilder.Append("UINT");
                    break;

                case VarEnum.VT_UI1:
                    m_typeStringBuilder.Append("BYTE");
                    break;

                case VarEnum.VT_UI2:
                    m_typeStringBuilder.Append("USHORT");
                    break;

                case VarEnum.VT_UI4:
                    m_typeStringBuilder.Append("UINT");
                    break;

                case VarEnum.VT_UI8:
                    m_typeStringBuilder.Append("ULONG");
                    break;

                case VarEnum.VT_I1:
                    m_typeStringBuilder.Append("SBYTE");
                    break;

                case VarEnum.VT_I2:
                    m_typeStringBuilder.Append("SHORT");
                    break;

                case VarEnum.VT_I4:
                    m_typeStringBuilder.Append("INT");
                    break;

                case VarEnum.VT_I8:
                    m_typeStringBuilder.Append("LONG");
                    break;

                case VarEnum.VT_R4:
                    m_typeStringBuilder.Append("FLOAT");
                    break;

                case VarEnum.VT_R8:
                    m_typeStringBuilder.Append("DOUBLE");
                    break;

                case VarEnum.VT_ERROR :
                    m_typeStringBuilder.Append("SCODE");
                    break;

                case VarEnum.VT_HRESULT:
                    m_typeStringBuilder.Append("HRESULT");
                    break;

                case VarEnum.VT_VOID:
                    m_typeStringBuilder.Append("void");
                    break;

                case VarEnum.VT_BSTR:
                    m_typeStringBuilder.Append("BSTR");
                    break;

                case VarEnum.VT_DISPATCH:
                    m_typeStringBuilder.Append("IDispatch *");
                    // VT_DISPATCH => IDispatch *
                    break;

                case VarEnum.VT_UNKNOWN:
                    m_typeStringBuilder.Append("IUnknown *");
                    // VT_UNKNOWN => IUnknown *
                    break;

                case VarEnum.VT_LPSTR:
                    m_typeStringBuilder.Append("LPSTR");
                    break;

                case VarEnum.VT_LPWSTR:
                    m_typeStringBuilder.Append("LPWSTR");
                    break;

                case VarEnum.VT_PTR:
                    Debug.Assert(false, "Should not get here");
                    break;

                case VarEnum.VT_VARIANT:
                    m_typeStringBuilder.Append("VARIANT");
                    break;

                case VarEnum.VT_CY:
                    m_typeStringBuilder.Append("CURRENCY");
                    break;

                case VarEnum.VT_DATE:
                    m_typeStringBuilder.Append("DATE");
                    break;

                case VarEnum.VT_DECIMAL:
                    m_typeStringBuilder.Append("DECIMAL");
                    break;

                case VarEnum.VT_BOOL:
                    m_typeStringBuilder.Append("VARIANT_BOOL");
                    break;

                case VarEnum.VT_CARRAY:
                    {
                        TypeDesc elemTypeDesc = m_typeDesc.lptdesc;
                        TlbType2String elemTypeConverter = new TlbType2String(m_interfaceType, elemTypeDesc);
                        string elementTypeString = elemTypeConverter.GetTypeString();

                        uint elements = 1;
                        SAFEARRAYBOUND[] bounds = m_typeDesc.lpadesc.Bounds;
                        foreach (SAFEARRAYBOUND bound in bounds)
                        {
                            elements *= bound.cElements;
                        }

                        // SizeConst can only hold Int32.MaxValue
                        m_typeStringBuilder.Append(elementTypeString);
                        m_typeStringBuilder.Append(" [");
                        m_typeStringBuilder.Append(elements);
                        m_typeStringBuilder.Append(']');
                    }
                    break;

                case VarEnum.VT_SAFEARRAY:
                    // TODO(yifeng)
                    m_typeStringBuilder.Append("SAFEARRAY");
                    break;

                case VarEnum.VT_RECORD:
                case VarEnum.VT_USERDEFINED:
                    // Handle structs, interfaces, enums, and unions
                    TypeInfo realType;
                    try
                    {
                        realType = m_typeDesc.GetUserDefinedTypeInfo(m_interfaceType);
                        string typeString = "[" + realType.GetContainingTypeLib().GetDocumentation() +
                            "]" + realType.GetDocumentation();
                        m_typeStringBuilder.Append(typeString);
                        //NativeType2String.AddNativeUserDefinedType(typeString);
                    }
                    catch (Exception)
                    {
                        m_typeStringBuilder.Append("USERDEFINED");
                    }

                    break;

                default:
                    // TODO(yifeng)
                    m_typeStringBuilder.Append("NONE");
                    break;
            }

            for (int i = 0; i < m_nativeIndirections; i++)
                m_typeStringBuilder.Append(" *");
        }

        #endregion 

        #region Private data members

        private int m_nativeIndirections;                   // level of indirections
        private TypeDesc m_typeDesc;                        // Type description
        private TypeInfo m_interfaceType;
        private StringBuilder m_typeStringBuilder;          // Result string

        #endregion
    }
}