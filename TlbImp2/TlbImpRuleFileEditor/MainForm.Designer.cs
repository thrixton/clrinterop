using TypeLibraryTreeView;
namespace TlbImpRuleFileEditor
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newRuleFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openRuleFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveRuleFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveRuleFileAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.openTypeLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNewRuleEditToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeRuleEditToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAllRulesEditToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.removeConditionEditToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAllSubconditionsEditToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.modifyActionEditToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutTlbImpRuleFileEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNewRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openTlbFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveRuleFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openRuleFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.ruleSetContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeAllRulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ruleContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actionContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.modifyActionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compositeConditionContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeConditionToolStripMenuItem_MultiCondition = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAllSubconditionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.atomicConditionContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeConditionToolStripMenuItem_SingleCondition = new System.Windows.Forms.ToolStripMenuItem();
            this.ruleIconList = new System.Windows.Forms.ImageList(this.components);
            this.systemToolStrip = new System.Windows.Forms.ToolStrip();
            this.newRuleFileToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.openRuleFileToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.saveRuleFileToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.saveRuleFileAsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.openTlbToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.addNewRuleToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.removeRuleToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.removeAllRulesToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.removeConditionToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.removeAllSubconditionsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.modifyActionToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.tlbTreeContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addNewRuleFromHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.treeSplitContainer = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tlbTreeView = new TypeLibraryTreeView.TlbTreeView();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.splitContainerRuleFile = new System.Windows.Forms.SplitContainer();
            this.splitContainerConditionExp = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.ruleTreeView = new TlbImpRuleFileEditor.RuleTreeView();
            this.conditionInPlaceEditor = new TlbImpRuleFileEditor.ConditionInPlaceEditor();
            this.insertionHighlight = new TlbImpRuleFileEditor.InsertionHighlight(this.components);
            this.imageCloseButton = new TlbImpRuleFileEditor.ImageCloseButton();
            this.richTextBoxConditionExp = new TlbImpRuleFileEditor.RichTextBoxConditionExpression();
            this.mainMenuStrip.SuspendLayout();
            this.ruleSetContextMenuStrip.SuspendLayout();
            this.ruleContextMenuStrip.SuspendLayout();
            this.actionContextMenuStrip.SuspendLayout();
            this.compositeConditionContextMenuStrip.SuspendLayout();
            this.atomicConditionContextMenuStrip.SuspendLayout();
            this.systemToolStrip.SuspendLayout();
            this.tlbTreeContextMenuStrip.SuspendLayout();
            this.treeSplitContainer.Panel1.SuspendLayout();
            this.treeSplitContainer.Panel2.SuspendLayout();
            this.treeSplitContainer.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.splitContainerRuleFile.Panel1.SuspendLayout();
            this.splitContainerRuleFile.Panel2.SuspendLayout();
            this.splitContainerRuleFile.SuspendLayout();
            this.splitContainerConditionExp.Panel1.SuspendLayout();
            this.splitContainerConditionExp.Panel2.SuspendLayout();
            this.splitContainerConditionExp.SuspendLayout();
            this.ruleTreeView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageCloseButton)).BeginInit();
            this.SuspendLayout();
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.mainMenuStrip.Size = new System.Drawing.Size(880, 24);
            this.mainMenuStrip.TabIndex = 1;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newRuleFileToolStripMenuItem,
            this.openRuleFileToolStripMenuItem,
            this.saveRuleFileToolStripMenuItem,
            this.saveRuleFileAsToolStripMenuItem,
            this.toolStripSeparator2,
            this.openTypeLibraryToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(38, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newRuleFileToolStripMenuItem
            // 
            this.newRuleFileToolStripMenuItem.Name = "newRuleFileToolStripMenuItem";
            this.newRuleFileToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.newRuleFileToolStripMenuItem.Text = "New Rule File";
            this.newRuleFileToolStripMenuItem.Click += new System.EventHandler(this.newRuleFileToolStripMenuItem_Click);
            // 
            // openRuleFileToolStripMenuItem
            // 
            this.openRuleFileToolStripMenuItem.Name = "openRuleFileToolStripMenuItem";
            this.openRuleFileToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.openRuleFileToolStripMenuItem.Text = "Open Rule File...";
            this.openRuleFileToolStripMenuItem.Click += new System.EventHandler(this.openRuleFileToolStripMenuItem_Click);
            // 
            // saveRuleFileToolStripMenuItem
            // 
            this.saveRuleFileToolStripMenuItem.Name = "saveRuleFileToolStripMenuItem";
            this.saveRuleFileToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.saveRuleFileToolStripMenuItem.Text = "Save Rule File";
            this.saveRuleFileToolStripMenuItem.Click += new System.EventHandler(this.saveRuleFileToolStripMenuItem_Click);
            // 
            // saveRuleFileAsToolStripMenuItem
            // 
            this.saveRuleFileAsToolStripMenuItem.Name = "saveRuleFileAsToolStripMenuItem";
            this.saveRuleFileAsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.saveRuleFileAsToolStripMenuItem.Text = "Save Rule File As...";
            this.saveRuleFileAsToolStripMenuItem.Click += new System.EventHandler(this.saveRuleFileAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(186, 6);
            // 
            // openTypeLibraryToolStripMenuItem
            // 
            this.openTypeLibraryToolStripMenuItem.Name = "openTypeLibraryToolStripMenuItem";
            this.openTypeLibraryToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.openTypeLibraryToolStripMenuItem.Text = "Load Type Library...";
            this.openTypeLibraryToolStripMenuItem.Click += new System.EventHandler(this.openTypeLibraryToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNewRuleEditToolStripMenuItem,
            this.removeRuleEditToolStripMenuItem,
            this.removeAllRulesEditToolStripMenuItem,
            this.toolStripSeparator4,
            this.removeConditionEditToolStripMenuItem,
            this.removeAllSubconditionsEditToolStripMenuItem,
            this.toolStripSeparator5,
            this.modifyActionEditToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.DropDownOpening += new System.EventHandler(this.editToolStripMenuItem_DropDownOpening);
            // 
            // addNewRuleEditToolStripMenuItem
            // 
            this.addNewRuleEditToolStripMenuItem.Name = "addNewRuleEditToolStripMenuItem";
            this.addNewRuleEditToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.addNewRuleEditToolStripMenuItem.Text = "Add New Rule...";
            this.addNewRuleEditToolStripMenuItem.Click += new System.EventHandler(this.addNewRuleEditToolStripMenuItem_Click);
            // 
            // removeRuleEditToolStripMenuItem
            // 
            this.removeRuleEditToolStripMenuItem.Name = "removeRuleEditToolStripMenuItem";
            this.removeRuleEditToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.removeRuleEditToolStripMenuItem.Text = "Remove Rule";
            this.removeRuleEditToolStripMenuItem.Click += new System.EventHandler(this.removeRuleEditToolStripMenuItem_Click);
            // 
            // removeAllRulesEditToolStripMenuItem
            // 
            this.removeAllRulesEditToolStripMenuItem.Name = "removeAllRulesEditToolStripMenuItem";
            this.removeAllRulesEditToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.removeAllRulesEditToolStripMenuItem.Text = "Remove All Rules";
            this.removeAllRulesEditToolStripMenuItem.Click += new System.EventHandler(this.removeAllRulesEditToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(219, 6);
            // 
            // removeConditionEditToolStripMenuItem
            // 
            this.removeConditionEditToolStripMenuItem.Name = "removeConditionEditToolStripMenuItem";
            this.removeConditionEditToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.removeConditionEditToolStripMenuItem.Text = "Remove Condition";
            this.removeConditionEditToolStripMenuItem.Click += new System.EventHandler(this.removeConditionEditToolStripMenuItem_Click);
            // 
            // removeAllSubconditionsEditToolStripMenuItem
            // 
            this.removeAllSubconditionsEditToolStripMenuItem.Name = "removeAllSubconditionsEditToolStripMenuItem";
            this.removeAllSubconditionsEditToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.removeAllSubconditionsEditToolStripMenuItem.Text = "Remove All Subconditions";
            this.removeAllSubconditionsEditToolStripMenuItem.Click += new System.EventHandler(this.removeAllSubconditionsEditToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(219, 6);
            // 
            // modifyActionEditToolStripMenuItem
            // 
            this.modifyActionEditToolStripMenuItem.Name = "modifyActionEditToolStripMenuItem";
            this.modifyActionEditToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.modifyActionEditToolStripMenuItem.Text = "Modify Action...";
            this.modifyActionEditToolStripMenuItem.Click += new System.EventHandler(this.modifyActionEditToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutTlbImpRuleFileEditorToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutTlbImpRuleFileEditorToolStripMenuItem
            // 
            this.aboutTlbImpRuleFileEditorToolStripMenuItem.Name = "aboutTlbImpRuleFileEditorToolStripMenuItem";
            this.aboutTlbImpRuleFileEditorToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.aboutTlbImpRuleFileEditorToolStripMenuItem.Text = "About TlbImp Config File Editor";
            this.aboutTlbImpRuleFileEditorToolStripMenuItem.Click += new System.EventHandler(this.aboutTlbImpRuleFileEditorToolStripMenuItem_Click);
            // 
            // addNewRuleToolStripMenuItem
            // 
            this.addNewRuleToolStripMenuItem.Name = "addNewRuleToolStripMenuItem";
            this.addNewRuleToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.addNewRuleToolStripMenuItem.Text = "Add New Rule...";
            this.addNewRuleToolStripMenuItem.Click += new System.EventHandler(this.addNewRuleToolStripMenuItem_Click);
            // 
            // filesToolStripMenuItem
            // 
            this.filesToolStripMenuItem.Name = "filesToolStripMenuItem";
            this.filesToolStripMenuItem.Size = new System.Drawing.Size(42, 20);
            this.filesToolStripMenuItem.Text = "Files";
            // 
            // openTlbFileDialog
            // 
            this.openTlbFileDialog.Filter = "Type library files|*.tlb|All files|*.*";
            this.openTlbFileDialog.RestoreDirectory = true;
            // 
            // saveRuleFileDialog
            // 
            this.saveRuleFileDialog.Filter = "TlbImp rule files|*.xml|All files|*.*";
            this.saveRuleFileDialog.RestoreDirectory = true;
            // 
            // openRuleFileDialog
            // 
            this.openRuleFileDialog.Filter = "TlbImp rule files|*.xml|All files|*.*";
            // 
            // ruleSetContextMenuStrip
            // 
            this.ruleSetContextMenuStrip.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ruleSetContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNewRuleToolStripMenuItem,
            this.removeAllRulesToolStripMenuItem});
            this.ruleSetContextMenuStrip.Name = "ruleSetContextMenuStrip";
            this.ruleSetContextMenuStrip.Size = new System.Drawing.Size(175, 48);
            // 
            // removeAllRulesToolStripMenuItem
            // 
            this.removeAllRulesToolStripMenuItem.Name = "removeAllRulesToolStripMenuItem";
            this.removeAllRulesToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.removeAllRulesToolStripMenuItem.Text = "Remove All Rules";
            this.removeAllRulesToolStripMenuItem.Click += new System.EventHandler(this.removeAllRulesToolStripMenuItem_Click);
            // 
            // ruleContextMenuStrip
            // 
            this.ruleContextMenuStrip.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ruleContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeRuleToolStripMenuItem});
            this.ruleContextMenuStrip.Name = "ruleContextMenuStrip";
            this.ruleContextMenuStrip.Size = new System.Drawing.Size(151, 26);
            // 
            // removeRuleToolStripMenuItem
            // 
            this.removeRuleToolStripMenuItem.Name = "removeRuleToolStripMenuItem";
            this.removeRuleToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.removeRuleToolStripMenuItem.Text = "Remove Rule";
            this.removeRuleToolStripMenuItem.Click += new System.EventHandler(this.removeRuleToolStripMenuItem_Click);
            // 
            // actionContextMenuStrip
            // 
            this.actionContextMenuStrip.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.actionContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modifyActionToolStripMenuItem});
            this.actionContextMenuStrip.Name = "actionContextMenuStrip";
            this.actionContextMenuStrip.Size = new System.Drawing.Size(163, 26);
            // 
            // modifyActionToolStripMenuItem
            // 
            this.modifyActionToolStripMenuItem.Name = "modifyActionToolStripMenuItem";
            this.modifyActionToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.modifyActionToolStripMenuItem.Text = "Modify Action...";
            this.modifyActionToolStripMenuItem.Click += new System.EventHandler(this.modifyActionToolStripMenuItem_Click);
            // 
            // compositeConditionContextMenuStrip
            // 
            this.compositeConditionContextMenuStrip.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.compositeConditionContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeConditionToolStripMenuItem_MultiCondition,
            this.removeAllSubconditionsToolStripMenuItem});
            this.compositeConditionContextMenuStrip.Name = "multiConditionContextMenuStrip";
            this.compositeConditionContextMenuStrip.Size = new System.Drawing.Size(223, 48);
            // 
            // removeConditionToolStripMenuItem_MultiCondition
            // 
            this.removeConditionToolStripMenuItem_MultiCondition.Name = "removeConditionToolStripMenuItem_MultiCondition";
            this.removeConditionToolStripMenuItem_MultiCondition.Size = new System.Drawing.Size(222, 22);
            this.removeConditionToolStripMenuItem_MultiCondition.Text = "Remove Condition";
            this.removeConditionToolStripMenuItem_MultiCondition.Click += new System.EventHandler(this.removeConditionToolStripMenuItem_MultiCondition_Click);
            // 
            // removeAllSubconditionsToolStripMenuItem
            // 
            this.removeAllSubconditionsToolStripMenuItem.Name = "removeAllSubconditionsToolStripMenuItem";
            this.removeAllSubconditionsToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.removeAllSubconditionsToolStripMenuItem.Text = "Remove All Subconditions";
            this.removeAllSubconditionsToolStripMenuItem.Click += new System.EventHandler(this.removeAllSubconditionsToolStripMenuItem_Click);
            // 
            // atomicConditionContextMenuStrip
            // 
            this.atomicConditionContextMenuStrip.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.atomicConditionContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeConditionToolStripMenuItem_SingleCondition});
            this.atomicConditionContextMenuStrip.Name = "singleConditionContextMenuStrip";
            this.atomicConditionContextMenuStrip.Size = new System.Drawing.Size(180, 26);
            // 
            // removeConditionToolStripMenuItem_SingleCondition
            // 
            this.removeConditionToolStripMenuItem_SingleCondition.Name = "removeConditionToolStripMenuItem_SingleCondition";
            this.removeConditionToolStripMenuItem_SingleCondition.Size = new System.Drawing.Size(179, 22);
            this.removeConditionToolStripMenuItem_SingleCondition.Text = "Remove Condition";
            this.removeConditionToolStripMenuItem_SingleCondition.Click += new System.EventHandler(this.removeConditionToolStripMenuItem_SingleCondition_Click);
            // 
            // ruleIconList
            // 
            this.ruleIconList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ruleIconList.ImageStream")));
            this.ruleIconList.TransparentColor = System.Drawing.Color.Fuchsia;
            this.ruleIconList.Images.SetKeyName(0, "Rules");
            this.ruleIconList.Images.SetKeyName(1, "Rule");
            this.ruleIconList.Images.SetKeyName(2, "Action");
            this.ruleIconList.Images.SetKeyName(3, "Condition");
            this.ruleIconList.Images.SetKeyName(4, "MultiCondition");
            this.ruleIconList.Images.SetKeyName(5, "SingleCondition");
            this.ruleIconList.Images.SetKeyName(6, "Category");
            // 
            // systemToolStrip
            // 
            this.systemToolStrip.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.systemToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newRuleFileToolStripButton,
            this.openRuleFileToolStripButton,
            this.saveRuleFileToolStripButton,
            this.saveRuleFileAsToolStripButton,
            this.toolStripSeparator1,
            this.openTlbToolStripButton,
            this.toolStripSeparator3,
            this.addNewRuleToolStripButton,
            this.removeRuleToolStripButton,
            this.removeAllRulesToolStripButton,
            this.toolStripSeparator7,
            this.removeConditionToolStripButton,
            this.removeAllSubconditionsToolStripButton,
            this.toolStripSeparator8,
            this.modifyActionToolStripButton});
            this.systemToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.systemToolStrip.Location = new System.Drawing.Point(0, 24);
            this.systemToolStrip.Name = "systemToolStrip";
            this.systemToolStrip.Size = new System.Drawing.Size(880, 25);
            this.systemToolStrip.TabIndex = 7;
            this.systemToolStrip.Text = "toolStrip1";
            // 
            // newRuleFileToolStripButton
            // 
            this.newRuleFileToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.newRuleFileToolStripButton.Image = global::TlbImpRuleFileEditor.Properties.Resources.NewRuleFile;
            this.newRuleFileToolStripButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.newRuleFileToolStripButton.Name = "newRuleFileToolStripButton";
            this.newRuleFileToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.newRuleFileToolStripButton.Text = "New Rule File";
            this.newRuleFileToolStripButton.Click += new System.EventHandler(this.newRuleFileToolStripButton_Click);
            // 
            // openRuleFileToolStripButton
            // 
            this.openRuleFileToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openRuleFileToolStripButton.Image = global::TlbImpRuleFileEditor.Properties.Resources.OpenRuleFile;
            this.openRuleFileToolStripButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.openRuleFileToolStripButton.Name = "openRuleFileToolStripButton";
            this.openRuleFileToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.openRuleFileToolStripButton.Text = "Open Rule File";
            this.openRuleFileToolStripButton.Click += new System.EventHandler(this.openRuleFileToolStripButton_Click);
            // 
            // saveRuleFileToolStripButton
            // 
            this.saveRuleFileToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveRuleFileToolStripButton.Image = global::TlbImpRuleFileEditor.Properties.Resources.SaveRuleFile;
            this.saveRuleFileToolStripButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.saveRuleFileToolStripButton.Name = "saveRuleFileToolStripButton";
            this.saveRuleFileToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.saveRuleFileToolStripButton.Text = "Save Rule File";
            this.saveRuleFileToolStripButton.Click += new System.EventHandler(this.saveRuleFileToolStripButton_Click);
            // 
            // saveRuleFileAsToolStripButton
            // 
            this.saveRuleFileAsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveRuleFileAsToolStripButton.Image = global::TlbImpRuleFileEditor.Properties.Resources.SaveAs;
            this.saveRuleFileAsToolStripButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.saveRuleFileAsToolStripButton.Name = "saveRuleFileAsToolStripButton";
            this.saveRuleFileAsToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.saveRuleFileAsToolStripButton.Text = "Save Rule File As";
            this.saveRuleFileAsToolStripButton.Click += new System.EventHandler(this.saveRuleFileAsToolStripButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // openTlbToolStripButton
            // 
            this.openTlbToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openTlbToolStripButton.Image = global::TlbImpRuleFileEditor.Properties.Resources.LoadTypeLibrary;
            this.openTlbToolStripButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.openTlbToolStripButton.Name = "openTlbToolStripButton";
            this.openTlbToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.openTlbToolStripButton.Text = "Load Type Library";
            this.openTlbToolStripButton.Click += new System.EventHandler(this.openTlbToolStripButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // addNewRuleToolStripButton
            // 
            this.addNewRuleToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addNewRuleToolStripButton.Image = global::TlbImpRuleFileEditor.Properties.Resources.addrule;
            this.addNewRuleToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addNewRuleToolStripButton.Name = "addNewRuleToolStripButton";
            this.addNewRuleToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.addNewRuleToolStripButton.Text = "Add New Rule";
            this.addNewRuleToolStripButton.Click += new System.EventHandler(this.addNewRuleToolStripButton_Click);
            // 
            // removeRuleToolStripButton
            // 
            this.removeRuleToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.removeRuleToolStripButton.Image = global::TlbImpRuleFileEditor.Properties.Resources.removerule;
            this.removeRuleToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.removeRuleToolStripButton.Name = "removeRuleToolStripButton";
            this.removeRuleToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.removeRuleToolStripButton.Text = "Remove Rule";
            this.removeRuleToolStripButton.Click += new System.EventHandler(this.removeRuleToolStripButton_Click);
            // 
            // removeAllRulesToolStripButton
            // 
            this.removeAllRulesToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.removeAllRulesToolStripButton.Image = global::TlbImpRuleFileEditor.Properties.Resources.removeallrules;
            this.removeAllRulesToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.removeAllRulesToolStripButton.Name = "removeAllRulesToolStripButton";
            this.removeAllRulesToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.removeAllRulesToolStripButton.Text = "Remove All Rules";
            this.removeAllRulesToolStripButton.Click += new System.EventHandler(this.removeAllRulesToolStripButton_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // removeConditionToolStripButton
            // 
            this.removeConditionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.removeConditionToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("removeConditionToolStripButton.Image")));
            this.removeConditionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.removeConditionToolStripButton.Name = "removeConditionToolStripButton";
            this.removeConditionToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.removeConditionToolStripButton.Text = "Remove Condition";
            this.removeConditionToolStripButton.Click += new System.EventHandler(this.removeConditionToolStripButton_Click);
            // 
            // removeAllSubconditionsToolStripButton
            // 
            this.removeAllSubconditionsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.removeAllSubconditionsToolStripButton.Image = global::TlbImpRuleFileEditor.Properties.Resources.removeallsubconditions;
            this.removeAllSubconditionsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.removeAllSubconditionsToolStripButton.Name = "removeAllSubconditionsToolStripButton";
            this.removeAllSubconditionsToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.removeAllSubconditionsToolStripButton.Text = "Remove All Subconditions";
            this.removeAllSubconditionsToolStripButton.Click += new System.EventHandler(this.removeAllSubconditionsToolStripButton_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
            // 
            // modifyActionToolStripButton
            // 
            this.modifyActionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.modifyActionToolStripButton.Image = global::TlbImpRuleFileEditor.Properties.Resources.modifyaction;
            this.modifyActionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.modifyActionToolStripButton.Name = "modifyActionToolStripButton";
            this.modifyActionToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.modifyActionToolStripButton.Text = "Modify Action";
            this.modifyActionToolStripButton.Click += new System.EventHandler(this.modifyActionToolStripButton_Click);
            // 
            // tlbTreeContextMenuStrip
            // 
            this.tlbTreeContextMenuStrip.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tlbTreeContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNewRuleFromHereToolStripMenuItem});
            this.tlbTreeContextMenuStrip.Name = "tlbTreeContextMenuStrip";
            this.tlbTreeContextMenuStrip.Size = new System.Drawing.Size(230, 26);
            // 
            // addNewRuleFromHereToolStripMenuItem
            // 
            this.addNewRuleFromHereToolStripMenuItem.Name = "addNewRuleFromHereToolStripMenuItem";
            this.addNewRuleFromHereToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this.addNewRuleFromHereToolStripMenuItem.Text = "Add New Rule From Here...";
            this.addNewRuleFromHereToolStripMenuItem.Click += new System.EventHandler(this.addNewRuleFromHereToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(171, 22);
            this.toolStripMenuItem1.Text = "Save rule file as...";
            // 
            // treeSplitContainer
            // 
            this.treeSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeSplitContainer.Location = new System.Drawing.Point(0, 49);
            this.treeSplitContainer.Name = "treeSplitContainer";
            // 
            // treeSplitContainer.Panel1
            // 
            this.treeSplitContainer.Panel1.AutoScroll = true;
            this.treeSplitContainer.Panel1.Controls.Add(this.groupBox1);
            // 
            // treeSplitContainer.Panel2
            // 
            this.treeSplitContainer.Panel2.AutoScroll = true;
            this.treeSplitContainer.Panel2.Controls.Add(this.groupBox2);
            this.treeSplitContainer.Size = new System.Drawing.Size(880, 433);
            this.treeSplitContainer.SplitterDistance = 246;
            this.treeSplitContainer.SplitterWidth = 5;
            this.treeSplitContainer.TabIndex = 6;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tlbTreeView);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(246, 433);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Type Library";
            // 
            // tlbTreeView
            // 
            this.tlbTreeView.AllowDrop = true;
            this.tlbTreeView.DisplayLevel = TypeLibraryTreeView.DisplayLevel.All;
            this.tlbTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlbTreeView.HideSelection = false;
            this.tlbTreeView.ImageIndex = 0;
            this.tlbTreeView.Location = new System.Drawing.Point(3, 17);
            this.tlbTreeView.Name = "tlbTreeView";
            this.tlbTreeView.SelectedImageIndex = 0;
            this.tlbTreeView.Size = new System.Drawing.Size(240, 413);
            this.tlbTreeView.TabIndex = 0;
            this.tlbTreeView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tlbTreeView_MouseDoubleClick);
            this.tlbTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tlbTreeView_AfterSelect);
            this.tlbTreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tlbTreeView_MouseDown);
            this.tlbTreeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.tlbTreeView_ItemDrag);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.splitContainerRuleFile);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(629, 433);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Config File";
            // 
            // splitContainerRuleFile
            // 
            this.splitContainerRuleFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerRuleFile.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerRuleFile.Location = new System.Drawing.Point(3, 17);
            this.splitContainerRuleFile.Name = "splitContainerRuleFile";
            this.splitContainerRuleFile.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerRuleFile.Panel1
            // 
            this.splitContainerRuleFile.Panel1.Controls.Add(this.ruleTreeView);
            this.splitContainerRuleFile.Panel1MinSize = 20;
            // 
            // splitContainerRuleFile.Panel2
            // 
            this.splitContainerRuleFile.Panel2.Controls.Add(this.splitContainerConditionExp);
            this.splitContainerRuleFile.Panel2MinSize = 20;
            this.splitContainerRuleFile.Size = new System.Drawing.Size(623, 413);
            this.splitContainerRuleFile.SplitterDistance = 371;
            this.splitContainerRuleFile.SplitterWidth = 1;
            this.splitContainerRuleFile.TabIndex = 2;
            // 
            // splitContainerConditionExp
            // 
            this.splitContainerConditionExp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerConditionExp.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerConditionExp.IsSplitterFixed = true;
            this.splitContainerConditionExp.Location = new System.Drawing.Point(0, 0);
            this.splitContainerConditionExp.Name = "splitContainerConditionExp";
            this.splitContainerConditionExp.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerConditionExp.Panel1
            // 
            this.splitContainerConditionExp.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(205)))), ((int)(((byte)(219)))));
            this.splitContainerConditionExp.Panel1.Controls.Add(this.imageCloseButton);
            this.splitContainerConditionExp.Panel1.Controls.Add(this.label1);
            this.splitContainerConditionExp.Panel1.Click += new System.EventHandler(this.splitContainerConditionExp_Panel1_Click);
            this.splitContainerConditionExp.Panel1MinSize = 20;
            // 
            // splitContainerConditionExp.Panel2
            // 
            this.splitContainerConditionExp.Panel2.Controls.Add(this.richTextBoxConditionExp);
            this.splitContainerConditionExp.Panel2MinSize = 20;
            this.splitContainerConditionExp.Size = new System.Drawing.Size(623, 41);
            this.splitContainerConditionExp.SplitterDistance = 20;
            this.splitContainerConditionExp.SplitterWidth = 1;
            this.splitContainerConditionExp.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(127, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Condition Expression";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // ruleTreeView
            // 
            this.ruleTreeView.AllowDrop = true;
            this.ruleTreeView.Controls.Add(this.conditionInPlaceEditor);
            this.ruleTreeView.Controls.Add(this.insertionHighlight);
            this.ruleTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ruleTreeView.HideSelection = false;
            this.ruleTreeView.ImageIndex = 0;
            this.ruleTreeView.ImageList = this.ruleIconList;
            this.ruleTreeView.LabelEdit = true;
            this.ruleTreeView.Location = new System.Drawing.Point(0, 0);
            this.ruleTreeView.Name = "ruleTreeView";
            this.ruleTreeView.SelectedImageIndex = 0;
            this.ruleTreeView.Size = new System.Drawing.Size(623, 371);
            this.ruleTreeView.TabIndex = 0;
            this.ruleTreeView.Scroll += new TlbImpRuleFileEditor.RuleTreeView.ScrollEventHandler(this.ruleTreeView_Scroll);
            this.ruleTreeView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ruleTreeView_MouseClick);
            this.ruleTreeView.SizeChanged += new System.EventHandler(this.ruleTreeView_SizeChanged);
            this.ruleTreeView.DragLeave += new System.EventHandler(this.ruleTreeView_DragLeave);
            this.ruleTreeView.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.ruleTreeView_AfterLabelEdit);
            this.ruleTreeView.DoubleClick += new System.EventHandler(this.ruleTreeView_DoubleClick);
            this.ruleTreeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.ruleTreeView_DragDrop);
            this.ruleTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ruleTreeView_AfterSelect);
            this.ruleTreeView.Leave += new System.EventHandler(this.ruleTreeView_Leave);
            this.ruleTreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ruleTreeView_MouseDown);
            this.ruleTreeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.ruleTreeView_DragEnter);
            this.ruleTreeView.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.ruleTreeView_BeforeLabelEdit);
            this.ruleTreeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ruleTreeView_KeyDown);
            this.ruleTreeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.ruleTreeView_ItemDrag);
            this.ruleTreeView.DragOver += new System.Windows.Forms.DragEventHandler(this.ruleTreeView_DragOver);
            // 
            // conditionInPlaceEditor
            // 
            this.conditionInPlaceEditor.AutoSize = true;
            this.conditionInPlaceEditor.IsProcesssed = false;
            this.conditionInPlaceEditor.Location = new System.Drawing.Point(128, 57);
            this.conditionInPlaceEditor.Margin = new System.Windows.Forms.Padding(0);
            this.conditionInPlaceEditor.Name = "conditionInPlaceEditor";
            this.conditionInPlaceEditor.Size = new System.Drawing.Size(443, 24);
            this.conditionInPlaceEditor.TabIndex = 1;
            this.conditionInPlaceEditor.Visible = false;
            this.conditionInPlaceEditor.VisibleChanged += new System.EventHandler(this.conditionInPlaceEditor_VisibleChanged);
            // 
            // insertionHighlight
            // 
            this.insertionHighlight.Location = new System.Drawing.Point(244, 147);
            this.insertionHighlight.Name = "insertionHighlight";
            this.insertionHighlight.Size = new System.Drawing.Size(47, 2);
            this.insertionHighlight.TabIndex = 1;
            this.insertionHighlight.Visible = false;
            // 
            // imageCloseButton
            // 
            this.imageCloseButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.imageCloseButton.Image = global::TlbImpRuleFileEditor.Properties.Resources.close_normal;
            this.imageCloseButton.ImageDown = global::TlbImpRuleFileEditor.Properties.Resources.close_down;
            this.imageCloseButton.ImageHover = global::TlbImpRuleFileEditor.Properties.Resources.close_hover;
            this.imageCloseButton.ImageNormal = global::TlbImpRuleFileEditor.Properties.Resources.close_normal;
            this.imageCloseButton.Location = new System.Drawing.Point(602, 0);
            this.imageCloseButton.Name = "imageCloseButton";
            this.imageCloseButton.Size = new System.Drawing.Size(21, 20);
            this.imageCloseButton.TabIndex = 2;
            this.imageCloseButton.TabStop = false;
            this.imageCloseButton.Click += new System.EventHandler(this.imageCloseButton_Click);
            // 
            // richTextBoxConditionExp
            // 
            this.richTextBoxConditionExp.BackColor = System.Drawing.SystemColors.Info;
            this.richTextBoxConditionExp.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxConditionExp.DetectUrls = false;
            this.richTextBoxConditionExp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxConditionExp.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxConditionExp.Name = "richTextBoxConditionExp";
            this.richTextBoxConditionExp.ReadOnly = true;
            this.richTextBoxConditionExp.Size = new System.Drawing.Size(623, 20);
            this.richTextBoxConditionExp.TabIndex = 0;
            this.richTextBoxConditionExp.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(880, 482);
            this.Controls.Add(this.treeSplitContainer);
            this.Controls.Add(this.systemToolStrip);
            this.Controls.Add(this.mainMenuStrip);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "MainForm";
            this.Text = "TlbImp Config File Editor";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.ruleSetContextMenuStrip.ResumeLayout(false);
            this.ruleContextMenuStrip.ResumeLayout(false);
            this.actionContextMenuStrip.ResumeLayout(false);
            this.compositeConditionContextMenuStrip.ResumeLayout(false);
            this.atomicConditionContextMenuStrip.ResumeLayout(false);
            this.systemToolStrip.ResumeLayout(false);
            this.systemToolStrip.PerformLayout();
            this.tlbTreeContextMenuStrip.ResumeLayout(false);
            this.treeSplitContainer.Panel1.ResumeLayout(false);
            this.treeSplitContainer.Panel2.ResumeLayout(false);
            this.treeSplitContainer.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.splitContainerRuleFile.Panel1.ResumeLayout(false);
            this.splitContainerRuleFile.Panel2.ResumeLayout(false);
            this.splitContainerRuleFile.ResumeLayout(false);
            this.splitContainerConditionExp.Panel1.ResumeLayout(false);
            this.splitContainerConditionExp.Panel1.PerformLayout();
            this.splitContainerConditionExp.Panel2.ResumeLayout(false);
            this.splitContainerConditionExp.ResumeLayout(false);
            this.ruleTreeView.ResumeLayout(false);
            this.ruleTreeView.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageCloseButton)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem filesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openTypeLibraryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveRuleFileToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openTlbFileDialog;
        private System.Windows.Forms.SaveFileDialog saveRuleFileDialog;
        private System.Windows.Forms.ToolStripMenuItem openRuleFileToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openRuleFileDialog;
        private System.Windows.Forms.ContextMenuStrip ruleSetContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem addNewRuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeAllRulesToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip ruleContextMenuStrip;
        private System.Windows.Forms.ContextMenuStrip actionContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem modifyActionToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip compositeConditionContextMenuStrip;
        private System.Windows.Forms.ContextMenuStrip atomicConditionContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem removeAllSubconditionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeConditionToolStripMenuItem_SingleCondition;
        private System.Windows.Forms.ToolStripMenuItem removeRuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeConditionToolStripMenuItem_MultiCondition;
        private System.Windows.Forms.SplitContainer treeSplitContainer;
        private TypeLibraryTreeView.TlbTreeView tlbTreeView;
        private RuleTreeView ruleTreeView;
        private System.Windows.Forms.ToolStrip systemToolStrip;
        private System.Windows.Forms.ToolStripButton openTlbToolStripButton;
        private System.Windows.Forms.ToolStripButton openRuleFileToolStripButton;
        private System.Windows.Forms.ImageList ruleIconList;
        private System.Windows.Forms.ContextMenuStrip tlbTreeContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem addNewRuleFromHereToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton saveRuleFileToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem newRuleFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton newRuleFileToolStripButton;
        private System.Windows.Forms.ToolStripButton saveRuleFileAsToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveRuleFileAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNewRuleEditToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeAllRulesEditToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeRuleEditToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeConditionEditToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeAllSubconditionsEditToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem modifyActionEditToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutTlbImpRuleFileEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton addNewRuleToolStripButton;
        private System.Windows.Forms.ToolStripButton removeAllRulesToolStripButton;
        private System.Windows.Forms.ToolStripButton removeRuleToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripButton removeConditionToolStripButton;
        private System.Windows.Forms.ToolStripButton removeAllSubconditionsToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripButton modifyActionToolStripButton;
        private ConditionInPlaceEditor conditionInPlaceEditor;
        private RichTextBoxConditionExpression richTextBoxConditionExp;
        private System.Windows.Forms.SplitContainer splitContainerConditionExp;
        private System.Windows.Forms.Label label1;
        private ImageCloseButton imageCloseButton;
        private InsertionHighlight insertionHighlight;
        private System.Windows.Forms.SplitContainer splitContainerRuleFile;
    }
}

