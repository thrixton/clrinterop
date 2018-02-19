namespace TlbImpRuleFileEditor
{
    partial class AddAttributeActionWizard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddAttributeActionWizard));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonTypeSelector = new System.Windows.Forms.Button();
            this.textBoxType = new System.Windows.Forms.TextBox();
            this.textBoxAssembly = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listBoxCtor = new System.Windows.Forms.ListBox();
            this.textBoxByteValue = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.buttonDataInput = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(222, 327);
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
            this.buttonOK.Location = new System.Drawing.Point(94, 327);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(87, 23);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.buttonTypeSelector);
            this.groupBox2.Controls.Add(this.textBoxType);
            this.groupBox2.Controls.Add(this.textBoxAssembly);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(14, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(377, 70);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Attribute";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Type :";
            // 
            // buttonTypeSelector
            // 
            this.buttonTypeSelector.Location = new System.Drawing.Point(336, 13);
            this.buttonTypeSelector.Name = "buttonTypeSelector";
            this.buttonTypeSelector.Size = new System.Drawing.Size(34, 23);
            this.buttonTypeSelector.TabIndex = 10;
            this.buttonTypeSelector.Text = "...";
            this.buttonTypeSelector.UseVisualStyleBackColor = true;
            this.buttonTypeSelector.Click += new System.EventHandler(this.buttonTypeSelector_Click);
            // 
            // textBoxType
            // 
            this.textBoxType.Location = new System.Drawing.Point(80, 41);
            this.textBoxType.Name = "textBoxType";
            this.textBoxType.ReadOnly = true;
            this.textBoxType.Size = new System.Drawing.Size(248, 21);
            this.textBoxType.TabIndex = 9;
            this.textBoxType.TextChanged += new System.EventHandler(this.textBoxType_TextChanged);
            // 
            // textBoxAssembly
            // 
            this.textBoxAssembly.Location = new System.Drawing.Point(80, 15);
            this.textBoxAssembly.Name = "textBoxAssembly";
            this.textBoxAssembly.ReadOnly = true;
            this.textBoxAssembly.Size = new System.Drawing.Size(248, 21);
            this.textBoxAssembly.TabIndex = 8;
            this.textBoxAssembly.TextChanged += new System.EventHandler(this.textBoxAssembly_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Assembly :";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.listBoxCtor);
            this.groupBox1.Location = new System.Drawing.Point(14, 88);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(377, 114);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Constructor";
            // 
            // listBoxCtor
            // 
            this.listBoxCtor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxCtor.FormattingEnabled = true;
            this.listBoxCtor.Location = new System.Drawing.Point(3, 17);
            this.listBoxCtor.Name = "listBoxCtor";
            this.listBoxCtor.Size = new System.Drawing.Size(371, 82);
            this.listBoxCtor.TabIndex = 0;
            this.listBoxCtor.SelectedIndexChanged += new System.EventHandler(this.listBoxCtor_SelectedIndexChanged);
            // 
            // textBoxByteValue
            // 
            this.textBoxByteValue.Dock = System.Windows.Forms.DockStyle.Left;
            this.textBoxByteValue.Location = new System.Drawing.Point(3, 17);
            this.textBoxByteValue.Multiline = true;
            this.textBoxByteValue.Name = "textBoxByteValue";
            this.textBoxByteValue.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxByteValue.Size = new System.Drawing.Size(325, 94);
            this.textBoxByteValue.TabIndex = 0;
            this.textBoxByteValue.Text = "01 00 00 00";
            this.textBoxByteValue.TextChanged += new System.EventHandler(this.textBoxByteValue_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.buttonDataInput);
            this.groupBox3.Controls.Add(this.textBoxByteValue);
            this.groupBox3.Location = new System.Drawing.Point(14, 208);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(377, 114);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Data";
            // 
            // buttonDataInput
            // 
            this.buttonDataInput.Location = new System.Drawing.Point(336, 14);
            this.buttonDataInput.Name = "buttonDataInput";
            this.buttonDataInput.Size = new System.Drawing.Size(34, 23);
            this.buttonDataInput.TabIndex = 11;
            this.buttonDataInput.Text = "...";
            this.buttonDataInput.UseVisualStyleBackColor = true;
            this.buttonDataInput.Click += new System.EventHandler(this.buttonDataInput_Click);
            // 
            // AddAttributeActionWizard
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(405, 359);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddAttributeActionWizard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AddAttribute Action Wizard";
            this.Load += new System.EventHandler(this.AddAttributeActionWizard_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxType;
        private System.Windows.Forms.TextBox textBoxAssembly;
        private System.Windows.Forms.Button buttonTypeSelector;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox listBoxCtor;
        private System.Windows.Forms.TextBox textBoxByteValue;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button buttonDataInput;
    }
}