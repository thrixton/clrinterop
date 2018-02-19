using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TypeLibTypes.Interop;
using System.Reflection;

namespace TlbImpRuleFileEditor
{
    public partial class AssemblyTypeSelectDialog : Form
    {
        private Type m_selectedType;

        public AssemblyTypeSelectDialog()
        {
            InitializeComponent();
        }

        public Type SelectedType
        {
            get
            {
                return m_selectedType;
            }
        }

        private void LoadAssemblyByName(string assemblyName)
        {
            try
            {
                Assembly assembly = Assembly.Load(assemblyName);
                this.treeViewAssembly.SetAssembly(assembly);
            }
            catch (Exception)
            {
                MessageBox.Show(Resource.FormatString("Wrn_AssemblyLoadFailed", assemblyName));
            }
        }

        private void LoadAssemblyByFile(string filePath)
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(filePath);
                this.treeViewAssembly.SetAssembly(assembly);
            }
            catch (Exception)
            {
                MessageBox.Show(Resource.FormatString("Wrn_AssemblyLoadFailed", filePath));
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DoSelect();
        }

        private void DoSelect()
        {
            if (treeViewAssembly.SelectedNode != null &&
                treeViewAssembly.SelectedNode.Tag is Type)
            {
                m_selectedType = treeViewAssembly.SelectedNode.Tag as Type;
                DialogResult = DialogResult.OK;
                Dispose();
            }
            else
            {
                MessageBox.Show(Resource.FormatString("Wrn_NoManagedTypeSelected_TypeSelector"));
                return;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Dispose();
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            LoadAssemblyByName(this.textBoxAssemblyName.Text);
            textBoxFilter.Clear();
            buttonOK.Enabled = false;
        }

        private void buttonOpenAssemblyFile_Click(object sender, EventArgs e)
        {
            if (openAssemblyFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxFilePath.Text = openAssemblyFileDialog.FileName;
                LoadAssemblyByFile(openAssemblyFileDialog.FileName);
                textBoxFilter.Clear();
                buttonOK.Enabled = false;
            }
        }

        private void treeViewAssembly_DoubleClick(object sender, EventArgs e)
        {
            DoSelect();
        }

        private void textBoxFilter_TextChanged(object sender, EventArgs e)
        {
            this.treeViewAssembly.SetFilter(textBoxFilter.Text);
        }

        private void textBoxAssemblyName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter)
                this.buttonLoad.PerformClick();
        }

        private void radioButtonAssemblyName_CheckedChanged(object sender, EventArgs e)
        {
            textBoxAssemblyName.Enabled = radioButtonAssemblyName.Checked;
            buttonLoad.Enabled = radioButtonAssemblyName.Checked;
        }

        private void radioButtonAssemblyFile_CheckedChanged(object sender, EventArgs e)
        {
            textBoxFilePath.Enabled = radioButtonAssemblyFile.Checked;
            buttonOpenAssemblyFile.Enabled = radioButtonAssemblyFile.Checked;
        }

        private void treeViewAssembly_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeViewAssembly.SelectedNode != null &&
                treeViewAssembly.SelectedNode.Tag is Type)
            {
                buttonOK.Enabled = true;
            }
            else
            {
                buttonOK.Enabled = false;
            }
        }

        private void treeViewAssembly_Leave(object sender, EventArgs e)
        {
            buttonOK.Enabled = false;
        }

    }
}
