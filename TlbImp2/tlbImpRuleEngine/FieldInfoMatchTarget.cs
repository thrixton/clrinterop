using System;
using System.Collections.Generic;
using System.Text;
using TypeLibTypes.Interop;
using CoreRuleEngine;

namespace TlbImpRuleEngine
{
    public class FieldInfoMatchTarget : IMatchTarget, IGetTypeLibElementCommonInfo, IGetNativeParentName
    {
        private const string TypeString = "Field";

        private TypeInfo m_parentTypeInfo;

        private int m_index;

        private VarDesc m_varDesc;

        private string m_name;

        private string m_nativeParentTypeName;

        public FieldInfoMatchTarget(TypeInfo parentTypeInfo, int index)
        {
            m_parentTypeInfo = parentTypeInfo;
            m_index = index;
            m_varDesc = m_parentTypeInfo.GetVarDesc(m_index);
            m_nativeParentTypeName = m_parentTypeInfo.GetDocumentation();
        }

        public TypeInfo TypeInfo
        {
            get
            {
                return m_parentTypeInfo;
            }
        }

        public int Index
        {
            get
            {
                return m_index;
            }
        }

        public VarDesc VarDesc
        {
            get
            {
                return m_varDesc;
            }
        }

        #region IMatchTarget Members

        public ICategory GetCategory()
        {
            return FieldCategory.GetInstance();
        }

        #endregion

        #region ITypeLibElementCommonInfo Members

        public string Name
        {
            get
            {
                if (m_name == null)
                {
                    m_name = m_parentTypeInfo.GetDocumentation(m_varDesc.memid);
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
