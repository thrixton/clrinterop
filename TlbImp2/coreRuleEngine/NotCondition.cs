using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRuleEngine
{
    /// <summary>
    /// The definition of NotCondition.
    /// NotConditionDef is the creator of NotCondition instances.
    /// In this definition, the condition name of NotConditions is set "Not".
    /// </summary>
    public class NotConditionDef : AbstractSingleConditionDef
    {
        private const string s_conditionName = "Not";

        private NotConditionDef() { }

        private static NotConditionDef s_notConditionDef = new NotConditionDef();

        public static NotConditionDef GetInstance()
        {
            return s_notConditionDef;
        }

        public override ICondition Create()
        {
            return new NotCondition();
        }

        public override string GetConditionName()
        {
            return s_conditionName;
        }
    }

    /// <summary>
    /// The Condition for Not logic.
    /// NotCondition has only one subcondition.
    /// A NotCondition returns true, if its subcondition returns false. That means its
    /// subcondition is not satisfied by MatchTarget.
    /// </summary>
    public class NotCondition : AbstractSingleCondition
    {
        public NotCondition()
        {
            m_list = new List<ICondition>();
        }

        public override bool Match(IMatchTarget matchTarget)
        {
            return !m_list[0].Match(matchTarget);
        }

        public override IConditionDef GetConditionDef()
        {
            return NotConditionDef.GetInstance();
        }
    }
}
