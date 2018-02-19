using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;

namespace CoreRuleEngine
{
    /// <summary>
    /// A collection of rule instances.
    /// RuleSet uses a dictionary to manage the rules. And, the dictionary uses the category name of rule
    /// as the key.
    /// </summary>
    public class RuleSet
    {
        /// <summary>
        /// Key is the key string returned from GetRuleKeyString;
        /// Value is Rule List.
        /// </summary>
        private Dictionary<string, List<Rule>> m_ruleDict;

        /// <summary>
        /// Load the rules from rule xml file.
        /// </summary>
         public RuleSet()
        {
            m_ruleDict = new Dictionary<string, List<Rule>>();
        }

        public List<Rule> GetAllRules()
        {
            List<Rule> rules = new List<Rule>();
            Dictionary<string, List<Rule>>.Enumerator enumerator =
                m_ruleDict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                List<Rule> value = enumerator.Current.Value;
                rules.AddRange(value);
            }
            return rules;
        }

        /// <summary>
        /// Get the all rules if the rule have the same category with the parameter,
        /// the same ActionDef with the parameter, and the "target" satisified the rule.
        /// </summary>
        public List<Rule> GetRule(ICategory category, IActionDef actionDef, IMatchTarget target)
        {
            List<Rule> matchedRuleList = new List<Rule>();
            string key = GetRuleKeyString(category.GetCategoryName());
            if (!m_ruleDict.ContainsKey(key))
                return matchedRuleList;
            List<Rule> ruleList = m_ruleDict[key];
            foreach (Rule rule in ruleList)
            {
                if (rule.Action.GetActionDef() == actionDef &&
                    rule.Condition.Match(target))
                {
                    matchedRuleList.Add(rule);
                }
            }
            return matchedRuleList;
        }

        public void AddRule(Rule rule)
        {
            string key = GetRuleKeyString(rule.Category.GetCategoryName());
            if (!m_ruleDict.ContainsKey(key))
                m_ruleDict[key] = new List<Rule>();
            m_ruleDict[key].Add(rule);
        }

        /// <summary>
        /// Used to organize the Rule in Dictionary.
        /// We use category name as the key.
        /// </summary>
        private string GetRuleKeyString(string categoryName)
        {
            return categoryName;
        }

        public void RemoveRule(Rule rule)
        {
            Dictionary<string, List<Rule>>.Enumerator enumerator =
                m_ruleDict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                List<Rule> value = enumerator.Current.Value;
                value.Remove(rule);
            }
        }

        public void RemoveAllRules()
        {
            m_ruleDict.Clear();
        }
    }
}
