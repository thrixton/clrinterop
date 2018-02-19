namespace TlbImpRuleFileEditor
{
    partial class ParameterIndexNumericUpDown
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
            this.numericUpDownParameterIndex = new System.Windows.Forms.NumericUpDown();
            this.textBoxParameterIndex = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownParameterIndex)).BeginInit();
            this.SuspendLayout();
            // 
            // numericUpDownParameterIndex
            // 
            this.numericUpDownParameterIndex.Dock = System.Windows.Forms.DockStyle.Right;
            this.numericUpDownParameterIndex.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numericUpDownParameterIndex.Location = new System.Drawing.Point(120, 0);
            this.numericUpDownParameterIndex.Name = "numericUpDownParameterIndex";
            this.numericUpDownParameterIndex.Size = new System.Drawing.Size(16, 21);
            this.numericUpDownParameterIndex.TabIndex = 0;
            this.numericUpDownParameterIndex.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDownParameterIndex.ValueChanged += new System.EventHandler(this.numericUpDownParameterIndex_ValueChanged);
            // 
            // textBoxParameterIndex
            // 
            this.textBoxParameterIndex.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxParameterIndex.Location = new System.Drawing.Point(0, 0);
            this.textBoxParameterIndex.Name = "textBoxParameterIndex";
            this.textBoxParameterIndex.ReadOnly = true;
            this.textBoxParameterIndex.Size = new System.Drawing.Size(119, 21);
            this.textBoxParameterIndex.TabIndex = 1;
            // 
            // ParameterIndexNumericUpDown
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxParameterIndex);
            this.Controls.Add(this.numericUpDownParameterIndex);
            this.Name = "ParameterIndexNumericUpDown";
            this.Size = new System.Drawing.Size(136, 21);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownParameterIndex)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numericUpDownParameterIndex;
        private System.Windows.Forms.TextBox textBoxParameterIndex;
    }
}
