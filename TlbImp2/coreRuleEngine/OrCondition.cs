using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRuleEngine
{
    /// <summary>
    /// The definition of OrCondition.
    /// OrConditionDef is the creator of OrCondition instances.
    /// In this definition, the condition name of OrConditions is set "Or".
    /// </summary>
    public class OrConditionDef : AbstractMultipleConditionDef
    {
        private const string ConditionName = "Or";

        private OrConditionDef() { }

        private static OrConditionDef s_orConditionDef = new OrConditionDef();

        public static OrConditionDef GetInstance()
        {
            return s_orConditionDef;
        }

        public override ICondition Create()
        {
            return new OrCondition();
        }

        public override string GetConditionName()
        {
            return ConditionName;
        }
    }

    /// <summary>
    /// The Condition for Or logic.
    /// A OrCondition returns true, if any one of its subconditions returns true. That means at least
    /// one of its subconditions is satisfied by MatchTarget.
    /// </summary>
    public class OrCondition : AbstractMultipleCondition
    {
        public OrCondition()
        {
            m_list = new List<ICondition>();
        }

        override public bool Match(IMatchTarget matchTarget)
        {
            for (int i = 0; i < m_list.Count; i++)
            {
                if (m_list[i].Match(matchTarget))
                {
                    return true;
                }
            }
            return false;
        }

        public override IConditionDef GetConditionDef()
        {
            return OrConditionDef.GetInstance();
        }
    }
}
