using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRuleEngine
{
    public abstract class AbstractMultipleConditionDef : AbstractCompositeConditionDef
    {
        public override int GetMaxSubconditionNumber()
        {
            return Int32.MaxValue;
        }
    }

    /// <summary>
    /// A multiple condition is a condition that can have more one sub conditions. That means the max sub
    /// condition number is Int32.MaxValue.
    /// </summary>
    public abstract class AbstractMultipleCondition : AbstractCompositeCondition
    {
        public override string GetExpression()
        {
            if (m_list.Count == 0)
                return "";
            if (m_list.Count == 1)
                return m_list[0].GetExpression();
            StringBuilder sb = new StringBuilder();
            sb.Append("( ");
            sb.Append(m_list[0].GetExpression());
            sb.Append(" )");
            for (int i = 1; i < m_list.Count; i++)
            {
                sb.Append(" ");
                sb.Append(GetConditionDef().GetConditionName());
                sb.Append(" ");
                sb.Append("( ");
                sb.Append(m_list[i].GetExpression());
                sb.Append(") ");
            }
            return sb.ToString();
        }
    }
}
