namespace AssemblyTreeViewer
{
    partial class AssemblyTreeView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssemblyTreeView));
            this.assemblyTypeIconList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // assemblyTypeIconList
            // 
            this.assemblyTypeIconList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("assemblyTypeIconList.ImageStream")));
            this.assemblyTypeIconList.TransparentColor = System.Drawing.Color.Fuchsia;
            this.assemblyTypeIconList.Images.SetKeyName(0, "Type");
            this.assemblyTypeIconList.Images.SetKeyName(1, "Assembly");
            // 
            // AssemblyTreeView
            // 
            this.ImageIndex = 0;
            this.ImageList = this.assemblyTypeIconList;
            this.LineColor = System.Drawing.Color.Black;
            this.SelectedImageIndex = 0;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList assemblyTypeIconList;

    }
}
