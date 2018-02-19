using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TypeLibTypes.Interop;
using TlbImpRuleEngine;

namespace TlbImpRuleFileEditor
{
    public partial class TlbTypeSelectDialog : Form
    {
        private TypeLib m_typeLib = null;

        private TypeInfoMatchTarget m_typeTarget = null;

        public TlbTypeSelectDialog()
        {
            InitializeComponent();
        }

        public TypeInfoMatchTarget TypeInfoMatchTarget
        {
            get
            {
                return m_typeTarget;
            }
        }

        public TypeLib TypeLib
        {
            get
            {
                return m_typeLib;
            }
        }

        private void buttonOpenTlbFile_Click(object sender, EventArgs e)
        {
            if (openTlbFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.Cursor = Cursors.WaitCursor;
                this.textBoxFilePath.Text = openTlbFileDialog.FileName;
                LoadTlb(openTlbFileDialog.FileName);
                this.Cursor = Cursors.Default;
            }
        }

        private void LoadTlb(string tlbFileName)
        {
            try
            {
                // Load the typelib.
                System.Runtime.InteropServices.ComTypes.ITypeLib TypeLib = null;
                APIHelper.LoadTypeLibEx(tlbFileName, REGKIND.REGKIND_DEFAULT, out TypeLib);

                // Update the tlbTreeView.
                m_typeLib = new TypeLib((ITypeLib)TypeLib);
                treeViewTypeLib.SetTypeLibrary(m_typeLib);
            }
            catch (Exception)
            {
                MessageBox.Show(Resource.FormatString("Wrn_TypeLibLoadFailed", tlbFileName));
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DoSelect();
        }

        private void DoSelect()
        {
            if (treeViewTypeLib.SelectedNode != null &&
                treeViewTypeLib.SelectedNode.Tag is TypeInfoMatchTarget)
            {
                m_typeTarget = treeViewTypeLib.SelectedNode.Tag as TypeInfoMatchTarget;
                DialogResult = DialogResult.OK;
                Dispose();
            }
            else
            {
                MessageBox.Show(Resource.FormatString("Wrn_NoTlbTypeSelected_TlbTypeSelector"));
                return;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Dispose();
        }

        private void treeViewTypeLib_DoubleClick(object sender, EventArgs e)
        {
            if (treeViewTypeLib.IsDefaultNodeSelected())
                buttonOpenTlbFile.PerformClick();
            else {
                DoSelect();
            }
        }
    }
}
