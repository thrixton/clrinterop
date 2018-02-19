using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TlbImpRuleEngine;

namespace TlbImpRuleFileEditor
{
    public partial class ChangeManagedNameActionDialog : Form
    {
        private ChangeManagedNameAction m_fixedChangeManagedNameAction;

        private string m_newName;

        public string NewName
        {
            get
            {
                return m_newName;
            }
        }

        
        public ChangeManagedNameActionDialog(ChangeManagedNameAction fixedChangeManagedNameAction)
        {
            InitializeComponent();
            if (fixedChangeManagedNameAction != null)
            {
                m_fixedChangeManagedNameAction = fixedChangeManagedNameAction;
                m_newName = m_fixedChangeManagedNameAction.NewName;
            }
        }

        public ChangeManagedNameActionDialog()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Dispose();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            m_newName = textBoxNewName.Text;
            DialogResult = DialogResult.OK;
            this.Dispose();
        }

        private void ChangeManagedNameActionDialog_Load(object sender, EventArgs e)
        {
            if (m_newName != null)
                textBoxNewName.Text = m_newName;
        }

        private void textBoxNewName_TextChanged(object sender, EventArgs e)
        {
            buttonOK.Enabled = !textBoxNewName.Text.Equals("");
        }
    }
}
