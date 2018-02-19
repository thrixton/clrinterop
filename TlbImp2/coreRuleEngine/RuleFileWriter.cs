using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace CoreRuleEngine
{
    /// <summary>
    /// RuleFileWriter is the writer of a rule file, and is used to write a RuleSet instance into a
    /// XmlDocument.
    /// </summary>
    public class RuleFileWriter
    {
        private RuleSet m_ruleSet;

        private XmlDocument m_doc;

        public RuleFileWriter(RuleSet ruleSet)
        {
            m_ruleSet = ruleSet;
        }

        public XmlDocument WriteToXmlDocument()
        {
            m_doc = new XmlDocument();
            XmlElement rootElement = m_doc.CreateElement(RuleFileConstants.RootElementName);
            List<Rule> ruleList = m_ruleSet.GetAllRules();
            foreach (Rule rule in ruleList)
            {
                rootElement.AppendChild(WriteRuleToXmlNode(rule));
            }
            m_doc.AppendChild(rootElement);
            return m_doc;
        }

        private XmlNode WriteRuleToXmlNode(Rule rule)
        {
            XmlElement ruleElement = m_doc.CreateElement(RuleFileConstants.RuleElementName);
            ruleElement.SetAttribute(RuleFileConstants.RulesElementRuleNameAttributeName,
                rule.Name);
            ruleElement.SetAttribute(RuleFileConstants.RulesElementCategoryAttributeName,
                rule.Category.GetCategoryName());
            ruleElement.AppendChild(WriteMatchToXmlNode(rule.Condition));
            if (!rule.Action.IsInitialized)
            {
                throw new ActionUninitializedException(rule.Name);
            }
            else
            {
                ruleElement.AppendChild(WriteActionToXmlNode(rule.Action));
            }
            return ruleElement;
        }

        private XmlNode WriteActionToXmlNode(IAction action)
        {
            // <Action>
            XmlElement actionNode = m_doc.CreateElement(RuleFileConstants.ActionElementName);
            actionNode.SetAttribute(RuleFileConstants.Name, action.GetActionDef().GetActionName());

            AbstractActionManager actionManager = RuleEngine.GetActionManager();
            IActionDef actionDef = actionManager.GetActionDef(action.GetActionDef().GetActionName());
            List<string> paramList = actionDef.GetParameterNames();
            foreach (string paramName in paramList)
            {
                // <Param>
                XmlElement newNameParamNode = m_doc.CreateElement(RuleFileConstants.Parameter);
                newNameParamNode.SetAttribute(RuleFileConstants.Key, paramName);
                newNameParamNode.SetAttribute(RuleFileConstants.Value,
                    action.GetParameterValue(paramName));
                actionNode.AppendChild(newNameParamNode);
            }
            return actionNode;
        }

        private XmlNode WriteMatchToXmlNode(ICondition condition)
        {
            XmlElement matchElement = m_doc.CreateElement(RuleFileConstants.ConditionElementName);
            matchElement.AppendChild(WriteConditionToXmlNode(condition));
            return matchElement;
        }

        private XmlNode WriteConditionToXmlNode(ICondition condition)
        {
            string conditionName = condition.GetConditionDef().GetConditionName();
            XmlElement conditionElement = m_doc.CreateElement(conditionName);
            if (condition is AbstractCompositeCondition)
            {
                if (condition is AbstractMultipleCondition)
                {
                    AbstractMultipleCondition multipleCondition = condition as AbstractMultipleCondition;
                    foreach (ICondition subCondition in multipleCondition.ConditionList)
                    {
                        conditionElement.AppendChild(WriteConditionToXmlNode(subCondition));
                    }
                }
                else
                {
                    AbstractSingleCondition singleCondition = condition as AbstractSingleCondition;
                    conditionElement.AppendChild(
                        WriteConditionToXmlNode(singleCondition.ConditionList[0]));
                }
            }
            else
            {
                AbstractAtomicCondition atomicCondition = condition as AbstractAtomicCondition;
                conditionElement.SetAttribute(RuleFileConstants.Operator,
                    atomicCondition.Operator.GetOperatorName());
                conditionElement.SetAttribute(RuleFileConstants.Value, atomicCondition.Value);
            }
            return conditionElement;
        }
    }
}
