namespace AssemblyTreeViewer
{
    partial class AssemblyTVForm
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
            this.treeViewAssembly = new AssemblyTreeView(this.components);
            this.SuspendLayout();
            // 
            // treeViewAssembly
            // 
            this.treeViewAssembly.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewAssembly.Location = new System.Drawing.Point(0, 0);
            this.treeViewAssembly.Name = "treeViewAssembly";
            this.treeViewAssembly.Size = new System.Drawing.Size(316, 410);
            this.treeViewAssembly.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 410);
            this.Controls.Add(this.treeViewAssembly);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private AssemblyTreeView treeViewAssembly;
    }
}

