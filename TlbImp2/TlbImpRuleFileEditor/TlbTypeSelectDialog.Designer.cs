using TypeLibraryTreeView;
namespace TlbImpRuleFileEditor
{
    partial class TlbTypeSelectDialog
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("<...>");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TlbTypeSelectDialog));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxFilePath = new System.Windows.Forms.TextBox();
            this.buttonOpenTlbFile = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.openTlbFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.treeViewTypeLib = new TypeLibraryTreeView.TlbTreeView();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxFilePath);
            this.groupBox1.Controls.Add(this.buttonOpenTlbFile);
            this.groupBox1.Location = new System.Drawing.Point(14, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(331, 43);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Select TLB File";
            // 
            // textBoxFilePath
            // 
            this.textBoxFilePath.Location = new System.Drawing.Point(7, 16);
            this.textBoxFilePath.Name = "textBoxFilePath";
            this.textBoxFilePath.ReadOnly = true;
            this.textBoxFilePath.Size = new System.Drawing.Size(272, 21);
            this.textBoxFilePath.TabIndex = 1;
            // 
            // buttonOpenTlbFile
            // 
            this.buttonOpenTlbFile.Location = new System.Drawing.Point(287, 15);
            this.buttonOpenTlbFile.Name = "buttonOpenTlbFile";
            this.buttonOpenTlbFile.Size = new System.Drawing.Size(37, 20);
            this.buttonOpenTlbFile.TabIndex = 0;
            this.buttonOpenTlbFile.Text = "...";
            this.buttonOpenTlbFile.UseVisualStyleBackColor = true;
            this.buttonOpenTlbFile.Click += new System.EventHandler(this.buttonOpenTlbFile_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(178, 302);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(87, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(68, 302);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(87, 23);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // openTlbFileDialog
            // 
            this.openTlbFileDialog.FileName = "unknown";
            this.openTlbFileDialog.Filter = "Type library files|*.tlb|All files|*.*";
            this.openTlbFileDialog.InitialDirectory = "D:\\cmdline";
            // 
            // treeViewTypeLib
            // 
            this.treeViewTypeLib.DisplayLevel = TypeLibraryTreeView.DisplayLevel.TypeOnly;
            this.treeViewTypeLib.ImageIndex = 0;
            this.treeViewTypeLib.Location = new System.Drawing.Point(14, 61);
            this.treeViewTypeLib.Name = "treeViewTypeLib";
            treeNode1.ImageKey = "Lib";
            treeNode1.Name = "";
            treeNode1.SelectedImageKey = "Lib";
            treeNode1.StateImageKey = "Lib";
            treeNode1.Tag = "<...>";
            treeNode1.Text = "<...>";
            this.treeViewTypeLib.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeViewTypeLib.SelectedImageIndex = 0;
            this.treeViewTypeLib.Size = new System.Drawing.Size(332, 235);
            this.treeViewTypeLib.TabIndex = 0;
            this.treeViewTypeLib.DoubleClick += new System.EventHandler(this.treeViewTypeLib_DoubleClick);
            // 
            // TlbTypeSelectDialog
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(360, 337);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.treeViewTypeLib);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TlbTypeSelectDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TLB Type Selector";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TlbTreeView treeViewTypeLib;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonOpenTlbFile;
        private System.Windows.Forms.TextBox textBoxFilePath;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.OpenFileDialog openTlbFileDialog;
    }
}