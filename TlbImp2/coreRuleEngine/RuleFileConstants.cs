using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRuleEngine
{
    public class RuleFileConstants
    {
        // Rules
        public static readonly string RootElementName = "Rules";
        // Rule
        public static readonly string RuleElementName = "Rule";
        public static readonly string RulesElementCategoryAttributeName = "Category";
        public static readonly string RulesElementRuleNameAttributeName = "Name";
        public static readonly string DefaultRuleName = "Rule";
        // Match
        public static readonly string ConditionElementName = "Condition";
        public static readonly string OrElementName = "Or";
        public static readonly string AndElementName = "And";
        // Action
        public static readonly string ActionElementName = "Action";

        // others
        public static readonly string Operator = "Operator";
        public static readonly string Name = "Name";
        public static readonly string Key = "Key";
        public static readonly string Value = "Value";
        public static readonly string Parameter = "Parameter";
    }
}
