using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using CoreRuleEngine;
using TlbImpRuleEngine;
using System.Globalization;

namespace TlbImpRuleFileEditor
{
    public partial class ConditionInPlaceEditor : UserControl
    {
        private TreeNode m_modifiedTreeNode;

        private ICategory m_fixedCategory;

        /// <summary>
        /// If the ConditionInPlaceEditor is used on an empty condition, this field is null.
        /// </summary>
        private ICondition m_fixedCondition;

        private bool m_isProcessed;

        public ConditionInPlaceEditor()
        {
            InitializeComponent();
        }

        public bool IsProcesssed
        {
            get
            {
                return m_isProcessed;
            }
            set
            {
                m_isProcessed = value;
            }
        }

        public TreeNode ModifiedTreeNode
        {
            get
            {
                return m_modifiedTreeNode;
            }
        }

        public void InitBeforeShow(TreeNode modifiedTreeNode, 
            ICategory fixedCategory, ICondition fixedCondition)
        {
            m_modifiedTreeNode = modifiedTreeNode;
            m_fixedCategory = fixedCategory;
            m_fixedCondition = fixedCondition;
            // Init conditionComboBox
            List<IConditionDef> conditionDefList =
                RuleEngine.GetConditionManager().GetPossibleConditionDefList(m_fixedCategory);
            conditionComboBox.Items.Clear();
            foreach (IConditionDef conditionDef in conditionDefList)
            {
                conditionComboBox.Items.Add(conditionDef.GetConditionName());
            }
            if (m_fixedCondition != null)
            {
                // Select the inital one.
                // And this line, will call conditionComboBox_SelectedIndexChanged synchronously.
                conditionComboBox.SelectedItem =
                    m_fixedCondition.GetConditionDef().GetConditionName();
                //ResizeControl();
                //UpdateOperatorComboBox();
                //UpdateValueComboBox();
                if (m_fixedCondition is AbstractAtomicCondition)
                {
                    AbstractAtomicCondition atomicCondition = m_fixedCondition as AbstractAtomicCondition;
                    operatorComboBox.SelectedItem = atomicCondition.Operator.GetOperatorName();
                    if (m_fixedCondition is NativeParameterIndexCondition)
                    {
                        parameterIndexNumericUpDown.Value = Int32.Parse(
                            atomicCondition.Value, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        valueComboBox.Text = atomicCondition.Value;
                    }
                }
            }
            else
            {
                SetControlVisibility();
            }
        }

        private void SetControlVisibility()
        {
            this.operatorComboBox.Visible = false;
            this.parameterIndexNumericUpDown.Visible = false;
            this.valueComboBox.Visible = false;
            this.buttonValueInputHelper.Visible = false;
            if (conditionComboBox.SelectedItem != null)
            {
                string conditionName = conditionComboBox.SelectedItem.ToString();
                IConditionDef conditionDef =
                    RuleEngine.GetConditionManager().GetConditionDef(conditionName);
                if (conditionDef is AbstractAtomicConditionDef)
                {
                    if (conditionDef == NativeSignatureConditionDef.GetInstance())
                    {
                        this.operatorComboBox.Visible = true;
                        this.valueComboBox.Visible = true;
                        this.buttonValueInputHelper.Visible = true;
                    }
                    else if (conditionDef == NativeParameterIndexConditionDef.GetInstance())
                    {
                        this.operatorComboBox.Visible = true;
                        this.parameterIndexNumericUpDown.Visible = true;
                    }
                    else
                    {
                        this.operatorComboBox.Visible = true;
                        this.valueComboBox.Visible = true;
                    }
                    return;
                }
            }
            // Composite Condition or Empty Condition
            this.Width = conditionComboBox.Width;
        }

        private void UpdateOperatorComboBox()
        {
            if (conditionComboBox.SelectedItem == null)
                return;
            string conditionName = conditionComboBox.SelectedItem.ToString();
            IConditionDef conditionDef =
                RuleEngine.GetConditionManager().GetConditionDef(conditionName);
            if (conditionDef is AbstractAtomicConditionDef)
            {
                // Init Operators
                List<IOperator> operatorList =
                    RuleEngine.GetOperatorManager().GetPossibleOperatorList(conditionDef);
                operatorComboBox.Items.Clear();
                foreach (IOperator Operator in operatorList)
                {
                    operatorComboBox.Items.Add(Operator.GetOperatorName());
                }
                // default for operator
                if (operatorComboBox.Items.Count != 0)
                    operatorComboBox.SelectedIndex = 0;
                else
                    operatorComboBox.SelectedIndex = -1;
            }
        }

        private void UpdateValuePart()
        {
            if (conditionComboBox.SelectedItem == null)
                return;
            string conditionName = conditionComboBox.SelectedItem.ToString();
            IConditionDef conditionDef =
                RuleEngine.GetConditionManager().GetConditionDef(conditionName);
            if (conditionDef is AbstractAtomicConditionDef)
            {
                AbstractAtomicConditionDef atomicConditionDef =
                    conditionDef as AbstractAtomicConditionDef;
                if (atomicConditionDef == NativeParameterIndexConditionDef.GetInstance())
                {
                    valueComboBox.Visible = false;
                    parameterIndexNumericUpDown.Visible = true;
                }
                else
                {
                    parameterIndexNumericUpDown.Visible = false;
                    valueComboBox.Visible = true;
                    valueComboBox.Text = "";
                    valueComboBox.Items.Clear();
                    // Disable valueComboBox? If yes, we need the ValueInputHelper.
                    if (atomicConditionDef == NativeSignatureConditionDef.GetInstance())
                        valueComboBox.Enabled = false;
                    else
                        valueComboBox.Enabled = true;
                    List<string> list = atomicConditionDef.GetMostPossibleValues();
                    if (list != null)
                    {
                        foreach (string stringValue in list)
                            valueComboBox.Items.Add(stringValue);
                        if (valueComboBox.Items.Count != 0)
                            valueComboBox.SelectedIndex = 0;
                    }
                }
            }
        }

        private void conditionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetControlVisibility();
            UpdateOperatorComboBox();
            UpdateValuePart();
        }

