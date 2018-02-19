namespace TypeLibraryTreeView
{
    partial class TypeLibraryViewer
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openTypeLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openTlbFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.tlbTreeView = new TypeLibraryTreeView.TlbTreeView();
            this.menuStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStripMain
            // 
            this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStripMain.Location = new System.Drawing.Point(0, 0);
            this.menuStripMain.Name = "menuStripMain";
            this.menuStripMain.Size = new System.Drawing.Size(395, 24);
            this.menuStripMain.TabIndex = 1;
            this.menuStripMain.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openTypeLibraryToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openTypeLibraryToolStripMenuItem
            // 
            this.openTypeLibraryToolStripMenuItem.Name = "openTypeLibraryToolStripMenuItem";
            this.openTypeLibraryToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.openTypeLibraryToolStripMenuItem.Text = "Load type library...";
            this.openTypeLibraryToolStripMenuItem.Click += new System.EventHandler(this.openTypeLibraryToolStripMenuItem_Click);
            // 
            // openTlbFileDialog
            // 
            this.openTlbFileDialog.FileName = "unknown";
            this.openTlbFileDialog.Filter = "Type library files|*.tlb|All files|*.*";
            this.openTlbFileDialog.InitialDirectory = "D:\\cmdline";
            // 
            // tlbTreeView
            // 
            this.tlbTreeView.DisplayLevel = TypeLibraryTreeView.DisplayLevel.All;
            this.tlbTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlbTreeView.ImageIndex = 0;
            this.tlbTreeView.Location = new System.Drawing.Point(0, 24);
            this.tlbTreeView.Name = "tlbTreeView";
            this.tlbTreeView.SelectedImageIndex = 0;
            this.tlbTreeView.Size = new System.Drawing.Size(395, 400);
            this.tlbTreeView.TabIndex = 0;
            this.tlbTreeView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tlbTreeView_MouseDoubleClick);
            // 
            // TypeLibraryViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(395, 424);
            this.Controls.Add(this.tlbTreeView);
            this.Controls.Add(this.menuStripMain);
            this.MainMenuStrip = this.menuStripMain;
            this.Name = "TypeLibraryViewer";
            this.Text = "Type Library Viewer";
            this.Load += new System.EventHandler(this.TypeLibraryViewer_Load);
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TlbTreeView tlbTreeView;
        private System.Windows.Forms.MenuStrip menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openTypeLibraryToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openTlbFileDialog;
    }
}