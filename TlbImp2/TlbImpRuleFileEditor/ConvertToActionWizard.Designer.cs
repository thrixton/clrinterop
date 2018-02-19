namespace TlbImpRuleFileEditor
{
    partial class ConvertToActionWizard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConvertToActionWizard));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxByRef = new System.Windows.Forms.CheckBox();
            this.radioButtonInOut = new System.Windows.Forms.RadioButton();
            this.radioButtonOut = new System.Windows.Forms.RadioButton();
            this.radioButtonIn = new System.Windows.Forms.RadioButton();
            this.comboBoxManageType = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxMarshalAs = new System.Windows.Forms.ComboBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBoxAdditionalArrayAttribute = new System.Windows.Forms.GroupBox();
            this.comboBoxSizeParamIndexOffset = new System.Windows.Forms.ComboBox();
            this.radioButtonSizeParamIndexOffset = new System.Windows.Forms.RadioButton();
            this.numericUpDownSizeParamIndex = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownSizeConst = new System.Windows.Forms.NumericUpDown();
            this.checkBoxEnableSizeControl = new System.Windows.Forms.CheckBox();
            this.radioButtonSizeParamIndex = new System.Windows.Forms.RadioButton();
            this.radioButtonSizeConst = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBoxAdditionalArrayAttribute.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSizeParamIndex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSizeConst)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxByRef);
            this.groupBox1.Controls.Add(this.radioButtonInOut);
            this.groupBox1.Controls.Add(this.radioButtonOut);
            this.groupBox1.Controls.Add(this.radioButtonIn);
            this.groupBox1.Location = new System.Drawing.Point(14, 77);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(325, 45);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Direction";
            // 
            // checkBoxByRef
            // 
            this.checkBoxByRef.AutoSize = true;
            this.checkBoxByRef.Location = new System.Drawing.Point(212, 20);
            this.checkBoxByRef.Name = "checkBoxByRef";
            this.checkBoxByRef.Size = new System.Drawing.Size(103, 17);
            this.checkBoxByRef.TabIndex = 3;
            this.checkBoxByRef.Text = "By Reference";
            this.checkBoxByRef.UseVisualStyleBackColor = true;
            // 
            // radioButtonInOut
            // 
            this.radioButtonInOut.AutoSize = true;
            this.radioButtonInOut.Location = new System.Drawing.Point(132, 19);
            this.radioButtonInOut.Name = "radioButtonInOut";
            this.radioButtonInOut.Size = new System.Drawing.Size(75, 17);
            this.radioButtonInOut.TabIndex = 2;
            this.radioButtonInOut.TabStop = true;
            this.radioButtonInOut.Text = "[In, Out]";
            this.radioButtonInOut.UseVisualStyleBackColor = true;
            // 
            // radioButtonOut
            // 
            this.radioButtonOut.AutoSize = true;
            this.radioButtonOut.Location = new System.Drawing.Point(69, 19);
            this.radioButtonOut.Name = "radioButtonOut";
            this.radioButtonOut.Size = new System.Drawing.Size(55, 17);
            this.radioButtonOut.TabIndex = 1;
            this.radioButtonOut.TabStop = true;
            this.radioButtonOut.Text = "[Out]";
            this.radioButtonOut.UseVisualStyleBackColor = true;
            // 
            // radioButtonIn
            // 
            this.radioButtonIn.AutoSize = true;
            this.radioButtonIn.Checked = true;
            this.radioButtonIn.Location = new System.Drawing.Point(19, 19);
            this.radioButtonIn.Name = "radioButtonIn";
            this.radioButtonIn.Size = new System.Drawing.Size(47, 17);
            this.radioButtonIn.TabIndex = 0;
            this.radioButtonIn.TabStop = true;
            this.radioButtonIn.Text = "[In]";
            this.radioButtonIn.UseVisualStyleBackColor = true;
            // 
            // comboBoxManageType
            // 
            this.comboBoxManageType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxManageType.FormattingEnabled = true;
            this.comboBoxManageType.Location = new System.Drawing.Point(100, 19);
            this.comboBoxManageType.Name = "comboBoxManageType";
            this.comboBoxManageType.Size = new System.Drawing.Size(217, 21);
            this.comboBoxManageType.TabIndex = 1;
            this.comboBoxManageType.SelectedIndexChanged += new System.EventHandler(this.comboBoxManageType_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.comboBoxManageType);
            this.groupBox2.Location = new System.Drawing.Point(14, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(325, 59);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Type";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Manage Type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Marshal As";
            // 
            // comboBoxMarshalAs
            // 
            this.comboBoxMarshalAs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMarshalAs.FormattingEnabled = true;
            this.comboBoxMarshalAs.Location = new System.Drawing.Point(100, 19);
            this.comboBoxMarshalAs.Name = "comboBoxMarshalAs";
            this.comboBoxMarshalAs.Size = new System.Drawing.Size(217, 21);
            this.comboBoxMarshalAs.TabIndex = 3;
            this.comboBoxMarshalAs.SelectedIndexChanged += new System.EventHandler(this.comboBoxMarshalAs_SelectedIndexChanged);
            // 
            // buttonOK
            // 
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(83, 319);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(87, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(205, 319);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(87, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.comboBoxMarshalAs);
            this.groupBox3.Location = new System.Drawing.Point(14, 128);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(325, 57);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Custom Attribute";
            // 
            // groupBoxAdditionalArrayAttribute
            // 
            this.groupBoxAdditionalArrayAttribute.Controls.Add(this.comboBoxSizeParamIndexOffset);
            this.groupBoxAdditionalArrayAttribute.Controls.Add(this.radioButtonSizeParamIndexOffset);
            this.groupBoxAdditionalArrayAttribute.Controls.Add(this.numericUpDownSizeParamIndex);
            this.groupBoxAdditionalArrayAttribute.Controls.Add(this.numericUpDownSizeConst);
            this.groupBoxAdditionalArrayAttribute.Controls.Add(this.checkBoxEnableSizeControl);
            this.groupBoxAdditionalArrayAttribute.Controls.Add(this.radioButtonSizeParamIndex);
            this.groupBoxAdditionalArrayAttribute.Controls.Add(this.radioButtonSizeConst);
            this.groupBoxAdditionalArrayAttribute.Enabled = false;
            this.groupBoxAdditionalArrayAttribute.Location = new System.Drawing.Point(14, 191);
            this.groupBoxAdditionalArrayAttribute.Name = "groupBoxAdditionalArrayAttribute";
            this.groupBoxAdditionalArrayAttribute.Size = new System.Drawing.Size(325, 122);
            this.groupBoxAdditionalArrayAttribute.TabIndex = 6;
            this.groupBoxAdditionalArrayAttribute.TabStop = false;
            this.groupBoxAdditionalArrayAttribute.Text = "Additional Array Attribute";
            // 
            // comboBoxSizeParamIndexOffset
            // 
            this.comboBoxSizeParamIndexOffset.Enabled = false;
            this.comboBoxSizeParamIndexOffset.FormattingEnabled = true;
            this.comboBoxSizeParamIndexOffset.Items.AddRange(new object[] {
            "-3",
            "-2",
            "-1",
            "+1",
            "+2",
            "+3"});
            this.comboBoxSizeParamIndexOffset.Location = new System.Drawing.Point(191, 93);
            this.comboBoxSizeParamIndexOffset.Name = "comboBoxSizeParamIndexOffset";
            this.comboBoxSizeParamIndexOffset.Size = new System.Drawing.Size(118, 21);
            this.comboBoxSizeParamIndexOffset.TabIndex = 9;
            this.comboBoxSizeParamIndexOffset.Text = "+1";
            // 
            // radioButtonSizeParamIndexOffset
            // 
            this.radioButtonSizeParamIndexOffset.AutoSize = true;
            this.radioButtonSizeParamIndexOffset.Enabled = false;
            this.radioButtonSizeParamIndexOffset.Location = new System.Drawing.Point(33, 94);
            this.radioButtonSizeParamIndexOffset.Name = "radioButtonSizeParamIndexOffset";
            this.radioButtonSizeParamIndexOffset.Size = new System.Drawing.Size(157, 17);
            this.radioButtonSizeParamIndexOffset.TabIndex = 8;
            this.radioButtonSizeParamIndexOffset.TabStop = true;
            this.radioButtonSizeParamIndexOffset.Text = "SizeParamIndex Offset";
            this.radioButtonSizeParamIndexOffset.UseVisualStyleBackColor = true;
            this.radioButtonSizeParamIndexOffset.CheckedChanged += new System.EventHandler(this.radioButtonSizeParamIndexOffset_CheckedChanged);
            // 
            // numericUpDownSizeParamIndex
            // 
            this.numericUpDownSizeParamIndex.Enabled = false;
            this.numericUpDownSizeParamIndex.Location = new System.Drawing.Point(191, 70);
            this.numericUpDownSizeParamIndex.Name = "numericUpDownSizeParamIndex";
            this.numericUpDownSizeParamIndex.Size = new System.Drawing.Size(119, 21);
            this.numericUpDownSizeParamIndex.TabIndex = 7;
            // 
            // numericUpDownSizeConst
            // 
            this.numericUpDownSizeConst.Enabled = false;
            this.numericUpDownSizeConst.Location = new System.Drawing.Point(191, 44);
            this.numericUpDownSizeConst.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownSizeConst.Name = "numericUpDownSizeConst";
            this.numericUpDownSizeConst.Size = new System.Drawing.Size(119, 21);
            this.numericUpDownSizeConst.TabIndex = 7;
            // 
            // checkBoxEnableSizeControl
            // 
            this.checkBoxEnableSizeControl.AutoSize = true;
            this.checkBoxEnableSizeControl.Location = new System.Drawing.Point(12, 21);
            this.checkBoxEnableSizeControl.Name = "checkBoxEnableSizeControl";
            this.checkBoxEnableSizeControl.Size = new System.Drawing.Size(138, 17);
            this.checkBoxEnableSizeControl.TabIndex = 6;
            this.checkBoxEnableSizeControl.Text = "Enable Size Control";
            this.checkBoxEnableSizeControl.UseVisualStyleBackColor = true;
            this.checkBoxEnableSizeControl.CheckedChanged += new System.EventHandler(this.checkBoxEnableSizeControl_CheckedChanged);
            // 
            // radioButtonSizeParamIndex
            // 
            this.radioButtonSizeParamIndex.AutoSize = true;
            this.radioButtonSizeParamIndex.Enabled = false;
            this.radioButtonSizeParamIndex.Location = new System.Drawing.Point(33, 70);
            this.radioButtonSizeParamIndex.Name = "radioButtonSizeParamIndex";
            this.radioButtonSizeParamIndex.Size = new System.Drawing.Size(119, 17);
            this.radioButtonSizeParamIndex.TabIndex = 4;
            this.radioButtonSizeParamIndex.TabStop = true;
            this.radioButtonSizeParamIndex.Text = "SizeParamIndex";
            this.radioButtonSizeParamIndex.UseVisualStyleBackColor = true;
            this.radioButtonSizeParamIndex.CheckedChanged += new System.EventHandler(this.radioButtonSizeParamIndex_CheckedChanged);
            // 
            // radioButtonSizeConst
            // 
            this.radioButtonSizeConst.AutoSize = true;
            this.radioButtonSizeConst.Checked = true;
            this.radioButtonSizeConst.Enabled = false;
            this.radioButtonSizeConst.Location = new System.Drawing.Point(33, 44);
            this.radioButtonSizeConst.Name = "radioButtonSizeConst";
            this.radioButtonSizeConst.Size = new System.Drawing.Size(82, 17);
            this.radioButtonSizeConst.TabIndex = 2;
            this.radioButtonSizeConst.TabStop = true;
            this.radioButtonSizeConst.Text = "SizeConst";
            this.radioButtonSizeConst.UseVisualStyleBackColor = true;
            this.radioButtonSizeConst.CheckedChanged += new System.EventHandler(this.radioButtonSizeConst_CheckedChanged);
            // 
            // ConvertToActionWizard
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(362, 354);
            this.Controls.Add(this.groupBoxAdditionalArrayAttribute);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConvertToActionWizard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ConvertTo Action Wizard";
            this.Load += new System.EventHandler(this.ConvertToActionWizard_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBoxAdditionalArrayAttribute.ResumeLayout(false);
            this.groupBoxAdditionalArrayAttribute.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSizeParamIndex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSizeConst)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonIn;
        private System.Windows.Forms.RadioButton radioButtonInOut;
        private System.Windows.Forms.RadioButton radioButtonOut;
        private System.Windows.Forms.ComboBox comboBoxManageType;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxMarshalAs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBoxAdditionalArrayAttribute;
        private System.Windows.Forms.RadioButton radioButtonSizeParamIndex;
        private System.Windows.Forms.RadioButton radioButtonSizeConst;
        private System.Windows.Forms.CheckBox checkBoxEnableSizeControl;
        private System.Windows.Forms.NumericUpDown numericUpDownSizeParamIndex;
        private System.Windows.Forms.NumericUpDown numericUpDownSizeConst;
        private System.Windows.Forms.CheckBox checkBoxByRef;
        private System.Windows.Forms.ComboBox comboBoxSizeParamIndexOffset;
        private System.Windows.Forms.RadioButton radioButtonSizeParamIndexOffset;
    }
}