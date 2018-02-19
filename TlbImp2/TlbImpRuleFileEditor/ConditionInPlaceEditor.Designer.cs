namespace TlbImpRuleFileEditor
{
    partial class ConditionInPlaceEditor
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
            this.conditionComboBox = new System.Windows.Forms.ComboBox();
            this.operatorComboBox = new System.Windows.Forms.ComboBox();
            this.valueComboBox = new System.Windows.Forms.ComboBox();
            this.buttonValueInputHelper = new System.Windows.Forms.Button();
            this.parameterIndexNumericUpDown = new TlbImpRuleFileEditor.ParameterIndexNumericUpDown();
            this.SuspendLayout();
            // 
            // conditionComboBox
            // 
            this.conditionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.conditionComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.conditionComboBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.conditionComboBox.FormattingEnabled = true;
            this.conditionComboBox.ItemHeight = 13;
            this.conditionComboBox.Location = new System.Drawing.Point(0, 0);
            this.conditionComboBox.Margin = new System.Windows.Forms.Padding(0);
            this.conditionComboBox.Name = "conditionComboBox";
            this.conditionComboBox.Size = new System.Drawing.Size(151, 21);
            this.conditionComboBox.TabIndex = 5;
            this.conditionComboBox.SelectedIndexChanged += new System.EventHandler(this.conditionComboBox_SelectedIndexChanged);
            // 
            // operatorComboBox
            // 
            this.operatorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.operatorComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.operatorComboBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.operatorComboBox.FormattingEnabled = true;
            this.operatorComboBox.Location = new System.Drawing.Point(151, 0);
            this.operatorComboBox.Margin = new System.Windows.Forms.Padding(0);
            this.operatorComboBox.Name = "operatorComboBox";
            this.operatorComboBox.Size = new System.Drawing.Size(65, 21);
            this.operatorComboBox.TabIndex = 7;
            // 
            // valueComboBox
            // 
            this.valueComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.valueComboBox.FormattingEnabled = true;
            this.valueComboBox.Location = new System.Drawing.Point(216, 0);
            this.valueComboBox.Margin = new System.Windows.Forms.Padding(0);
            this.valueComboBox.Name = "valueComboBox";
            this.valueComboBox.Size = new System.Drawing.Size(136, 21);
            this.valueComboBox.TabIndex = 10;
            this.valueComboBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.valueComboBox_KeyDown);
            // 
            // buttonValueInputHelper
            // 
            this.buttonValueInputHelper.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonValueInputHelper.Location = new System.Drawing.Point(353, 0);
            this.buttonValueInputHelper.Margin = new System.Windows.Forms.Padding(0);
            this.buttonValueInputHelper.Name = "buttonValueInputHelper";
            this.buttonValueInputHelper.Size = new System.Drawing.Size(27, 21);
            this.buttonValueInputHelper.TabIndex = 11;
            this.buttonValueInputHelper.Text = "...";
            this.buttonValueInputHelper.UseVisualStyleBackColor = true;
            this.buttonValueInputHelper.Click += new System.EventHandler(this.buttonValueInputHelper_Click);
            // 
            // parameterIndexNumericUpDown
            // 
            this.parameterIndexNumericUpDown.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.parameterIndexNumericUpDown.Location = new System.Drawing.Point(216, 0);
            this.parameterIndexNumericUpDown.Name = "parameterIndexNumericUpDown";
            this.parameterIndexNumericUpDown.Size = new System.Drawing.Size(136, 21);
            this.parameterIndexNumericUpDown.TabIndex = 12;
            this.parameterIndexNumericUpDown.Value = 0;
            // 
            // ConditionInPlaceEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.parameterIndexNumericUpDown);
            this.Controls.Add(this.buttonValueInputHelper);
            this.Controls.Add(this.valueComboBox);
            this.Controls.Add(this.operatorComboBox);
            this.Controls.Add(this.conditionComboBox);
            this.Name = "ConditionInPlaceEditor";
            this.Size = new System.Drawing.Size(381, 22);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox conditionComboBox;
        private System.Windows.Forms.ComboBox operatorComboBox;
        private System.Windows.Forms.ComboBox valueComboBox;
        private System.Windows.Forms.Button buttonValueInputHelper;
        private ParameterIndexNumericUpDown parameterIndexNumericUpDown;
    }
}
