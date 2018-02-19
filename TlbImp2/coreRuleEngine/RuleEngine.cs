using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRuleEngine
{
    /// <summary>
    /// RuleEngine manages four following managers:
    ///     AbstractCategoryManager,
    ///     AbstractActionManager,
    ///     AbstractConditionManager,
    ///     AbstractOperatorManager.
    /// In the beginning of a rule/action based system. It is needed to call RuleEngine.InitRuleEngine
    /// method with the four implementations of these four abstract managers. So, that the four managers
    /// are registered.
    /// RuleEngine also offers the four static method to access the four managers registered.
    /// </summary>
    public class RuleEngine
    {
        private static AbstractActionManager s_actionManager;

        private static AbstractCategoryManager s_categoryManager;

        private static AbstractConditionManager s_conditionManager;

        private static AbstractOperatorManager s_operatorManager;

        public static void InitRuleEngine(AbstractActionManager actionManager,
                                  AbstractCategoryManager categoryManager,
                                  AbstractConditionManager conditionManager,
                                  AbstractOperatorManager operatorManager)
        {
            s_actionManager = actionManager;
            s_categoryManager = categoryManager;
            s_conditionManager = conditionManager;
            s_operatorManager = operatorManager;
        }

        public static AbstractConditionManager GetConditionManager()
        {
            return s_conditionManager;
        }

        public static AbstractActionManager GetActionManager()
        {
            return s_actionManager;
        }

        public static AbstractCategoryManager GetCategoryManager()
        {
            return s_categoryManager;
        }

        public static AbstractOperatorManager GetOperatorManager()
        {
            return s_operatorManager;
        }
    }
}
