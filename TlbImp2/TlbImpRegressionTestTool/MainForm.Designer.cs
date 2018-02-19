namespace TlbImpRegressionTestTool
{
    partial class MainForm
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
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemOpenTestCaseFile = new System.Windows.Forms.ToolStripMenuItem();
            this.runToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRunAllTestCases = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRunSelectedTestCases = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemBaselineSelectedTestcases = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemStop = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.settingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridViewTestCases = new System.Windows.Forms.DataGridView();
            this.Selection = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Target = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.WorkingDirectory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Arguments = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Baseline = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.openTestCaseFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.menuStripMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTestCases)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStripMain
            // 
            this.menuStripMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.runToolStripMenuItem});
            this.menuStripMain.Location = new System.Drawing.Point(0, 0);
            this.menuStripMain.Name = "menuStripMain";
            this.menuStripMain.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStripMain.Size = new System.Drawing.Size(976, 28);
            this.menuStripMain.TabIndex = 0;
            this.menuStripMain.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemOpenTestCaseFile});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // toolStripMenuItemOpenTestCaseFile
            // 
            this.toolStripMenuItemOpenTestCaseFile.Name = "toolStripMenuItemOpenTestCaseFile";
            this.toolStripMenuItemOpenTestCaseFile.Size = new System.Drawing.Size(217, 26);
            this.toolStripMenuItemOpenTestCaseFile.Text = "Open TestCase File...";
            this.toolStripMenuItemOpenTestCaseFile.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // runToolStripMenuItem
            // 
            this.runToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemRunAllTestCases,
            this.toolStripMenuItemRunSelectedTestCases,
            this.toolStripMenuItemBaselineSelectedTestcases,
            this.toolStripSeparator3,
            this.toolStripMenuItemStop,
            this.toolStripSeparator2,
            this.settingToolStripMenuItem});
            this.runToolStripMenuItem.Name = "runToolStripMenuItem";
            this.runToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.runToolStripMenuItem.Text = "Run";
            // 
            // toolStripMenuItemRunAllTestCases
            // 
            this.toolStripMenuItemRunAllTestCases.Name = "toolStripMenuItemRunAllTestCases";
            this.toolStripMenuItemRunAllTestCases.Size = new System.Drawing.Size(267, 26);
            this.toolStripMenuItemRunAllTestCases.Text = "Run All TestCases";
            this.toolStripMenuItemRunAllTestCases.Click += new System.EventHandler(this.toolStripMenuItemRunAllTestCases_Click);
            // 
            // toolStripMenuItemRunSelectedTestCases
            // 
            this.toolStripMenuItemRunSelectedTestCases.Name = "toolStripMenuItemRunSelectedTestCases";
            this.toolStripMenuItemRunSelectedTestCases.Size = new System.Drawing.Size(267, 26);
            this.toolStripMenuItemRunSelectedTestCases.Text = "Run Selected TestCases";
            this.toolStripMenuItemRunSelectedTestCases.Click += new System.EventHandler(this.toolStripMenuItemRunSelectedTestCases_Click);
            // 
            // toolStripMenuItemBaselineSelectedTestcases
            // 
            this.toolStripMenuItemBaselineSelectedTestcases.Name = "toolStripMenuItemBaselineSelectedTestcases";
            this.toolStripMenuItemBaselineSelectedTestcases.Size = new System.Drawing.Size(267, 26);
            this.toolStripMenuItemBaselineSelectedTestcases.Text = "Baseline Selected TestCases";
            this.toolStripMenuItemBaselineSelectedTestcases.Click += new System.EventHandler(this.toolStripMenuItemBaselineSelectedTestcases_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(264, 6);
            // 
            // toolStripMenuItemStop
            // 
            this.toolStripMenuItemStop.Enabled = false;
            this.toolStripMenuItemStop.Name = "toolStripMenuItemStop";
            this.toolStripMenuItemStop.Size = new System.Drawing.Size(267, 26);
            this.toolStripMenuItemStop.Text = "Stop";
            this.toolStripMenuItemStop.Click += new System.EventHandler(this.toolStripMenuItemStop_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(264, 6);
            // 
            // settingToolStripMenuItem
            // 
            this.settingToolStripMenuItem.Name = "settingToolStripMenuItem";
            this.settingToolStripMenuItem.Size = new System.Drawing.Size(267, 26);
            this.settingToolStripMenuItem.Text = "Settings...";
            this.settingToolStripMenuItem.Click += new System.EventHandler(this.settingToolStripMenuItem_Click);
            // 
            // dataGridViewTestCases
            // 
            this.dataGridViewTestCases.AllowUserToAddRows = false;
            this.dataGridViewTestCases.AllowUserToDeleteRows = false;
            this.dataGridViewTestCases.AllowUserToOrderColumns = true;
            this.dataGridViewTestCases.AllowUserToResizeRows = false;
            this.dataGridViewTestCases.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTestCases.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Selection,
            this.Id,
            this.Target,
            this.WorkingDirectory,
            this.Arguments,
            this.Baseline,
            this.Status});
            this.dataGridViewTestCases.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewTestCases.Location = new System.Drawing.Point(0, 28);
            this.dataGridViewTestCases.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridViewTestCases.MultiSelect = false;
            this.dataGridViewTestCases.Name = "dataGridViewTestCases";
            this.dataGridViewTestCases.RowHeadersVisible = false;
            this.dataGridViewTestCases.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewTestCases.Size = new System.Drawing.Size(976, 548);
            this.dataGridViewTestCases.TabIndex = 1;
            this.dataGridViewTestCases.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGridViewTestCases_DataError);
            this.dataGridViewTestCases.DoubleClick += new System.EventHandler(this.dataGridViewTestCases_DoubleClick);
            // 
            // Selection
            // 
            this.Selection.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.Selection.HeaderText = "";
            this.Selection.Name = "Selection";
            this.Selection.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Selection.Width = 5;
            // 
            // Id
            // 
            this.Id.HeaderText = "Id";
            this.Id.Name = "Id";
            this.Id.ReadOnly = true;
            this.Id.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Id.Width = 30;
            // 
            // Target
            // 
            this.Target.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Target.HeaderText = "Target";
            this.Target.Name = "Target";
            this.Target.ReadOnly = true;
            this.Target.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // WorkingDirectory
            // 
            this.WorkingDirectory.HeaderText = "WorkingDirectory";
            this.WorkingDirectory.Name = "WorkingDirectory";
            this.WorkingDirectory.ReadOnly = true;
            this.WorkingDirectory.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.WorkingDirectory.Width = 150;
            // 
            // Arguments
            // 
            this.Arguments.HeaderText = "Arguments";
            this.Arguments.Name = "Arguments";
            this.Arguments.ReadOnly = true;
            this.Arguments.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Arguments.Width = 150;
            // 
            // Baseline
            // 
            this.Baseline.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Baseline.HeaderText = "Baseline";
            this.Baseline.Name = "Baseline";
            this.Baseline.ReadOnly = true;
            this.Baseline.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Status
            // 
            this.Status.HeaderText = "Status";
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            this.Status.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Status.Width = 60;
            // 
            // openTestCaseFileDialog
            // 
            this.openTestCaseFileDialog.FileName = "unknown";
            this.openTestCaseFileDialog.Filter = "Test Case files|*.xml|All files|*.*";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(976, 576);
            this.Controls.Add(this.dataGridViewTestCases);
            this.Controls.Add(this.menuStripMain);
            this.MainMenuStrip = this.menuStripMain;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "TlbImp Regression Test Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTestCases)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenTestCaseFile;
        private System.Windows.Forms.ToolStripMenuItem runToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRunAllTestCases;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRunSelectedTestCases;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem settingToolStripMenuItem;
        private System.Windows.Forms.DataGridView dataGridViewTestCases;
        private System.Windows.Forms.OpenFileDialog openTestCaseFileDialog;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStop;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Selection;
        private System.Windows.Forms.DataGridViewTextBoxColumn Id;
        private System.Windows.Forms.DataGridViewTextBoxColumn Target;
        private System.Windows.Forms.DataGridViewTextBoxColumn WorkingDirectory;
        private System.Windows.Forms.DataGridViewTextBoxColumn Arguments;
        private System.Windows.Forms.DataGridViewTextBoxColumn Baseline;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBaselineSelectedTestcases;
    }
}

