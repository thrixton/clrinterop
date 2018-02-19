using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Reflection;

namespace CoreRuleEngine
{
    /// <summary>
    /// The definition of the ICondition.
    /// Every ICondition implementation has a corresponding IConditionDef implementation. The IConditionDef class
    /// is responable for creating the ICondition instances, and the common information of the ICondition instances.
    /// </summary>
    public interface IConditionDef
    {
        /// <summary>
        /// Create the corresponding ICondition instance.
        /// </summary>
        ICondition Create();

        /// <summary>
        /// Tell if this kind of ICondition can be applied to the category.
        /// Every kind of ICondition has a list of Category that it can be applied on.
        /// </summary>
        bool CanApplyToCategory(ICategory category);

        /// <summary>
        /// The name of this kind of Conditions. For example, "And".
        /// </summary>
        string GetConditionName();
    }

    /// <summary>
    /// The interface that all conditions implement.
    /// Condition is used to describe, select and filter the target elements. Only when an element
    /// satisfies the condition, can the rule be applied to the element.
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        /// Tell if the matchTarget satisfy this condition.
        /// </summary>
        bool Match(IMatchTarget matchTarget);

        /// <summary>
        /// Get the definition of the condition, which is also the creator of the condition.
        /// </summary>
        IConditionDef GetConditionDef();

        /// <summary>
        /// Get the expression of condition.
        /// For multiple condition, such as AND, this method will give a human readable logic expression.
        /// For example, "( NativeName Equal 'abc' ) And ( TypeKind Equal 'Interface') "
        /// </summary>
        string GetExpression();
    }

    /// <summary>
    /// The mananger of the all ConditionDefs.
    /// All IConditionDefs are registered in this manager. Use the condition name as the key to get the
    /// corresponding IConditionDef.
    /// </summary>
    public abstract class AbstractConditionManager
    {
        private Dictionary<string, IConditionDef> m_registeredConditionDef =
            new Dictionary<string, IConditionDef>();

        /// <summary>
        /// Register "And", "Or", "Not" conditionDefs in ctor.
        /// Because, all rule engine based systems will use these three kinds of conditions.
        /// They are basic logic nodes.
        /// </summary>
        protected AbstractConditionManager()
        {
            AndConditionDef andConditionDef = AndConditionDef.GetInstance();
            RegisterCondition(andConditionDef.GetConditionName(), andConditionDef);
            OrConditionDef orConditionDef = OrConditionDef.GetInstance();
            RegisterCondition(orConditionDef.GetConditionName(), orConditionDef);
            NotConditionDef notConditionDef = NotConditionDef.GetInstance();
            RegisterCondition(notConditionDef.GetConditionName(), notConditionDef);
        }

        /// <summary>
        /// Register a condition in this manager.
        /// The condition name is used as the key of IConditionDef in the register collection.
        /// Usually, this method is called in the ctor of AbstractConditionManager's sub classes.
        /// </summary>
        protected void RegisterCondition(string conditionName, IConditionDef conditionDef)
        {
            m_registeredConditionDef.Add(conditionName, conditionDef);
        }

        /// <summary>
        /// Get all IConditionDefs, whose corresponding IConditions can be applied to category.
        /// </summary>
        public List<IConditionDef> GetPossibleConditionDefList(ICategory category)
        {
            List<IConditionDef> conditionDefList = new List<IConditionDef>();
            Dictionary<string, IConditionDef>.Enumerator enumerator =
                m_registeredConditionDef.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Value.CanApplyToCategory(category))
                    conditionDefList.Add(enumerator.Current.Value);
            }
            return conditionDefList;
        }

        /// <summary>
        /// Get all IConditionDefs registered.
        /// </summary>
        public List<IConditionDef> GetAllConditionDefList()
        {
            List<IConditionDef> conditionDefList = new List<IConditionDef>();
            Dictionary<string, IConditionDef>.Enumerator enumerator =
                m_registeredConditionDef.GetEnumerator();
            while (enumerator.MoveNext())
            {
                conditionDefList.Add(enumerator.Current.Value);
            }
            return conditionDefList;
        }

        /// <summary>
        /// Get the IConditionDef by condition name.
        /// The condition name of an IConditionDef is used as the key of IConditionDef.
        /// </summary>
        public IConditionDef GetConditionDef(string conditionName)
        {
            if (m_registeredConditionDef.ContainsKey(conditionName))
                return m_registeredConditionDef[conditionName];
            else
                return null;
        }

        /// <summary>
        /// Create condition using the IConditionDef whose name is "conditionName".
        /// </summary>
        public ICondition CreateCondition(string conditionName)
        {
            if (m_registeredConditionDef.ContainsKey(conditionName))
            {
                IConditionDef conditionDef = m_registeredConditionDef[conditionName];
                ICondition action = conditionDef.Create();
                return action;
            }
            return null;
        }

    }
}
