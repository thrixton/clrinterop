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
    public partial class ResolveToActionWizard : Form
    {
        private string m_managedAssemblyName;

        private string m_managedTypeFullName;

        private ResolveToAction m_fixedResolveToAction;

        public ResolveToActionWizard()
        {
            InitializeComponent();
        }

        public ResolveToActionWizard(ResolveToAction fixedResolveToAction)
        {
            InitializeComponent();
            if (fixedResolveToAction != null)
            {
                m_fixedResolveToAction = fixedResolveToAction;
            }
        }

        private void buttonManagedTypeSelector_Click(object sender, EventArgs e)
        {
            AssemblyTypeSelectDialog dlg = new AssemblyTypeSelectDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.textBoxMangedAssembly.Text = dlg.SelectedType.Assembly.GetName().Name;
                this.textBoxManagedTypeFullName.Text = dlg.SelectedType.FullName;
            }
        }

        public string ManagedAssemblyName
        {
            get
            {
                return m_managedAssemblyName;
            }
        }

        public string ManagedTypeFullName
        {
            get
            {
                return m_managedTypeFullName;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            m_managedAssemblyName = this.textBoxMangedAssembly.Text.Trim();
            m_managedTypeFullName = this.textBoxManagedTypeFullName.Text.Trim();
            DialogResult = DialogResult.OK;
            Dispose();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Dispose();
        }

        private void ResolveToActionWizard_Load(object sender, EventArgs e)
        {
            if (m_fixedResolveToAction != null)
            {
                this.textBoxMangedAssembly.Text = m_fixedResolveToAction.AssemblyName;
                this.textBoxManagedTypeFullName.Text = m_fixedResolveToAction.ManagedTypeFullName;
            }
        }

        private void textBoxMangedAssembly_TextChanged(object sender, EventArgs e)
        {
            CheckOKButtonEnabled();
        }

        private void CheckOKButtonEnabled()
        {
            if (this.textBoxMangedAssembly.Text.Trim().Equals("") ||
                this.textBoxManagedTypeFullName.Text.Trim().Equals(""))
            {
                buttonOK.Enabled = false;
            }
            else
            {
                buttonOK.Enabled = true;
            }
        }

        private void textBoxManagedTypeFullName_TextChanged(object sender, EventArgs e)
        {
            CheckOKButtonEnabled();
        }
    }
}
