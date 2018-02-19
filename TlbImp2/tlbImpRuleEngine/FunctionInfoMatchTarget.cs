using System;
using System.Collections.Generic;
using System.Text;
using TypeLibTypes.Interop;
using CoreRuleEngine;

namespace TlbImpRuleEngine
{
    public class FunctionInfoMatchTarget : IMatchTarget, IGetTypeLibElementCommonInfo, IGetNativeParentName
    {
        private const string TypeString = "Function";

        private TypeInfo m_interfaceTypeInfo;

        private int m_index;

        private FuncDesc m_funcDesc;

        private string m_name;

        private string m_nativeParentTypeName;

        public FunctionInfoMatchTarget(TypeInfo parentTypeInfo, int index)
        {
            m_interfaceTypeInfo = parentTypeInfo;
            m_index = index;
            m_funcDesc = parentTypeInfo.GetFuncDesc(m_index);
            m_nativeParentTypeName = m_interfaceTypeInfo.GetDocumentation();
        }

        public TypeInfo InterfaceTypeInfo
        {
            get
            {
                return m_interfaceTypeInfo;
            }
        }

        public int Index
        {
            get
            {
                return m_index;
            }
        }

        public FuncDesc FuncDesc
        {
            get
            {
                return m_funcDesc;
            }
        }

        #region IMatchTarget Members

        public ICategory GetCategory()
        {
            return FunctionCategory.GetInstance();
        }

        #endregion

        #region ITypeLibElementCommonInfo Members

        public string Name
        {
            get
            {
                if (m_name == null)
                {
                    m_name = m_interfaceTypeInfo.GetDocumentation(m_funcDesc.memid);
                }
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
            return m_nativeParentTypeName;
        }

        #endregion

    }
}
