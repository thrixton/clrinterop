using TypeLibraryTreeView;
namespace TlbImpRuleFileEditor
{
    partial class AssemblyTypeSelectDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssemblyTypeSelectDialog));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.openAssemblyFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.treeViewAssembly = new AssemblyTreeViewer.AssemblyTreeView(this.components);
            this.textBoxAssemblyName = new System.Windows.Forms.TextBox();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.textBoxFilePath = new System.Windows.Forms.TextBox();
            this.buttonOpenAssemblyFile = new System.Windows.Forms.Button();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButtonAssemblyFile = new System.Windows.Forms.RadioButton();
            this.radioButtonAssemblyName = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(227, 379);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(87, 23);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(108, 379);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(87, 23);
            this.buttonOK.TabIndex = 11;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // openAssemblyFileDialog
            // 
            this.openAssemblyFileDialog.Filter = "Assembly files|*.dll;*.exe|All files|*.*";
            // 
            // treeViewAssembly
            // 
            this.treeViewAssembly.HideSelection = false;
            this.treeViewAssembly.ImageIndex = 0;
            this.treeViewAssembly.Location = new System.Drawing.Point(12, 45);
            this.treeViewAssembly.Name = "treeViewAssembly";
            this.treeViewAssembly.SelectedImageIndex = 0;
            this.treeViewAssembly.Size = new System.Drawing.Size(363, 221);
            this.treeViewAssembly.TabIndex = 10;
            this.treeViewAssembly.DoubleClick += new System.EventHandler(this.treeViewAssembly_DoubleClick);
            this.treeViewAssembly.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewAssembly_AfterSelect);
            // 
            // textBoxAssemblyName
            // 
            this.textBoxAssemblyName.Location = new System.Drawing.Point(129, 18);
            this.textBoxAssemblyName.Name = "textBoxAssemblyName";
            this.textBoxAssemblyName.Size = new System.Drawing.Size(184, 21);
            this.textBoxAssemblyName.TabIndex = 4;
            this.textBoxAssemblyName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxAssemblyName_KeyPress);
            // 
            // buttonLoad
            // 
            this.buttonLoad.Location = new System.Drawing.Point(319, 19);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(54, 20);
            this.buttonLoad.TabIndex = 5;
            this.buttonLoad.Text = "Load";
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // textBoxFilePath
            // 
            this.textBoxFilePath.Enabled = false;
            this.textBoxFilePath.Location = new System.Drawing.Point(129, 50);
            this.textBoxFilePath.Name = "textBoxFilePath";
            this.textBoxFilePath.ReadOnly = true;
            this.textBoxFilePath.Size = new System.Drawing.Size(184, 21);
            this.textBoxFilePath.TabIndex = 7;
            // 
            // buttonOpenAssemblyFile
            // 
            this.buttonOpenAssemblyFile.Enabled = false;
            this.buttonOpenAssemblyFile.Location = new System.Drawing.Point(319, 51);
            this.buttonOpenAssemblyFile.Name = "buttonOpenAssemblyFile";
            this.buttonOpenAssemblyFile.Size = new System.Drawing.Size(54, 20);
            this.buttonOpenAssemblyFile.TabIndex = 8;
            this.buttonOpenAssemblyFile.Text = "...";
            this.buttonOpenAssemblyFile.UseVisualStyleBackColor = true;
            this.buttonOpenAssemblyFile.Click += new System.EventHandler(this.buttonOpenAssemblyFile_Click);
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Location = new System.Drawing.Point(12, 18);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(363, 21);
            this.textBoxFilter.TabIndex = 9;
            this.textBoxFilter.TextChanged += new System.EventHandler(this.textBoxFilter_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxFilter);
            this.groupBox1.Controls.Add(this.treeViewAssembly);
            this.groupBox1.Location = new System.Drawing.Point(14, 101);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(387, 272);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filter";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioButtonAssemblyFile);
            this.groupBox2.Controls.Add(this.radioButtonAssemblyName);
            this.groupBox2.Controls.Add(this.textBoxFilePath);
            this.groupBox2.Controls.Add(this.buttonOpenAssemblyFile);
            this.groupBox2.Controls.Add(this.textBoxAssemblyName);
            this.groupBox2.Controls.Add(this.buttonLoad);
            this.groupBox2.Location = new System.Drawing.Point(14, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(387, 83);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Assembly";
            // 
            // radioButtonAssemblyFile
            // 
            this.radioButtonAssemblyFile.AutoSize = true;
            this.radioButtonAssemblyFile.Location = new System.Drawing.Point(6, 51);
            this.radioButtonAssemblyFile.Name = "radioButtonAssemblyFile";
            this.radioButtonAssemblyFile.Size = new System.Drawing.Size(103, 17);
            this.radioButtonAssemblyFile.TabIndex = 6;
            this.radioButtonAssemblyFile.TabStop = true;
            this.radioButtonAssemblyFile.Text = "Assembly File";
            this.radioButtonAssemblyFile.UseVisualStyleBackColor = true;
            this.radioButtonAssemblyFile.CheckedChanged += new System.EventHandler(this.radioButtonAssemblyFile_CheckedChanged);
            // 
            // radioButtonAssemblyName
            // 
            this.radioButtonAssemblyName.AutoSize = true;
            this.radioButtonAssemblyName.Checked = true;
            this.radioButtonAssemblyName.Location = new System.Drawing.Point(6, 22);
            this.radioButtonAssemblyName.Name = "radioButtonAssemblyName";
            this.radioButtonAssemblyName.Size = new System.Drawing.Size(117, 17);
            this.radioButtonAssemblyName.TabIndex = 3;
            this.radioButtonAssemblyName.TabStop = true;
            this.radioButtonAssemblyName.Text = "Assembly Name";
            this.radioButtonAssemblyName.UseVisualStyleBackColor = true;
            this.radioButtonAssemblyName.CheckedChanged += new System.EventHandler(this.radioButtonAssemblyName_CheckedChanged);
            // 
            // AssemblyTypeSelectDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(415, 424);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AssemblyTypeSelectDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Managed Type Selector";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.OpenFileDialog openAssemblyFileDialog;
        private AssemblyTreeViewer.AssemblyTreeView treeViewAssembly;
        private System.Windows.Forms.TextBox textBoxAssemblyName;
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.TextBox textBoxFilePath;
        private System.Windows.Forms.Button buttonOpenAssemblyFile;
        private System.Windows.Forms.TextBox textBoxFilter;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButtonAssemblyFile;
        private System.Windows.Forms.RadioButton radioButtonAssemblyName;
    }
}