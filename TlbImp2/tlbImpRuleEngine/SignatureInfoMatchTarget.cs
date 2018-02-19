using System;
using System.Collections.Generic;
using System.Text;
using TypeLibTypes.Interop;
using CoreRuleEngine;

namespace TlbImpRuleEngine
{
    public class SignatureInfoMatchTarget : IMatchTarget, IGetTypeLibElementCommonInfo, IGetNativeParentName
    {
        private const string TypeString = "Signature";

        private TypeInfo m_interfaceTypeInfo;

        private int m_functionIndex;

        private FuncDesc m_funcDesc;

        private string m_name;

        private string m_nativeParentFunctionName;

        private ElemDesc m_elemDesc;

        private int m_parameterIndex;

        private string m_nativeSignature;

        public SignatureInfoMatchTarget(TypeInfo interfaceTypeInfo, int functionIndex,
            ElemDesc elemDesc, int parameterIndex)
        {
            m_interfaceTypeInfo = interfaceTypeInfo;
            m_functionIndex = functionIndex;
            m_funcDesc = interfaceTypeInfo.GetFuncDesc(m_functionIndex);
            m_elemDesc = elemDesc;
            m_parameterIndex = parameterIndex;

            if (m_parameterIndex == 0)
            {
                m_name = "return";
            }
            else
            {
                // the name of the parameter.
                string[] signatureNames = m_interfaceTypeInfo.GetNames(m_funcDesc.memid,
                    m_funcDesc.cParams + 1);
                m_name = signatureNames[m_parameterIndex];
                if (m_name == null || m_name.Trim().Equals(""))
                    m_name = "_unnamed_arg_" + m_parameterIndex;
            }
            // NativeParentFunctionName
            m_nativeParentFunctionName =
                m_interfaceTypeInfo.GetDocumentation(m_funcDesc.memid);

            // NativeSignature
            m_nativeSignature = (new TlbType2String(interfaceTypeInfo, m_elemDesc.tdesc)).GetTypeString();
        }

        public string NativeSignature
        {
            get
            {
                return m_nativeSignature;
            }
        }

        /// <summary>
        /// Start with '1'
        /// void Function(ParameterIndex1, ParameterIndex2, ...)
        /// </summary>
        public int NativeParameterIndex
        {
            get
            {
                return m_parameterIndex;
            }
        }

        public TypeInfo InterfaceTypeInfo
        {
            get
            {
                return m_interfaceTypeInfo;
            }
        }

        public int FunctionIndex
        {
            get
            {
                return m_functionIndex;
            }
        }

        public FuncDesc FuncDesc
        {
            get
            {
                return m_funcDesc;
            }
        }

        public ElemDesc ElemDesc
        {
            get
            {
                return m_elemDesc;
            }
        }

        #region IMatchTarget Members

        public ICategory GetCategory()
        {
            return SignatureCategory.GetInstance();
        }

        #endregion

        #region ITypeLibElementCommonInfo Members

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public string Type
        {
            get { return TypeString; }
        }

        #endregion

        #region IGetNativeParentName Members

        public string GetNativeParentName()
        {
            return m_nativeParentFunctionName;
        }

        #endregion

    }
}
