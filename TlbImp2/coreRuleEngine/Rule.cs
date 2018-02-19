using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRuleEngine
{
    /// <summary>
    /// A Rule is a combination of Category, Condition and Action:
    /// 1) Category:  What kind of MatchTarget this rule can be applied to.
    /// 2) Condition: If and only if what condition is satisfied by the element,
    ///               this rule is applied to the element.
    /// 3) Action:    What the rule actually does, when it is applied to a MatchTarget.
    /// </summary>
    public class Rule
    {
        private ICategory m_applyCategory;

        private ICondition m_condition;
        
        private IAction m_action;

        private string m_name;

        public Rule(ICategory applyCategory, string name)
        {
            m_applyCategory = applyCategory;
            m_name = name;
        }

        public Rule(ICategory applyCategory, IAction action, ICondition condition, string name)
        {
            m_condition = condition;
            m_action = action;
            m_applyCategory = applyCategory;
            m_name = name;
        }

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        public ICategory Category
        {
            get
            {
                return m_applyCategory;
            }
        }

        public ICondition Condition
        {
            get
            {
                return m_condition;
            }
            set
            {
                m_condition = value;
            }
        }

        public IAction Action
        {
            get
            {
                return m_action;
            }
            set
            {
                m_action = value;
            }
        }
    }
}