using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRuleEngine
{
    /// <summary>
    /// The definition of composite condition.
    /// </summary>
    public abstract class AbstractCompositeConditionDef : IConditionDef
    {
        /// <summary>
        /// Return the max subconditions that this kind of composite conditions can contain.
        /// </summary>
        public abstract int GetMaxSubconditionNumber();

        #region IConditionDef Members

        abstract public ICondition Create();

        public bool CanApplyToCategory(ICategory category)
        {
            return true;
        }

        abstract public string GetConditionName();

        #endregion
    }

    /// <summary>
    /// A composite condition is a condition that has one or more subconditions.
    /// The value of a composite condition is affected by all the subconditions.
    /// </summary>
    public abstract class AbstractCompositeCondition : ICondition
    {
        protected List<ICondition> m_list;

        public List<ICondition> ConditionList
        {
            get
            {
                return m_list;
            }
        }

        public void AppendCondition(ICondition condition)
        {
            AbstractCompositeConditionDef compositeConditionDef =
                GetConditionDef() as AbstractCompositeConditionDef;
            if (m_list.Count < compositeConditionDef.GetMaxSubconditionNumber())
                m_list.Add(condition);
            else
                throw new AppendSubconditionException(this, condition);
        }

        public void InsertConditionAt(ICondition condition, int index)
        {
            AbstractCompositeConditionDef compositeConditionDef =
                GetConditionDef() as AbstractCompositeConditionDef;
            if (m_list.Count < compositeConditionDef.GetMaxSubconditionNumber())
                m_list.Insert(index, condition);
            else
                throw new AppendSubconditionException(this, condition);
        }

        public void RemoveAllCondition()
        {
            m_list.Clear();
        }

        public void RemoveConditionAt(int index)
        {
            m_list.RemoveAt(index);
        }

        /// <summary>
        /// Two composite conditions are equal, if these two composite conditions belong to the same
        /// ConditionDef.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is AbstractCompositeCondition)
            {
                AbstractCompositeCondition condition = obj as AbstractCompositeCondition;
                return (GetConditionDef() == condition.GetConditionDef());
            }
            return false;
        }

        public override int GetHashCode()
        {
            return GetConditionDef().GetConditionName().GetHashCode();
        }

        #region ICondition Members

        abstract public bool Match(IMatchTarget matchTarget);

        abstract public IConditionDef GetConditionDef();

        abstract public string GetExpression();

        #endregion

        /// <summary>
        /// Replace the subcondition at "index" position with the newCondition.
        /// </summary>
        public void ReplaceCondition(int index, ICondition newCondition)
        {
            m_list.RemoveAt(index);
            m_list.Insert(index, newCondition);
        }
    }
}
