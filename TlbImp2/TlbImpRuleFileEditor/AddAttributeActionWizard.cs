using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using TlbImpRuleEngine;
using System.Globalization;
using System.Xml;

namespace TlbImpRuleFileEditor
{
    public partial class AddAttributeActionWizard : Form
    {
        private AddAttributeAction m_fixedAddAttributeAction;

        string m_assembly;

        string m_selectedAttribute;

        private Type m_selectedAttributeType;

        string m_ctor;

        string m_value;

        public string SelectedAssembly
        {
            get
            {
                return m_assembly;
            }
        }

        public string SelectedAttribute
        {
            get
            {
                return m_selectedAttribute;
            }
        }

        public string Constructor
        {
            get
            {
                return m_ctor;
            }
        }

        public string AttributeValue
        {
            get
            {
                return m_value;
            }
        }

        private string GetFormatedBytes(string bytesString)
        {
            StringBuilder sb = new StringBuilder();
            string oneByte = "";
            for (int i = 0; i < bytesString.Length; i++)
            {
                if (Char.IsLetterOrDigit(bytesString[i]))
                {
                    oneByte = oneByte + bytesString[i];
                    if (oneByte.Length == 2)
                    {
                        if (sb.Length != 0)
                            sb.Append(' ');
                        byte b = Byte.Parse(oneByte, NumberStyles.AllowHexSpecifier,
                            CultureInfo.InvariantCulture);
                        sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));
                        oneByte = "";
                    }
                }
            }
            if (oneByte.Length == 1)
            {
                if (sb.Length != 0)
                    sb.Append(' ');
                sb.Append(Byte.Parse(oneByte, NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture).ToString("X2", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        public AddAttributeActionWizard()
        {
            InitializeComponent();
        }

        public AddAttributeActionWizard(AddAttributeAction fixedAddAttributeAction)
        {
            InitializeComponent();
            if (fixedAddAttributeAction != null)
            {
                m_fixedAddAttributeAction = fixedAddAttributeAction;

                m_assembly = m_fixedAddAttributeAction.AssemblyName;
                m_selectedAttribute = m_fixedAddAttributeAction.TypeName;
                m_ctor = m_fixedAddAttributeAction.Constructor;
                m_value = m_fixedAddAttributeAction.Data;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Dispose();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                this.textBoxByteValue.Text = GetFormatedBytes(this.textBoxByteValue.Text);
            }
            catch (Exception)
            {
                MessageBox.Show(Resource.FormatString("Wrn_InvalidByteData"));
                this.textBoxByteValue.Focus();
                return;
            }
            m_assembly = this.textBoxAssembly.Text;
            m_selectedAttribute = this.textBoxType.Text;
            m_ctor = this.listBoxCtor.SelectedItem.ToString();
            m_value = this.textBoxByteValue.Text;
            this.DialogResult = DialogResult.OK;
            this.Dispose();
        }

        private void buttonTypeSelector_Click(object sender, EventArgs e)
        {
            AssemblyTypeSelectDialog dlg = new AssemblyTypeSelectDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.m_selectedAttributeType = dlg.SelectedType;
                this.textBoxAssembly.Text = dlg.SelectedType.Assembly.GetName().Name;
                this.textBoxType.Text = dlg.SelectedType.FullName;
                UpdateConstructors();
            }
        }

        private void UpdateConstructors()
        {
            this.listBoxCtor.Items.Clear();

            if (this.m_selectedAttributeType != null)
            {
                ConstructorInfo[] ctors = this.m_selectedAttributeType.GetConstructors();
                foreach (ConstructorInfo ctor in ctors)
                {
                    this.listBoxCtor.Items.Add(ctor.ToString());
                }
            }
        }

        private void AddAttributeActionWizard_Load(object sender, EventArgs e)
        {
            if (m_assembly != null && m_selectedAttribute != null)
            {
                this.textBoxAssembly.Text = m_assembly;
                this.textBoxType.Text = m_selectedAttribute;
                try
                {
                    Assembly assembly = Assembly.Load(m_assembly);
                    this.m_selectedAttributeType = assembly.GetType(m_selectedAttribute);
                }
                catch (Exception)
                {
                    MessageBox.Show(Resource.FormatString("Wrn_ManagedTypeCannotBeLoaded",
                        this.m_selectedAttribute, this.m_assembly));
                }
                if (this.m_selectedAttributeType != null)
                {
                    UpdateConstructors();
                    listBoxCtor.SelectedItem = this.m_ctor;
                }
                else
                {
                    listBoxCtor.Items.Clear();
                    listBoxCtor.Items.Add(this.m_ctor);
                    listBoxCtor.SelectedItem = this.m_ctor;
                }
            }
            if (m_value != null)
            {
                textBoxByteValue.Text = m_value;
            }
        }

        private void buttonDataInput_Click(object sender, EventArgs e)
        {
            if (m_selectedAttributeType == null)
            {
                MessageBox.Show(Resource.FormatString("Wrn_CannotUseByteDataInputHelper_TypeNotLoaded"));
                return;
            }
            if (listBoxCtor.SelectedItem == null)
            {
                MessageBox.Show(Resource.FormatString("Wrn_CannotUseByteDataInputHelper_CtorNotSelected"));
                return;
            }
            ConstructorInfo ctor = GetConstructor(m_selectedAttributeType.GetConstructors(),
                listBoxCtor.SelectedItem.ToString());
            if (ctor == null)
            {
                MessageBox.Show(Resource.FormatString("Wrn_CannotUseByteDataInputHelper_CtorNotFound",
                    listBoxCtor.SelectedItem.ToString(), m_selectedAttributeType.FullName));
                return;
            }
            foreach (ParameterInfo param in ctor.GetParameters())
            {
                if (!SupportParameterType(param.ParameterType))
                {
                    MessageBox.Show(Resource.FormatString("Wrn_CannotUseByteDataInputHelper_ParameterNotSupported",
                        param.ParameterType.Name));
                    return;
                }
            }
            try
            {
                this.textBoxByteValue.Text = GetFormatedBytes(this.textBoxByteValue.Text);
            }
            catch (Exception)
            {
                MessageBox.Show(Resource.FormatString("Wrn_InvalidByteData"));
                this.textBoxByteValue.Focus();
                return;
            }
            AddAttributeDataInputHelper dlg = new AddAttributeDataInputHelper(m_selectedAttributeType,
                ctor, textBoxByteValue.Text);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxByteValue.Text = dlg.GetEncodingData();
            }
        }

        private static bool SupportParameterType(Type type)
        {
            return type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64) ||
                type == typeof(UInt16) || type == typeof(UInt32) || type == typeof(UInt64) ||
                type == typeof(Boolean) || type == typeof(String) || type == typeof(Type) ||
                type.IsEnum;
        }

        private ConstructorInfo GetConstructor(ConstructorInfo[] constructorInfos, string ctorToken)
        {
            foreach (ConstructorInfo ctor in constructorInfos)
            {
                if (ctor.ToString().Equals(ctorToken))
                {
                    return ctor;
                }
            }
            return null;
        }

        private void textBoxAssembly_TextChanged(object sender, EventArgs e)
        {
            CheckOKButtonEnabled();
        }

        private void CheckOKButtonEnabled()
        {
            if (this.textBoxAssembly.Text.Equals("") || this.textBoxType.Text.Equals("") ||
                this.listBoxCtor.SelectedItem == null ||
                this.textBoxByteValue.Text.Trim().Equals(""))
            {
                buttonOK.Enabled = false;
            }
            else
            {
                buttonOK.Enabled = true;
            }
        }

        private void textBoxType_TextChanged(object sender, EventArgs e)
        {
            CheckOKButtonEnabled();
        }

        private void listBoxCtor_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckOKButtonEnabled();
        }

        private void textBoxByteValue_TextChanged(object sender, EventArgs e)
        {
            CheckOKButtonEnabled();
        }
    }
}
