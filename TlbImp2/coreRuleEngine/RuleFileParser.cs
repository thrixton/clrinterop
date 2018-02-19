using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace CoreRuleEngine
{
    /// <summary>
    /// RuleFileParser is the reader of the rule file, and is used to parse the xml rule file to RuleSet.
    /// </summary>
    public class RuleFileParser
    {
        private XmlDocument m_doc;

        public RuleFileParser(string ruleFilePath)
        {
            m_doc = new XmlDocument();
            m_doc.Load(ruleFilePath);
        }

        public RuleSet Parse()
        {
            RuleSet ruleSet = new RuleSet();

            XmlNodeList nodeList = m_doc.GetElementsByTagName(RuleFileConstants.RuleElementName);
            foreach (XmlNode node in nodeList)
            {
                // Rule node
                Rule rule = ProcessRuleNode(node);
                ruleSet.AddRule(rule);
            }
            return ruleSet;
        }

        private Rule ProcessRuleNode(XmlNode ruleNode)
        {
            // for name attribute
            string nameString = RuleFileConstants.DefaultRuleName;
            XmlNode nameAttributeNode = ruleNode.Attributes.GetNamedItem(
                    RuleFileConstants.RulesElementRuleNameAttributeName);
            if (nameAttributeNode != null)
                nameString = nameAttributeNode.InnerText;

            // for category attribute
            string categoryString = ruleNode.Attributes.GetNamedItem(
                    RuleFileConstants.RulesElementCategoryAttributeName).InnerText;
            AbstractCategoryManager categoryManager = RuleEngine.GetCategoryManager();
            ICategory category = categoryManager.GetCategory(categoryString);
            if (category == null)
                throw new NoCategoryAttributeException(nameString);

            // for action
            XmlNodeList actionList = ruleNode.SelectNodes(RuleFileConstants.ActionElementName);
            if (actionList.Count != 1)
                throw new ActionNodeNumberException(nameString);
            IAction action = ProcessActionNode(actionList[0]);

            // for condition
            XmlNodeList conditionList = ruleNode.SelectNodes(RuleFileConstants.ConditionElementName);
            if (conditionList.Count != 1)
                throw new ConditionNodeNumberException(nameString);
            ICondition condition = ProcessMatchNode(conditionList[0], nameString);

            return new Rule(category, action, condition, nameString);
        }

        private ICondition ProcessMatchNode(XmlNode matchNode, string ruleName)
        {
            if (matchNode.ChildNodes.Count != 1)
                throw new RootConditionNumberException(ruleName);

            // "And"
            XmlNode conditionNode = matchNode.ChildNodes[0];
            return ProcessConditionNode(conditionNode);
        }

        private ICondition ProcessConditionNode(XmlNode conditionNode)
        {
            AbstractConditionManager conditionManager = RuleEngine.GetConditionManager();
            ICondition condition = conditionManager.CreateCondition(conditionNode.Name);
            if (condition is AbstractCompositeCondition)
            {
                if (condition is AbstractMultipleCondition)
                {
                    AbstractMultipleCondition multipleCondition =
                        condition as AbstractMultipleCondition;
                    foreach (XmlNode childConditionNode in conditionNode.ChildNodes)
                    {
                        ICondition childCondition = ProcessConditionNode(childConditionNode);
                        multipleCondition.AppendCondition(childCondition);
                    }
                    return multipleCondition;
                }
                else
                {
                    AbstractSingleCondition singleCondition =
                        condition as AbstractSingleCondition;
                    ICondition childCondition = ProcessConditionNode(conditionNode.ChildNodes[0]);
                    singleCondition.AppendCondition(childCondition);
                    return singleCondition;
                }
            }
            else
            {
                AbstractAtomicCondition atomicCondition = condition as AbstractAtomicCondition;
                string operatorName = conditionNode.Attributes[RuleFileConstants.Operator].InnerText;
                atomicCondition.Operator = RuleEngine.GetOperatorManager().GetOperator(operatorName);
                atomicCondition.Value =
                    conditionNode.Attributes[RuleFileConstants.Value].InnerText;
                return atomicCondition;
            }
        }

        private IAction ProcessActionNode(XmlNode actionNode)
        {
            AbstractActionManager actionManager = RuleEngine.GetActionManager();
            string actionName = actionNode.Attributes[RuleFileConstants.Name].InnerText;
            Dictionary<string, string> parameters = GetActionParameters(actionNode);
            return actionManager.CreateAction(actionName, parameters);
        }

        private Dictionary<string, string> GetActionParameters(XmlNode actionNode)
        {
            Dictionary<string, string> paramDict = new Dictionary<string, string>();
            XmlNodeList paramList = actionNode.SelectNodes(RuleFileConstants.Parameter);
            foreach (XmlNode paramNode in paramList)
            {
                string key = paramNode.Attributes[RuleFileConstants.Key].InnerText;
                string value = paramNode.Attributes[RuleFileConstants.Value].InnerText;
                paramDict.Add(key, value);
            }
            return paramDict;
        }
    }
}
