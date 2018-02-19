using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using CoreRuleEngine;
using System.Drawing;
using System.Text.RegularExpressions;

namespace TlbImpRuleFileEditor
{
    class RichTextBoxConditionExpression : RichTextBox
    {
        static readonly Color DEFAULT_COLOR = Color.Black;
        static readonly Color LOGIC_COLOR = Color.Black;
        static readonly Color OPERATOR_COLOR = Color.Green;
        static readonly Color CONDITION_NAME_COLOR = Color.Blue;
        static readonly Color VALUE_COLOR = Color.Purple;

        private List<string> m_logicKeywords;
        private List<string> m_operatorKeywords;
        private List<string> m_conditionNameKeywords;

        private List<string> GetLogicKeywords()
        {
            if (m_logicKeywords == null)
            {
                m_logicKeywords = new List<string>();
                // logic key words
                List<IConditionDef> conditionDefList =
                    RuleEngine.GetConditionManager().GetAllConditionDefList();
                foreach (IConditionDef conditionDef in conditionDefList)
                {
                    if (conditionDef is AbstractCompositeConditionDef)
                        m_logicKeywords.Add(conditionDef.GetConditionName());
                }
            }
            return m_logicKeywords;
        }

        private List<string> GetConditionNameKeywords()
        {
            if (m_conditionNameKeywords == null)
            {
                m_conditionNameKeywords = new List<string>();
                // condition key words
                List<IConditionDef> conditionDefList =
                    RuleEngine.GetConditionManager().GetAllConditionDefList();
                foreach (IConditionDef conditionDef in conditionDefList)
                {
                    if (!(conditionDef is AbstractCompositeConditionDef))
                        m_conditionNameKeywords.Add(conditionDef.GetConditionName());
                }
            }
            return m_conditionNameKeywords;
        }

        private List<string> GetOperatorKeywords()
        {
            if (m_operatorKeywords == null)
            {
                m_operatorKeywords = new List<string>();
                // operator key words
                List<IOperator> operatorList =
                    RuleEngine.GetOperatorManager().GetAllOperatorList();
                foreach (IOperator iOperator in operatorList)
                    m_operatorKeywords.Add(iOperator.GetOperatorName());
            }
            return m_operatorKeywords;
        }

        public void UpdateText(Rule rule)
        {
            this.Text = rule.Condition.GetExpression();
            this.SelectAll();
            this.SelectionColor = DEFAULT_COLOR;
            HighlightText();
        }

        private void HighlightText()
        {
            // logic key words
            foreach (string keyword in GetLogicKeywords())
                HighlightKeyword(keyword, LOGIC_COLOR);
            // condition key words
            foreach (string keyword in GetConditionNameKeywords())
                HighlightKeyword(keyword, CONDITION_NAME_COLOR);
            // operator key words
            foreach (string keyword in GetOperatorKeywords())
                HighlightKeyword(keyword, OPERATOR_COLOR);
            // value words
            Regex valueReg = new Regex(@"'[^']*'");
            HighlightKeyword(valueReg, VALUE_COLOR);
        }

        private void HighlightKeyword(Regex reg, Color color)
        {
            MatchCollection mc = reg.Matches(this.Text);
            foreach (Match match in mc)
            {
                this.Select(match.Index, match.Length);
                this.SelectionColor = color;
            }
        }

        private void HighlightKeyword(string keyword, Color color)
        {
            Regex reg = new Regex("\\b" + keyword + "\\b");
            HighlightKeyword(reg, color);
        }
    }
}
