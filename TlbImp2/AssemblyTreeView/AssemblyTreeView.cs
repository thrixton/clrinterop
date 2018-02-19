using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Globalization;

namespace AssemblyTreeViewer
{
    public partial class AssemblyTreeView : TreeView
    {
        Assembly m_assembly;

        Type[] m_types;

        string m_filter;

        const string IMAGE_KEY_ASSEMBLY = "Assembly";

        const string IMAGE_KEY_TYPE = "Type";

        public AssemblyTreeView()
        {
            InitializeComponent();
        }

        public AssemblyTreeView(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public void SetFilter(string filter)
        {
            m_filter = filter;
            UpdateTree();
        }

        public void SetAssembly(Assembly assembly)
        {
            m_assembly = assembly;
            m_types = m_assembly.GetTypes();
            Array.Sort(m_types, new TypeComparer());
            UpdateTree();
        }

        private void UpdateTree()
        {
            this.BeginUpdate();
            this.Nodes.Clear();
            if (m_types != null)
            {
                TreeNode root = new TreeNode(m_assembly.FullName);
                root.Tag = m_assembly;
                SetTreeNodeImage(root);
                foreach (Type type in m_types)
                {
                    if (IsPassFilter(type))
                    {
                        TreeNode typeNode = new TreeNode(type.FullName);
                        typeNode.Tag = type;
                        SetTreeNodeImage(typeNode);
                        root.Nodes.Add(typeNode);
                    }
                }
                this.Nodes.Add(root);
                root.Expand();
            }
            this.EndUpdate();
        }

        private bool IsPassFilter(Type type)
        {
            if (m_filter != null && !m_filter.Trim().Equals(""))
            {
                return type.FullName.ToUpper(CultureInfo.InvariantCulture).Contains(
                    m_filter.ToUpper(CultureInfo.InvariantCulture));
            }
            else
            {
                return true;
            }
        }

        private static void SetTreeNodeImage(TreeNode treeNode)
        {
            treeNode.ImageKey = GetTreeNodeImageKey(treeNode);
            treeNode.SelectedImageKey = treeNode.ImageKey;
            treeNode.StateImageKey = treeNode.ImageKey;
        }

        private static string GetTreeNodeImageKey(TreeNode treeNode)
        {
            object tag = treeNode.Tag;
            if (tag is Assembly)
            {
                return IMAGE_KEY_ASSEMBLY;
            }
            else if (tag is Type)
            {
                return IMAGE_KEY_TYPE;
            }
            return null;
        }
    }

    public class TypeComparer : IComparer<Type>
    {
        #region IComparer<Type> Members

        public int Compare(Type x, Type y)
        {
            return String.Compare(x.FullName, y.FullName,
                StringComparison.Ordinal);
        }

        #endregion
    }
}
