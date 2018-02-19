using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TypeLibTypes.Interop;
using System.Runtime.InteropServices;
using System.Threading;

namespace TypeLibraryTreeView
{
    public partial class TypeLibraryViewer : Form
    {
        public TypeLibraryViewer()
        {
            InitializeComponent();
        }

        //******************************************************************************
        // Enum passed in to LoadTypeLibEx.
        //******************************************************************************
        [Flags]
        public enum REGKIND
        {
            REGKIND_DEFAULT = 0,
            REGKIND_REGISTER = 1,
            REGKIND_NONE = 2,
            REGKIND_LOAD_TLB_AS_32BIT = 0x20,
            REGKIND_LOAD_TLB_AS_64BIT = 0x40,
        }

        public class APIHelper
        {
            [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
            public static extern void LoadTypeLibEx(String strTypeLibName,
                REGKIND regKind, out System.Runtime.InteropServices.ComTypes.ITypeLib TypeLib);
        }

        private void openTypeLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openTypeLibraryOperation();
        }

        private void openTypeLibraryOperation()
        {
            if (openTlbFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadTlb(openTlbFileDialog.FileName);
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
                TypeLib tlb = new TypeLib((ITypeLib)TypeLib);
                this.tlbTreeView.SetTypeLibrary(tlb);
            }
            catch (Exception)
            {
                MessageBox.Show("Err_TypeLibLoad");
            }
        }

        private void TypeLibraryViewer_Load(object sender, EventArgs e)
        {
            TreeNode root = TypeLib2TreeNodeProcessor.GetDefaultLibNode();
            root.Text = "<double click to open a .tlb file…>";
            TypeLib2TreeNodeProcessor.SetTlbTreeNodeImage(root);
            tlbTreeView.BeginUpdate();
            tlbTreeView.Nodes.Clear();
            tlbTreeView.Nodes.Add(root);
            tlbTreeView.EndUpdate();
        }

        private void tlbTreeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (tlbTreeView.IsDefaultNodeSelected())
                openTypeLibraryOperation();
        }
    }
}
