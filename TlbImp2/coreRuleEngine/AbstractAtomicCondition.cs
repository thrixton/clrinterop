using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRuleEngine
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractAtomicConditionDef : IConditionDef
    {
        abstract public List<string> GetMostPossibleValues();

        #region IConditionDef Members

        abstract public ICondition Create();

        abstract public bool CanApplyToCategory(ICategory category);

        abstract public string GetConditionName();

        #endregion
    }

    /// <summary>
    /// An atomic condition is a condition that does not have subconditions.
    /// The Operator property is the predicte of the Condition.
    /// The Value property is the object part of the Condition statement.
    /// </summary>
    public abstract class AbstractAtomicCondition : ICondition
    {
        protected string m_value;

        protected IOperator m_operator;

        public IOperator Operator
        {
            get
            {
                return m_operator;
            }
            set
            {
                m_operator = value;
            }
        }

        public string Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        /// <summary>
        /// Two atomic conditions are equal, if they belong to the same ConditionDef, and they have
        /// the same Operator and Value.
        /// This method can be used to determine whether an atomic condition is modified.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is AbstractAtomicCondition)
            {
                AbstractAtomicCondition condition = obj as AbstractAtomicCondition;
                return (GetConditionDef() == condition.GetConditionDef() &&
                                    m_operator == condition.m_operator &&
                                    m_value == condition.m_value);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return GetConditionDef().GetConditionName().GetHashCode() +
                m_operator.GetOperatorName().GetHashCode() +
                m_value.GetHashCode();
        }

        #region ICondition Members

        abstract public bool Match(IMatchTarget matchTarget);

        abstract public IConditionDef GetConditionDef();

        public virtual string GetExpression()
        {
            return GetConditionDef().GetConditionName() + " " +
                m_operator.GetOperatorName() + " '" +
                m_value + "'";
        }
        
        #endregion
    }
}
