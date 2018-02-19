using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TlbImpRuleEngine;
using TypeLibTypes.Interop;
using System.Globalization;

namespace TlbImpRuleFileEditor
{
    public partial class NativeSignatureInputHelper : Form
    {
        string m_fixedTypeString;

        string m_typeString;

        public NativeSignatureInputHelper()
        {
            InitializeComponent();
        }

        public NativeSignatureInputHelper(string fixedTypeString)
        {
            InitializeComponent();
            m_fixedTypeString = fixedTypeString;
        }

        public string GetTypeString()
        {
            return m_typeString;
        }

        private void NativeSignatureInputHelper_Load(object sender, EventArgs e)
        {
            comboBoxNativeType.Items.Clear();
            List<string> tlbSimpleTypeStrings = TlbType2String.TlbSimpleTypes;
            foreach (string tlbSimpleType in tlbSimpleTypeStrings)
                comboBoxNativeType.Items.Add(tlbSimpleType);
            List<string> tlbUserDefinedTypeStrings = TlbType2String.TlbUserDefinedTypes;
            foreach (string tlbUserDefinedType in tlbUserDefinedTypeStrings)
                comboBoxNativeType.Items.Add(tlbUserDefinedType);
            // ... for UserDefined
            comboBoxNativeType.Items.Add("...");
            if (m_fixedTypeString != null)
            {
                string tlbType = m_fixedTypeString.Split(' ')[0];
                if (!comboBoxNativeType.Items.Contains(tlbType))
                {
                    TlbType2String.AddTlbUserDefinedType(tlbType);
                    comboBoxNativeType.Items.Insert(comboBoxNativeType.Items.Count - 1, tlbType);
                }
                comboBoxNativeType.SelectedItem = tlbType;
                string remain = m_fixedTypeString.Substring(tlbType.Length);
                int indirections = 0;
                while (remain.StartsWith(" *", StringComparison.InvariantCulture))
                {
                    remain = remain.Substring(2);
                    indirections++;
                }
                numericUpDownIndirection.Value = indirections;
                if (remain.StartsWith(" [", StringComparison.InvariantCulture))
                {
                    checkBoxMakeArray.Checked = true;
                    remain = remain.Substring(2, remain.Length - 3);
                    numericUpDownArraySize.Value = Int32.Parse(remain, CultureInfo.InvariantCulture);
                }
            }
            else
            {
                if (comboBoxNativeType.Items.Count != 0)
                    comboBoxNativeType.SelectedIndex = 0;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder(comboBoxNativeType.SelectedItem.ToString());
            for (int i = 0; i < numericUpDownIndirection.Value; i++)
            {
                sb.Append(" *");
            }
            if (checkBoxMakeArray.Checked)
            {
                sb.Append(" [");
                sb.Append(numericUpDownArraySize.Value);
                sb.Append(']');
            }
            m_typeString = sb.ToString();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void checkBoxMakeArray_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownArraySize.Enabled = checkBoxMakeArray.Checked;
        }

        private void comboBoxNativeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxNativeType.SelectedItem != null &&
                comboBoxNativeType.SelectedItem.ToString().Equals("..."))
            {
                DoAddTlbType();
            }
            buttonOK.Enabled = (comboBoxNativeType.SelectedItem != null);
        }

        private void DoAddTlbType()
        {
            TlbTypeSelectDialog dlg = new TlbTypeSelectDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string typeLibName = dlg.TypeLib.GetDocumentation();
                string typeName = dlg.TypeInfoMatchTarget.Name;
                string typeFullName = "[" + typeLibName + "]" + typeName;
                if (!comboBoxNativeType.Items.Contains(typeFullName))
                {
                    TlbType2String.AddTlbUserDefinedType(typeFullName);
                    comboBoxNativeType.Items.Insert(comboBoxNativeType.Items.Count - 1,
                        typeFullName);
                }
                comboBoxNativeType.SelectedItem = typeFullName;
            }
            else
            {
                comboBoxNativeType.SelectedIndex = -1;
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            DoAddTlbType();
        }
    }
}
