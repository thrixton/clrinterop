using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TypeLibTypes.Interop;
using System.Threading;

namespace TypeLibraryTreeView
{

    public enum DisplayLevel
    {
        All, TypeOnly
    }

    public class TlbTreeView : TreeView
    {
        private ImageList typeLibIconList;
        private System.ComponentModel.IContainer components;
        FormDaemon m_daemon;
        private DisplayLevel m_displayLevel = DisplayLevel.All;

        public void SetTypeLibrary(TypeLib typeLib)
        {
            TreeNode root = TypeLib2TreeNodeProcessor.GetTypeLibNode(typeLib, m_displayLevel);
            TypeLib2TreeNodeProcessor.SetTlbTreeNodeImage(root);
            this.BeginUpdate();
            this.Nodes.Clear();
            this.Nodes.Add(root);
            root.Expand();
            this.EndUpdate();
        }

        public TlbTreeView()
        {
            InitializeComponent();
            // Try to init TypeInteropManager.
            DaemonForm daemonForm = new DaemonForm();
            m_daemon = new FormDaemon(daemonForm);
            if (TypeLibResourceManager.InitTypeLibResourceManager(m_daemon))
            {
                // Init successfully, and start daemon thread.
                Thread daemonThread = new Thread(delegate()
                {
                    daemonForm.ShowDialog();
                });
                daemonThread.Start();
            }
            else
            {
                if (!(TypeLibResourceManager.GetDaemon() is FormDaemon))
                {
                    throw new NotFormDaemonException();
                }
                m_daemon = null;
            }
        }

        public DisplayLevel DisplayLevel
        {
            get
            {
                return m_displayLevel;
            }
            set
            {
                m_displayLevel = value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (m_daemon != null)
            {
                // Close DaemonForm
                DaemonForm form = m_daemon.DaemonForm;
                m_daemon.DaemonForm = null;
                form.Close();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TlbTreeView));
            this.typeLibIconList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // typeLibIconList
            // 
            this.typeLibIconList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("typeLibIconList.ImageStream")));
            this.typeLibIconList.TransparentColor = System.Drawing.Color.Fuchsia;
            this.typeLibIconList.Images.SetKeyName(0, "CoClass");
            this.typeLibIconList.Images.SetKeyName(1, "Enum");
            this.typeLibIconList.Images.SetKeyName(2, "Field");
            this.typeLibIconList.Images.SetKeyName(3, "Interface");
            this.typeLibIconList.Images.SetKeyName(4, "Lib");
            this.typeLibIconList.Images.SetKeyName(5, "Function");
            this.typeLibIconList.Images.SetKeyName(6, "Module");
            this.typeLibIconList.Images.SetKeyName(7, "Struct");
            this.typeLibIconList.Images.SetKeyName(8, "Union");
            this.typeLibIconList.Images.SetKeyName(9, "DispatchInterface");
            this.typeLibIconList.Images.SetKeyName(10, "Alias");
            this.typeLibIconList.Images.SetKeyName(11, "Signature");
            // 
            // TlbTreeView
            // 
            this.ImageIndex = 0;
            this.ImageList = this.typeLibIconList;
            this.LineColor = System.Drawing.Color.Black;
            this.SelectedImageIndex = 0;
            this.ResumeLayout(false);

        }

        public bool IsDefaultNodeSelected()
        {
            return (this.SelectedNode != null &&
                this.SelectedNode.Tag.Equals(TypeLib2TreeNodeProcessor.DEFAULT_TREE_ROOT_TAG));
        }

    }
}
