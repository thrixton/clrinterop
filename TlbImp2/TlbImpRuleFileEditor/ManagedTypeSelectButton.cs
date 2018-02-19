using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace TlbImpRuleFileEditor
{
    public partial class ManagedTypeSelectButton : Button
    {
        private TextBox m_textBox;

        public ManagedTypeSelectButton(TextBox textBox)
        {
            InitializeComponent();
            
            m_textBox = textBox;
        }

        private void ManagedTypeSelectButton_Click(object sender, EventArgs e)
        {
            AssemblyTypeSelectDialog dlg = new AssemblyTypeSelectDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                m_textBox.Text = dlg.SelectedType.FullName;
            }
        }
    }
}
