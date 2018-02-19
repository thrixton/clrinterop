using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TlbImpRuleEngine;
using System.Globalization;

namespace TlbImpRuleFileEditor
{
    public partial class ConvertToActionWizard : Form
    {
        private ConvertToAction m_fixedConvertToAction;

        private string m_direction;

        private bool m_byref = false;

        private string m_managedType;

        private string m_marshalAs;

        private string m_attributes;

        private bool m_formLoad = true;

        public ConvertToActionWizard(ConvertToAction fixedConvertToAction)
        {
            InitializeComponent();
            m_fixedConvertToAction = fixedConvertToAction;
            m_direction = fixedConvertToAction.Direction;
            m_byref = fixedConvertToAction.ByRef;
            m_managedType = fixedConvertToAction.ManagedTypeConvertTo;
            m_marshalAs = fixedConvertToAction.UnmanagedTypeMarshalAs;
            m_attributes = fixedConvertToAction.Attributes;
        }

        public ConvertToActionWizard()
        {
            InitializeComponent();
        }

        public string Direction
        {
            get
            {
                return m_direction;
            }
        }

        public bool ByRef
        {
            get
            {
                return m_byref;
            }
        }

        public string ManagedType
        {
            get
            {
                return m_managedType;
            }
        }

        public string MarshalAs
        {
            get
            {
                return m_marshalAs;
            }
        }

        public string Attributes
        {
            get
            {
                return m_attributes;
            }
        }

        private void ConvertToActionWizard_Load(object sender, EventArgs e)
        {
            comboBoxManageType.Items.Clear();
            foreach (string managedType in ConvertToActionConstants.ManagedTypes)
            {
                comboBoxManageType.Items.Add(managedType);
            }
            if (m_managedType != null)
            {
                comboBoxManageType.SelectedItem = m_managedType;
            }
            // Direction
            if (m_direction != null)
            {
                if (m_direction.Equals(ConvertToActionConstants.ParameterDirectionIn))
                {
                    radioButtonIn.Checked = true;
                }
                else if (m_direction.Equals(ConvertToActionConstants.ParameterDirectionOut))
                {
                    radioButtonOut.Checked = true;
                }
                else if (m_direction.Equals(ConvertToActionConstants.ParameterDirectionInOut))
                {
                    radioButtonInOut.Checked = true;
                }
            }
            // ByReference
            if (m_byref)
            {
                checkBoxByRef.Checked = true;
            }
        }

