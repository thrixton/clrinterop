using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CoreRuleEngine;
using TlbImpRuleEngine;

namespace TlbImpRuleFileEditor
{
    public partial class RuleForm : Form
    {
        public RuleForm(string ruleName)
        {
            InitializeComponent();
            textBoxName.Text = ruleName;
        }

        private ICategory m_fixedCategory;

        public RuleForm(ICategory fixedCategory, string ruleName)
        {
            InitializeComponent();
            m_fixedCategory = fixedCategory;
            textBoxName.Text = ruleName;
        }

        public ICategory GetCategory()
        {
            if (m_fixedCategory != null)
                return m_fixedCategory;

            if (categoryComboBox.SelectedItem == null)
                return null;
            return RuleEngine.GetCategoryManager().GetCategory(
                categoryComboBox.SelectedItem.ToString());
        }

        public IAction GetAction()
        {
            if (actionComboBox.SelectedItem == null)
                return null;
            string actionName = actionComboBox.SelectedItem.ToString();
            return RuleEngine.GetActionManager().CreateAction(actionName);
        }

        public string GetRuleName()
        {
            return textBoxName.Text.Trim();
        }

        private void RuleForm_Load(object sender, EventArgs e)
        {
            labelInstruction.Text = Resource.FormatString("Msg_InstructionAboutAddNewRule");
            AbstractCategoryManager categoryManager = RuleEngine.GetCategoryManager();
            List<ICategory> categoryList = categoryManager.GetAllCategory();
            categoryComboBox.Items.Clear();
            foreach (ICategory category in categoryList)
            {
                categoryComboBox.Items.Add(category.GetCategoryName());
            }
            if (m_fixedCategory != null)
            {
                categoryComboBox.SelectedItem = m_fixedCategory.GetCategoryName();
                categoryComboBox.Enabled = false;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void categoryComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (categoryComboBox.SelectedItem == null)
                return;
            string categoryName = categoryComboBox.SelectedItem as string;
            AbstractCategoryManager categoryManager = RuleEngine.GetCategoryManager();
            ICategory category = categoryManager.GetCategory(categoryName);
            AbstractActionManager actionManager = RuleEngine.GetActionManager();
            List<IActionDef> actionDefList =
                actionManager.GetPossibleActionDefList(category);
            actionComboBox.Items.Clear();
            foreach (IActionDef actionDef in actionDefList)
            {
                actionComboBox.Items.Add(actionDef.GetActionName());
            }
            actionComboBox.SelectedIndex = -1;
            CheckOKButtonEnabled();
        }

        private void CheckOKButtonEnabled()
        {
            bool valueCheck = true;
            if (categoryComboBox.SelectedItem == null ||
                actionComboBox.SelectedItem == null ||
                textBoxName.Text.Trim().Equals(""))
            {
                valueCheck = false;
            }
            okButton.Enabled = valueCheck;
        }

        private void actionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (actionComboBox.SelectedItem == null)
                return;
            string actionName = actionComboBox.SelectedItem as string;
            AbstractActionManager actionManager = RuleEngine.GetActionManager();
            IActionDef actionDef = actionManager.GetActionDef(actionName);
            if (actionDef == null)
                return;
            string actionDescription =
                Resource.FormatString("Msg_ActionDescription_" + actionDef.GetActionName());
            textBoxDescription.Text = actionDescription;
            CheckOKButtonEnabled();
        }

        private void textBoxName_TextChanged(object sender, EventArgs e)
        {
            CheckOKButtonEnabled();
        }

    }
}
