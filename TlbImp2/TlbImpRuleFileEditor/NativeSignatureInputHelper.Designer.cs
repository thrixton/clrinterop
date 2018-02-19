namespace TlbImpRuleFileEditor
{
    partial class NativeSignatureInputHelper
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NativeSignatureInputHelper));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.numericUpDownIndirection = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxNativeType = new System.Windows.Forms.ComboBox();
            this.groupBoxIndirectionNumber = new System.Windows.Forms.GroupBox();
            this.checkBoxMakeArray = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownArraySize = new System.Windows.Forms.NumericUpDown();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIndirection)).BeginInit();
            this.groupBoxIndirectionNumber.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownArraySize)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(171, 187);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(87, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(61, 187);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(87, 23);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonAdd);
            this.groupBox2.Controls.Add(this.numericUpDownIndirection);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.comboBoxNativeType);
            this.groupBox2.Location = new System.Drawing.Point(14, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(330, 87);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Type";
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(289, 19);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(34, 23);
            this.buttonAdd.TabIndex = 9;
            this.buttonAdd.Text = "...";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // numericUpDownIndirection
            // 
            this.numericUpDownIndirection.Location = new System.Drawing.Point(127, 55);
            this.numericUpDownIndirection.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownIndirection.Name = "numericUpDownIndirection";
            this.numericUpDownIndirection.Size = new System.Drawing.Size(196, 21);
            this.numericUpDownIndirection.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Indirection Number";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Tlb Type";
            // 
            // comboBoxNativeType
            // 
            this.comboBoxNativeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxNativeType.FormattingEnabled = true;
            this.comboBoxNativeType.Location = new System.Drawing.Point(127, 19);
            this.comboBoxNativeType.Name = "comboBoxNativeType";
            this.comboBoxNativeType.Size = new System.Drawing.Size(157, 21);
            this.comboBoxNativeType.TabIndex = 1;
            this.comboBoxNativeType.SelectedIndexChanged += new System.EventHandler(this.comboBoxNativeType_SelectedIndexChanged);
            // 
            // groupBoxIndirectionNumber
            // 
            this.groupBoxIndirectionNumber.Controls.Add(this.checkBoxMakeArray);
            this.groupBoxIndirectionNumber.Controls.Add(this.label3);
            this.groupBoxIndirectionNumber.Controls.Add(this.numericUpDownArraySize);
            this.groupBoxIndirectionNumber.Location = new System.Drawing.Point(14, 105);
            this.groupBoxIndirectionNumber.Name = "groupBoxIndirectionNumber";
            this.groupBoxIndirectionNumber.Size = new System.Drawing.Size(330, 76);
            this.groupBoxIndirectionNumber.TabIndex = 8;
            this.groupBoxIndirectionNumber.TabStop = false;
            this.groupBoxIndirectionNumber.Text = "Additional Array Infomation";
            // 
            // checkBoxMakeArray
            // 
            this.checkBoxMakeArray.AutoSize = true;
            this.checkBoxMakeArray.Location = new System.Drawing.Point(12, 19);
            this.checkBoxMakeArray.Name = "checkBoxMakeArray";
            this.checkBoxMakeArray.Size = new System.Drawing.Size(92, 17);
            this.checkBoxMakeArray.TabIndex = 8;
            this.checkBoxMakeArray.Text = "Make Array";
            this.checkBoxMakeArray.UseVisualStyleBackColor = true;
            this.checkBoxMakeArray.CheckedChanged += new System.EventHandler(this.checkBoxMakeArray_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(57, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Array Size";
            // 
            // numericUpDownArraySize
            // 
            this.numericUpDownArraySize.Enabled = false;
            this.numericUpDownArraySize.Location = new System.Drawing.Point(127, 41);
            this.numericUpDownArraySize.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownArraySize.Name = "numericUpDownArraySize";
            this.numericUpDownArraySize.Size = new System.Drawing.Size(196, 21);
            this.numericUpDownArraySize.TabIndex = 7;
            // 
            // NativeSignatureInputHelper
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(358, 227);
            this.Controls.Add(this.groupBoxIndirectionNumber);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NativeSignatureInputHelper";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "NativeSignature Input Helper";
            this.Load += new System.EventHandler(this.NativeSignatureInputHelper_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIndirection)).EndInit();
            this.groupBoxIndirectionNumber.ResumeLayout(false);
            this.groupBoxIndirectionNumber.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownArraySize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxNativeType;
        private System.Windows.Forms.NumericUpDown numericUpDownIndirection;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBoxIndirectionNumber;
        private System.Windows.Forms.NumericUpDown numericUpDownArraySize;
        private System.Windows.Forms.CheckBox checkBoxMakeArray;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonAdd;
    }
}