        private void comboBoxManageType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxManageType.SelectedItem == null)
                return;
            string manageTypeString = comboBoxManageType.SelectedItem.ToString();
            if (ConvertToActionConstants.GetManagedTypeConvertTo(manageTypeString) ==
                ManagedTypeConvertTo.LPARRAY)
            {
                groupBoxAdditionalArrayAttribute.Enabled = true;
                if (m_attributes != null)
                {
                    Dictionary<string, string> attributePairDictionary =
                        ConvertToAction.GetConvertToAttributeDictionary(m_attributes);
                    if (attributePairDictionary.ContainsKey(ConvertToActionDef.SizeConst))
                    {
                        checkBoxEnableSizeControl.Checked = true;
                        radioButtonSizeConst.Checked = true;
                        numericUpDownSizeConst.Enabled = true;
                        numericUpDownSizeConst.Value =
                            Int32.Parse(attributePairDictionary[ConvertToActionDef.SizeConst],
                                CultureInfo.InvariantCulture);
                    }
                    else if (attributePairDictionary.ContainsKey(ConvertToActionDef.SizeParamIndex))
                    {
                        checkBoxEnableSizeControl.Checked = true;
                        radioButtonSizeParamIndex.Checked = true;
                        numericUpDownSizeParamIndex.Value =
                            Int32.Parse(attributePairDictionary[ConvertToActionDef.SizeParamIndex],
                                CultureInfo.InvariantCulture);
                    }
                    else if (attributePairDictionary.ContainsKey(ConvertToActionDef.SizeParamIndexOffset))
                    {
                        checkBoxEnableSizeControl.Checked = true;
                        radioButtonSizeParamIndexOffset.Checked = true;
                        comboBoxSizeParamIndexOffset.Text =
                            attributePairDictionary[ConvertToActionDef.SizeParamIndexOffset];
                    }
                }
            }
            else
            {
                groupBoxAdditionalArrayAttribute.Enabled = false;
            }
            comboBoxMarshalAs.Items.Clear();
            comboBoxMarshalAs.Items.Add("(default)");
            List<string> marshalAsTypes =
                ConvertToActionConstants.GetMarshalAsTypes(manageTypeString);
            if (marshalAsTypes != null)
            {
                foreach (string marshalAsType in marshalAsTypes)
                {
                    comboBoxMarshalAs.Items.Add(marshalAsType);
                }
            }
            if (m_marshalAs != null && m_formLoad)
            {
                comboBoxMarshalAs.SelectedItem = m_marshalAs;
                m_formLoad = false;
            }
            else
            {
                comboBoxMarshalAs.SelectedIndex = 0;
            }
            CheckOKButtonEnabled();
        }

        private void CheckOKButtonEnabled()
        {
            if (comboBoxManageType.SelectedItem == null ||
                comboBoxMarshalAs.SelectedItem == null)
            {
                buttonOK.Enabled = false;
            }
            else
            {
                buttonOK.Enabled = true;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            m_managedType = comboBoxManageType.SelectedItem.ToString();

            m_marshalAs = comboBoxMarshalAs.SelectedItem.ToString();

            m_direction = GetDirection();

            m_byref = checkBoxByRef.Checked;

            m_attributes = "[";
            if (groupBoxAdditionalArrayAttribute.Enabled && checkBoxEnableSizeControl.Checked)
            {
                if (radioButtonSizeConst.Checked)
                {
                    m_attributes += ConvertToActionDef.SizeConst + "=" + numericUpDownSizeConst.Value;
                }
                else if (radioButtonSizeParamIndex.Checked)
                {
                    m_attributes += ConvertToActionDef.SizeParamIndex + "=" + numericUpDownSizeParamIndex.Value;
                }
                else if (radioButtonSizeParamIndexOffset.Checked)
                {
                    try
                    {
                        int offset = Int32.Parse(comboBoxSizeParamIndexOffset.Text, CultureInfo.InvariantCulture);
                        if (offset == 0)
                            throw new Exception();
                        m_attributes += ConvertToActionDef.SizeParamIndexOffset + "=" +
                            (offset > 0 ? "+" + offset: "" + offset);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(Resource.FormatString("Wrn_InvalidSizeParamIndexOffset"));
                        return;
                    }
                }
            }
            m_attributes += "]";

            DialogResult = DialogResult.OK;
            Close();
        }

        private string GetDirection()
        {
            if (radioButtonIn.Checked)
                return ConvertToActionConstants.ParameterDirectionIn;
            if (radioButtonOut.Checked)
                return ConvertToActionConstants.ParameterDirectionOut;
            return ConvertToActionConstants.ParameterDirectionInOut;
        }

        private void checkBoxEnableSizeControl_CheckedChanged(object sender, EventArgs e)
        {
            radioButtonSizeConst.Enabled = checkBoxEnableSizeControl.Checked;
            radioButtonSizeParamIndex.Enabled = checkBoxEnableSizeControl.Checked;
            radioButtonSizeParamIndexOffset.Enabled = checkBoxEnableSizeControl.Checked;
            // update numericUpDown(s)
            if (checkBoxEnableSizeControl.Checked)
            {
                numericUpDownSizeConst.Enabled = radioButtonSizeConst.Checked;
                numericUpDownSizeParamIndex.Enabled = radioButtonSizeParamIndex.Checked;
                comboBoxSizeParamIndexOffset.Enabled = radioButtonSizeParamIndexOffset.Checked;
            }
            else
            {
                numericUpDownSizeConst.Enabled = false;
                numericUpDownSizeParamIndex.Enabled = false;
                comboBoxSizeParamIndexOffset.Enabled = false;
            }
        }

        private void radioButtonSizeConst_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownSizeConst.Enabled = radioButtonSizeConst.Checked;
        }

        private void radioButtonSizeParamIndex_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownSizeParamIndex.Enabled = radioButtonSizeParamIndex.Checked;
        }

        private void radioButtonSizeParamIndexOffset_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxSizeParamIndexOffset.Enabled = radioButtonSizeParamIndexOffset.Checked;
        }

        private void comboBoxMarshalAs_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckOKButtonEnabled();
        }

    }
}
