using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRuleEngine
{
    public abstract class AbstractSingleConditionDef : AbstractCompositeConditionDef
    {
        public override int GetMaxSubconditionNumber()
        {
            return 1;
        }
    }

    /// <summary>
    /// A single condition is a condition that can only have one sub conditions. That means the max sub
    /// condition number is 1.
    /// </summary>
    public abstract class AbstractSingleCondition : AbstractCompositeCondition
    {
        public override string GetExpression()
        {
            if (m_list.Count == 0)
                return "";
            if (m_list.Count == 1)
                return GetConditionDef().GetConditionName() + " ( " +
                    m_list[0].GetExpression() + " )";
            return null;
        }
    }
}