        internal object GetConditionComboBoxSelectedItem()
        {
            return conditionComboBox.SelectedItem;
        }

        internal ICondition GetCondition()
        {
            if (conditionComboBox.SelectedItem == null)
                return null;
            AbstractConditionManager conditionManager = RuleEngine.GetConditionManager();
            ICondition condition =
                conditionManager.CreateCondition(conditionComboBox.SelectedItem.ToString());
            if (condition is AbstractAtomicCondition)
            {
                AbstractAtomicCondition atomicCondition =
                    condition as AbstractAtomicCondition;
                if (operatorComboBox.SelectedItem == null)
                    return null;
                string operatorName = operatorComboBox.SelectedItem.ToString();
                atomicCondition.Operator = RuleEngine.GetOperatorManager().GetOperator(operatorName);
                if (atomicCondition is NativeParameterIndexCondition)
                {
                    atomicCondition.Value = "" + parameterIndexNumericUpDown.Value;
                }
                else
                {
                    if (valueComboBox.Text.Trim().Equals(""))
                        return null;
                    atomicCondition.Value = valueComboBox.Text;
                }
                return atomicCondition;
            }
            else
            {
                return condition;
            }
        }

        private void buttonValueInputHelper_Click(object sender, EventArgs e)
        {
            string conditionName = conditionComboBox.SelectedItem.ToString();
            IConditionDef conditionDef =
                RuleEngine.GetConditionManager().GetConditionDef(conditionName);

            if (conditionDef == NativeSignatureConditionDef.GetInstance())
            {
                NativeSignatureInputHelper form = null;
                if (valueComboBox.Text.Trim().Equals(""))
                    form = new NativeSignatureInputHelper();
                else
                    form = new NativeSignatureInputHelper(valueComboBox.Text);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    valueComboBox.Text = form.GetTypeString();
                }
            }
        }

        private void valueComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    this.Visible = false;
                    break;
            }
        }

    }
}
