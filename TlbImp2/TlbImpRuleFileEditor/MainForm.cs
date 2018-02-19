using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TypeLibTypes.Interop;
using System.Threading;
using CoreRuleEngine;
using System.IO;
using TlbImpRuleEngine;
using System.Xml;
using TypeLibraryTreeView;

namespace TlbImpRuleFileEditor
{
    public partial class MainForm : Form
    {
        private string m_storedRuleFilePath;

        private bool m_isModified = false;

        const int CTRL = 8;

        public MainForm()
        {
            // Init RuleEngine
            RuleEngine.InitRuleEngine(new TlbImpActionManager(),
                                      new TlbImpCategoryManager(),
                                      new TlbImpConditionManager(),
                                      new TlbImpOperatorManager());
            InitializeComponent();
        }

        private void openTypeLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openTypeLibraryOperation();
        }

        private void openTypeLibraryOperation()
        {
            if (openTlbFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.Cursor = Cursors.WaitCursor;
                LoadTlb(openTlbFileDialog.FileName);
                UpdateToolBar();
                this.Cursor = Cursors.Default;
            }
        }
        private void openRuleFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openRuleFileOperation();
        }

        private void openRuleFileOperation()
        {
            conditionInPlaceEditor.Visible = false;
            if (SaveExistingRuleSet())
            {
                if (openRuleFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadRuleFile(openRuleFileDialog.FileName);
                    UpdateToolBar();
                }
            }
        }

        /// <summary>
        /// Save existing rule set before operations including 'New Rule File', 'Open Rule File',...
        /// </summary>
        /// <returns>
        /// If user click "Cancel", Return false, to indicate that this closing operation is canceled.
        /// If user click "No", Return true, to indicate user wants to close without saving.
        /// If user click "Yes", do the save rule file operation,
        ///     and return the saveRuleFileOperation's return value.
        /// </returns>
        private bool SaveExistingRuleSet()
        {
            if (HasRuleSet() && m_isModified)
            {
                DialogResult response = MessageBox.Show(
                    Resource.FormatString("Msg_SaveFileQuestion"),
                    Resource.FormatString("Msg_SaveFileQuestionTitle"),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button3);
                if (response == DialogResult.Cancel)
                {
                    return false;
                }
                else if (response == DialogResult.Yes)
                {
                    return saveRuleFileOperation();
                }
            }
            return true;
        }

        private void saveRuleFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveRuleFileOperation();
        }

        /// <summary>
        /// If the save rule file is done successfully, return true, else return false.
        /// </summary>
        /// <returns></returns>
        private bool saveRuleFileOperation()
        {
            if (HasRuleSet())
            {
                RuleSet ruleSet = ruleTreeView.Nodes[0].Tag as RuleSet;
                if (!CheckRuleSet(ruleSet))
                {
                    return false;
                }
                string storedRuleFilePath = m_storedRuleFilePath;
                if (storedRuleFilePath == null)
                {
                    if (saveRuleFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        storedRuleFilePath = saveRuleFileDialog.FileName;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (SaveRuleFile(storedRuleFilePath))
                {
                    m_storedRuleFilePath = storedRuleFilePath;
                    SetModified(false);
                    return true;
                }
            }
            else
            {
                MessageBox.Show(Resource.FormatString("Msg_NoRuleFileToSave"));
            }
            return false;
        }

        private bool CheckRuleSet(RuleSet ruleSet)
        {
            List<Rule> rules = ruleSet.GetAllRules();
            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i].Action == null || !rules[i].Action.IsInitialized)
                {
                    MessageBox.Show(Resource.FormatString("Wrn_ActionUninitialized", rules[i].Name));
                    return false;
                }
                if (!CheckCondition(rules[i].Condition))
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckCondition(ICondition condition)
        {
            if (condition is AbstractCompositeCondition)
            {
                AbstractCompositeCondition compositeCondition = condition as AbstractCompositeCondition;
                List<ICondition> list = compositeCondition.ConditionList;
                if (list.Count == 0)
                {
                    MessageBox.Show(Resource.FormatString("Wrn_EmptyCompositeCondition",
                                                          condition.GetConditionDef().GetConditionName()));
                    return false;
                }
                for (int i = 0; i < list.Count; i++)
                {
                    if (!CheckCondition(list[i]))
                        return false;
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        private bool SaveRuleFile(string ruleFilePath)
        {
            try
            {
                RuleSet ruleSet = ruleTreeView.Nodes[0].Tag as RuleSet;
                RuleFileWriter ruleFileWriter = new RuleFileWriter(ruleSet);
                XmlDocument doc = ruleFileWriter.WriteToXmlDocument();
                XmlTextWriter xmlTextWriter = new XmlTextWriter(ruleFilePath, null);
                xmlTextWriter.Formatting = Formatting.Indented;

                doc.WriteContentTo(xmlTextWriter);
                xmlTextWriter.Close();
                return true;
            }
            catch (ActionUninitializedException ex)
            {
                MessageBox.Show(Resource.FormatString("Wrn_ActionUninitialized", ex.RuleName));
                return false;
            }
            catch (Exception)
            {
                MessageBox.Show(Resource.FormatString("Wrn_SaveRuleFileFailed"));
                return false;
            }
        }

        private bool HasRuleSet()
        {
            return ruleTreeView.Nodes.Count != 0 && ruleTreeView.Nodes[0].Tag != null &&
                            ruleTreeView.Nodes[0].Tag is RuleSet;
        }

        private void LoadTlb(string tlbFileName)
        {
            try
            {
                // Load the typelib.
                System.Runtime.InteropServices.ComTypes.ITypeLib TypeLib = null;
                APIHelper.LoadTypeLibEx(tlbFileName, REGKIND.REGKIND_DEFAULT, out TypeLib);

                // Update the tlbTreeView.
                TypeLib tlb = new TypeLib((ITypeLib)TypeLib);
                tlbTreeView.SetTypeLibrary(tlb);
            }
            catch (Exception)
            {
                MessageBox.Show(Resource.FormatString("Wrn_TypeLibLoadFailed", tlbFileName));
            }
        }

        private void LoadRuleFile(string ruleFileName)
        {
            try
            {
                RuleFileParser ruleFileParser = new RuleFileParser(ruleFileName);
                RuleSet ruleSet = ruleFileParser.Parse();

                TreeNode ruleTreeRoot = RuleSet2TreeNodeProcessor.GetRuleSetTreeNode(ruleSet);

                ruleTreeRoot.Text = "Rules";
                ruleTreeView.BeginUpdate();
                ruleTreeView.Nodes.Clear();
                ruleTreeView.Nodes.Add(ruleTreeRoot);
                ruleTreeRoot.Expand();
                ruleTreeView.EndUpdate();

                m_storedRuleFilePath = ruleFileName;
                SetModified(false);
            }
            catch (Exception)
            {
                MessageBox.Show(Resource.FormatString("Wrn_LoadRuleFileFailed", ruleFileName));
            }
        }

        private void UpdateDisplayedFileName()
        {
            if (m_storedRuleFilePath != null)
            {
                this.Text = Resource.FormatString("Msg_MainFormTitle") +
                    " - " + m_storedRuleFilePath;
                ruleTreeView.Nodes[0].Text = Path.GetFileName(m_storedRuleFilePath);
            }
            else
            {
                this.Text = Resource.FormatString("Msg_MainFormTitle") + 
                    " - " + TreeNodeConstants.Untitled;
                ruleTreeView.Nodes[0].Text = TreeNodeConstants.Untitled;
            }
            if (m_isModified)
                this.Text += "*";
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SaveExistingRuleSet())
            {
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void tlbTreeView_MouseDown(object sender, MouseEventArgs e)
        {
            TreeNode node = tlbTreeView.GetNodeAt(e.X, e.Y);
            if (node != null && node.Bounds.Contains(e.X, e.Y))
            {
                tlbTreeView.SelectedNode = node;
                if (e.Button == MouseButtons.Right)
                {
                    if (node.Parent != null)
                    {
                        node.ContextMenuStrip = tlbTreeContextMenuStrip;
                    }
                }
            }
        }

        private void UpdateToolBar()
        {
            DisableToolBarButtons();
            // rule file tool buttons
            if (ruleTreeView.Focused &&
                ruleTreeView.SelectedNode != null && ruleTreeView.SelectedNode.Tag != null)
            {
                object nodeTag = ruleTreeView.SelectedNode.Tag;
                if (nodeTag is Rule)
                {
                    removeRuleToolStripButton.Enabled = true;
                }
                else if (this.IsRootConditionTreeNode(ruleTreeView.SelectedNode))
                {
                }
                else if (nodeTag is ICondition)
                {
                    if (nodeTag is AbstractCompositeCondition)
                    {
                        removeAllSubconditionsToolStripButton.Enabled = true;
                    }
                    // first Add condition
                    if (ruleTreeView.SelectedNode.Parent != null &&
                    this.IsRootConditionTreeNode(ruleTreeView.SelectedNode.Parent))
                        removeConditionToolStripButton.Enabled = false;
                    else
                        removeConditionToolStripButton.Enabled = true;
                }
                else if (nodeTag is IAction)
                {
                    modifyActionToolStripButton.Enabled = true;
                }
            }
        }

        private void DisableToolBarButtons()
        {
            removeRuleToolStripButton.Enabled = false;
            removeConditionToolStripButton.Enabled = false;
            removeAllSubconditionsToolStripButton.Enabled = false;
            modifyActionToolStripButton.Enabled = false;
        }

        private void ruleTreeView_MouseDown(object sender, MouseEventArgs e)
        {
            // clean the previous context menu strip first.
            ruleTreeView.ContextMenuStrip = null;
            conditionInPlaceEditor.Visible = false;
            TreeNode node = ruleTreeView.GetNodeAt(e.X, e.Y);
            if (node != null)
            {
                // clean the previous context menu strip first.
                node.ContextMenuStrip = null;
                if (node.Bounds.Contains(e.X, e.Y))
                {
                    ruleTreeView.SelectedNode = node;
                    if (e.Button == MouseButtons.Right)
                    {
                        node.ContextMenuStrip = GetContextMenuStrip(node);
                    }
                }
                else
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        // Right click on the empty place open the default context menu.
                        ruleTreeView.ContextMenuStrip = ruleSetContextMenuStrip;
                    }
                }
            }
            else
            {
                // if no node is selected.
                if (e.Button == MouseButtons.Right)
                {
                    // Right click on the empty place open the default context menu.
                    ruleTreeView.ContextMenuStrip = ruleSetContextMenuStrip;
                }
            }
        }

        private bool IsEmptyConditionNode(TreeNode treeNode)
        {
            return treeNode.Tag is string &&
                treeNode.Tag.Equals(TreeNodeConstants.EmptyCondition);
        }

        private bool IsRootConditionTreeNode(TreeNode treeNode)
        {
            return treeNode.Tag is string &&
                treeNode.Tag.Equals(TreeNodeConstants.Condition);
        }

        private ContextMenuStrip GetContextMenuStrip(TreeNode treeNode)
        {
            object nodeTag = treeNode.Tag;
            if (nodeTag is RuleSet)
            {
                return ruleSetContextMenuStrip;
            }
            else if (nodeTag is Rule)
            {
                return ruleContextMenuStrip;
            }
            else if (this.IsRootConditionTreeNode(treeNode))
            {
                return null;
            }
            else if (nodeTag is ICondition)
            {
                if (nodeTag is AbstractCompositeCondition)
                {
                    // The first Add condition.
                    if (treeNode.Parent != null &&
                        this.IsRootConditionTreeNode(treeNode.Parent))
                        removeConditionToolStripMenuItem_MultiCondition.Enabled = false;
                    else
                        removeConditionToolStripMenuItem_MultiCondition.Enabled = true;
                    return compositeConditionContextMenuStrip;
                }
                else
                {
                    return atomicConditionContextMenuStrip;
                }
            }
            else if (nodeTag is IAction)
            {
                return actionContextMenuStrip;
            }
            else
            {
                return null;
            }
        }

        private void removeAllRulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            removeAllRulesOperation();
        }

        private void removeAllRulesOperation()
        {
            TreeNode ruleTreeRoot = ruleTreeView.Nodes[0];
            if (ruleTreeRoot != null && ruleTreeRoot.Nodes.Count == 0)
                return;
            DialogResult response = MessageBox.Show(
                    Resource.FormatString("Msg_RemoveAllRuleQuestion"),
                    Resource.FormatString("Msg_RemoveAllRuleTitle"),
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
            if (response != DialogResult.OK)
            {
                return;
            }
            TreeNode ruleSetNode = ruleTreeView.Nodes[0];
            ruleTreeView.SelectedNode = ruleSetNode;
            ruleSetNode.Nodes.Clear();
            RuleSet ruleSet = ruleSetNode.Tag as RuleSet;
            ruleSet.RemoveAllRules();
            SetModified(true);
        }

        private void addNewRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addNewRuleOperation();
        }

        private void addNewRuleOperation()
        {
            RuleForm form = new RuleForm("Rule #" + (ruleTreeView.Nodes[0].Nodes.Count + 1));
            if (form.ShowDialog() == DialogResult.OK)
            {
                ICategory category = form.GetCategory();
                IAction action = form.GetAction();
                Rule newRule = new Rule(category, form.GetRuleName());
                newRule.Action = action;
                newRule.Condition = new AndCondition();
                TreeNode ruleSetNode = ruleTreeView.Nodes[0];
                RuleSet ruleSet = ruleSetNode.Tag as RuleSet;
                ruleSet.AddRule(newRule);
                // Add tree node.
                TreeNode ruleNode = RuleSet2TreeNodeProcessor.GetRuleTreeNode(newRule);
                ruleNode.ExpandAll();
                ruleSetNode.Nodes.Add(ruleNode);
                SetModified(true);
            }
        }

        private void removeRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            removeRuleOperation();
        }

        private void removeRuleOperation()
        {
            DialogResult response = MessageBox.Show(
                    Resource.FormatString("Msg_RemoveRuleQuestion",
                                          (ruleTreeView.SelectedNode.Tag as Rule).Name),
                    Resource.FormatString("Msg_RemoveRuleTitle"),
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
            if (response != DialogResult.OK)
            {
                return;
            }
            RuleSet ruleSet = ruleTreeView.SelectedNode.Parent.Tag as RuleSet;
            ruleSet.RemoveRule(ruleTreeView.SelectedNode.Tag as Rule);
            ruleTreeView.SelectedNode.Remove();
            SetModified(true);
        }

        private void modifyActionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            modifyActionOperation();
        }

        private void modifyActionOperation()
        {
            TreeNode ruleNode = GetRuleNode(ruleTreeView.SelectedNode);
            Rule rule = ruleNode.Tag as Rule;
            IAction action = rule.Action;
            bool updated = false;
            if (action is ChangeManagedNameAction)
            {
                updated = DoChangeManagedNameActionDialog(action as ChangeManagedNameAction);
            }
            else if (action is ResolveToAction)
            {
                updated = DoResolveToActionWizard(action as ResolveToAction);
            }
            else if (action is AddAttributeAction)
            {
                updated = DoAddAttributeActionWizard(action as AddAttributeAction);
            }
            else if (action is ConvertToAction)
            {
                updated = DoConvertToActionWizard(action as ConvertToAction);
            }
            else
            {
                MessageBox.Show(Resource.FormatString("Msg_NoParametersToEdit"));
            }
            if (updated)
            {
                ruleNode.Nodes[ruleNode.Nodes.Count - 1].Remove();
                TreeNode newActionNode = RuleSet2TreeNodeProcessor.GetActionTreeNode(action);
                newActionNode.ExpandAll();
                ruleNode.Nodes.Add(newActionNode);
                SetModified(true);
            }
        }

        private bool DoChangeManagedNameActionDialog(ChangeManagedNameAction action)
        {
            ChangeManagedNameActionDialog changeManagedNameActionDialog;
            if (action != null && action.IsInitialized)
                changeManagedNameActionDialog =
                    new ChangeManagedNameActionDialog(action);
            else
                changeManagedNameActionDialog = new ChangeManagedNameActionDialog();
            DialogResult dialogResult = changeManagedNameActionDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                action.NewName = changeManagedNameActionDialog.NewName;
                action.IsInitialized = true;
                return true;
            }
            return false;
        }

        private bool DoAddAttributeActionWizard(AddAttributeAction action)
        {
            AddAttributeActionWizard addAttributeActionWizard;
            if (action != null && action.IsInitialized)
                addAttributeActionWizard =
                    new AddAttributeActionWizard(action);
            else
                addAttributeActionWizard = new AddAttributeActionWizard();
            DialogResult dialogResult = addAttributeActionWizard.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                action.AssemblyName = addAttributeActionWizard.SelectedAssembly;
                action.TypeName = addAttributeActionWizard.SelectedAttribute;
                action.Constructor = addAttributeActionWizard.Constructor;
                action.Data = addAttributeActionWizard.AttributeValue;
                action.IsInitialized = true;
                return true;
            }
            return false;
        }

        private bool DoResolveToActionWizard(ResolveToAction action)
        {
            ResolveToActionWizard resolveToActionWizard;
            if (action != null && action.IsInitialized)
                resolveToActionWizard = new ResolveToActionWizard(action);
            else
                resolveToActionWizard = new ResolveToActionWizard();
            DialogResult dialogResult = resolveToActionWizard.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                action.AssemblyName = resolveToActionWizard.ManagedAssemblyName;
                action.ManagedTypeFullName = resolveToActionWizard.ManagedTypeFullName;
                action.IsInitialized = true;
                return true;
            }
            return false;
        }

        private bool DoConvertToActionWizard(ConvertToAction action)
        {
            ConvertToActionWizard convertToActionWizard;
            if (action != null && action.IsInitialized)
                convertToActionWizard = new ConvertToActionWizard(action);
            else
                convertToActionWizard = new ConvertToActionWizard();
            DialogResult dialogResult = convertToActionWizard.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                action.Direction = convertToActionWizard.Direction;
                action.ByRef = Boolean.Parse(convertToActionWizard.ByRef.ToString());
                action.ManagedTypeConvertTo = convertToActionWizard.ManagedType;
                action.UnmanagedTypeMarshalAs = convertToActionWizard.MarshalAs;
                action.Attributes = convertToActionWizard.Attributes;
                action.IsInitialized = true;
                return true;
            }
            return false;
        }

        private void removeConditionToolStripMenuItem_SingleCondition_Click(object sender,
            EventArgs e)
        {
            removeConditionOperation(ruleTreeView.SelectedNode, true);
        }

        private void removeConditionOperation(TreeNode treeNode, bool askFirst)
        {
            if (askFirst)
            {
                DialogResult response = MessageBox.Show(
                        Resource.FormatString("Msg_RemoveConditionQuestion",
                                              (treeNode.Tag as ICondition).GetConditionDef().GetConditionName()),
                        Resource.FormatString("Msg_RemoveConditionTitle"),
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                if (response != DialogResult.OK)
                {
                    return;
                }
            }

            AbstractCompositeCondition compositeCondition =
                    treeNode.Parent.Tag as AbstractCompositeCondition;
            compositeCondition.RemoveConditionAt(treeNode.Index);

            if (compositeCondition is AbstractSingleCondition)
            {
                // Add Empty Condition.
                treeNode.Parent.Nodes.Add(
                    RuleSet2TreeNodeProcessor.GetEmptyConditionTreeNode());
            }
            // Remove the tree node.
            treeNode.Remove();
            SetModified(true);
        }

        private void removeConditionToolStripMenuItem_MultiCondition_Click(object sender,
            EventArgs e)
        {
            removeConditionOperation(ruleTreeView.SelectedNode, true);
        }

        private void removeAllSubconditionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            removeAllSubconditionsOperation();
        }

        private void removeAllSubconditionsOperation()
        {
            AbstractCompositeCondition compositeCondition =
                ruleTreeView.SelectedNode.Tag as AbstractCompositeCondition;
            if (compositeCondition.ConditionList.Count == 0)
                return;
            DialogResult response = MessageBox.Show(
                    Resource.FormatString("Msg_RemoveAllSubconditionQuestion",
                                          compositeCondition.GetConditionDef().GetConditionName()),
                    Resource.FormatString("Msg_RemoveAllSubconditionTitle"),
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
            if (response != DialogResult.OK)
            {
                return;
            }
            compositeCondition.RemoveAllCondition();
            ruleTreeView.SelectedNode.Nodes.Clear();
            ruleTreeView.SelectedNode.Nodes.Add(
                RuleSet2TreeNodeProcessor.GetEmptyConditionTreeNode());
            SetModified(true);
        }

        private TreeNode GetRuleNode(TreeNode treeNode)
        {
            while (treeNode != null)
            {
                if (treeNode.Tag is Rule)
                {
                    return treeNode;
                }
                treeNode = treeNode.Parent;
            }
            return null;
        }

        private void addNewRuleFromHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IMatchTarget matchTarget = tlbTreeView.SelectedNode.Tag as IMatchTarget;
            addNewRuleFromHereOperation(matchTarget);
        }

        private void addNewRuleFromHereOperation(IMatchTarget matchTarget)
        {
            if (!HasRuleSet())
            {
                MessageBox.Show(Resource.FormatString("Wrn_NoRuleFileToAddRuleTo"));
                return;
            }
            ICategory fixedCategory = matchTarget.GetCategory();
            RuleForm form = new RuleForm(fixedCategory,
                "Rule #" + (ruleTreeView.Nodes[0].Nodes.Count + 1));
            if (form.ShowDialog() == DialogResult.OK)
            {
                IAction action = form.GetAction();
                AndCondition andCondition = ExtractDefaultConditions(matchTarget);
                Rule newRule = new Rule(fixedCategory, action, andCondition, form.GetRuleName());
                RuleSet ruleSet = ruleTreeView.Nodes[0].Tag as RuleSet;
                ruleSet.AddRule(newRule);
                // Add tree node.
                TreeNode ruleNode = RuleSet2TreeNodeProcessor.GetRuleTreeNode(newRule);
                ruleNode.ExpandAll();
                ruleTreeView.Nodes[0].Nodes.Add(ruleNode);
                // set modified
                SetModified(true);
            }
        }

        private bool CanAddCondition(TreeNode node)
        {
            if (node.Tag is AbstractCompositeCondition)
            {
                AbstractCompositeCondition compositeCondition = node.Tag as AbstractCompositeCondition;
                AbstractCompositeConditionDef compositeConditionDef =
                    compositeCondition.GetConditionDef() as AbstractCompositeConditionDef;
                return compositeCondition.ConditionList.Count <
                    compositeConditionDef.GetMaxSubconditionNumber();
            }
            return false;
        }

        private AndCondition ExtractDefaultConditions(IMatchTarget matchTarget)
        {
            ICategory fixedCategory = matchTarget.GetCategory();
            AndCondition andCondition =
                AndConditionDef.GetInstance().Create() as AndCondition;
            if (fixedCategory == SignatureCategory.GetInstance())
            {
                NativeParentFunctionNameCondition nativeParentFunctionNameCondition =
                    GetNativeParentFunctionNameCondition(matchTarget);
                andCondition.AppendCondition(nativeParentFunctionNameCondition);
                NativeParameterIndexCondition parameterIndexCondition =
                    GetNativeParameterIndexCondition(matchTarget as SignatureInfoMatchTarget);
                andCondition.AppendCondition(parameterIndexCondition);
            }
            else
            {
                // Add conditions. Currently, only the name condition is extracted.
                NativeNameCondition nativeNameCondition = GetNativeNameCondition(matchTarget);
                if (nativeNameCondition != null)
                    andCondition.AppendCondition(nativeNameCondition);
            }
            return andCondition;
        }

        private NativeParameterIndexCondition GetNativeParameterIndexCondition(
            SignatureInfoMatchTarget signatureInfoMatchTarget)
        {
            NativeParameterIndexConditionDef parameterIndexConditionDef =
                NativeParameterIndexConditionDef.GetInstance();
            NativeParameterIndexCondition parameterIndexCondition =
                parameterIndexConditionDef.Create() as NativeParameterIndexCondition;
            parameterIndexCondition.Operator = EqualOperator.GetInstance();
            parameterIndexCondition.Value = "" + signatureInfoMatchTarget.NativeParameterIndex;
            return parameterIndexCondition;
        }

        private NativeParentFunctionNameCondition GetNativeParentFunctionNameCondition(
            IMatchTarget matchTarget)
        {
            NativeParentFunctionNameConditionDef nativeParentFunctionNameConditionDef =
                NativeParentFunctionNameConditionDef.GetInstance();
            if (nativeParentFunctionNameConditionDef.CanApplyToCategory(matchTarget.GetCategory()) &&
                matchTarget is IGetNativeParentName)
            {
                NativeParentFunctionNameCondition nativeParentFunctionNameCondition =
                    nativeParentFunctionNameConditionDef.Create() as NativeParentFunctionNameCondition;
                nativeParentFunctionNameCondition.Operator = EqualOperator.GetInstance();
                nativeParentFunctionNameCondition.Value =
                    (matchTarget as IGetNativeParentName).GetNativeParentName();
                return nativeParentFunctionNameCondition;
            }
            else
            {
                return null;
            }
        }

        private void SetModified(bool isModified)
        {
            m_isModified = isModified;
            UpdateDisplayedFileName();
            UpdateConditionExpressionPanel();
        }

        private static NativeNameCondition GetNativeNameCondition(IMatchTarget matchTarget)
        {
            NativeNameConditionDef nativeNameConditionDef = NativeNameConditionDef.GetInstance();
            if (nativeNameConditionDef.CanApplyToCategory(matchTarget.GetCategory()))
            {
                NativeNameCondition nativeNameCondition =
                    nativeNameConditionDef.Create() as NativeNameCondition;
                nativeNameCondition.Operator = EqualOperator.GetInstance();
                if (matchTarget is IGetTypeLibElementCommonInfo)
                {
                    nativeNameCondition.Value = (matchTarget as IGetTypeLibElementCommonInfo).Name;
                    return nativeNameCondition;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private void openRuleFileToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                openRuleFileToolStripButton.Enabled = false;
                openRuleFileOperation();
            }
            finally
            {
                openRuleFileToolStripButton.Enabled = true;
            }
        }

        private void saveRuleFileToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                saveRuleFileToolStripButton.Enabled = false;
                saveRuleFileOperation();
            }
            finally
            {
                saveRuleFileToolStripButton.Enabled = true;
            }
        }

        private void openTlbToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                openTlbToolStripButton.Enabled = false;
                openTypeLibraryOperation();
            }
            finally
            {
                openTlbToolStripButton.Enabled = true;
            }
        }

        private void newRuleFileToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                newRuleFileToolStripButton.Enabled = false;
                newRuleFileOperation();
            }
            finally
            {
                newRuleFileToolStripButton.Enabled = true;
            }
        }

        private void newRuleFileOperation()
        {
            conditionInPlaceEditor.Visible = false;
            if (SaveExistingRuleSet())
            {
                RuleSet newRuleSet = new RuleSet();
                TreeNode ruleTreeRoot = RuleSet2TreeNodeProcessor.GetRuleSetTreeNode(newRuleSet);

                ruleTreeView.BeginUpdate();
                ruleTreeView.Nodes.Clear();
                ruleTreeView.Nodes.Add(ruleTreeRoot);
                ruleTreeRoot.Expand();
                ruleTreeView.EndUpdate();

                m_storedRuleFilePath = null;
                SetModified(false);
                UpdateToolBar();
            }
        }

        private void newRuleFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newRuleFileOperation();
        }

        private void saveRuleFileAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveRuleFileAsOperation();
        }

        private void saveRuleFileAsToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                saveRuleFileAsToolStripButton.Enabled = false;
                saveRuleFileAsOperation();
            }
            finally
            {
                saveRuleFileAsToolStripButton.Enabled = true;
            }
        }

        private bool saveRuleFileAsOperation()
        {
            if (HasRuleSet())
            {
                string storedRuleFilePath;
                if (saveRuleFileDialog.ShowDialog() == DialogResult.OK)
                {
                    storedRuleFilePath = saveRuleFileDialog.FileName;
                }
                else
                {
                    return false;
                }
                if (SaveRuleFile(storedRuleFilePath))
                {
                    m_storedRuleFilePath = storedRuleFilePath;
                    SetModified(false);
                    return true;
                }
            }
            else
            {
                MessageBox.Show(Resource.FormatString("Msg_NoRuleFileToSave"));
            }
            return false;
        }

        private void addNewRuleEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addNewRuleOperation();
        }

        private void removeAllRulesEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            removeAllRulesOperation();
        }

        private void removeRuleEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            removeRuleOperation();
        }

        private void removeConditionEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            removeConditionOperation(ruleTreeView.SelectedNode, true);
        }

        private void removeAllSubconditionsEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            removeAllSubconditionsOperation();
        }

        private void modifyActionEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            modifyActionOperation();
        }

        private void DisableEditMenuItems()
        {
            removeRuleEditToolStripMenuItem.Enabled = false;
            removeConditionEditToolStripMenuItem.Enabled = false;
            removeAllSubconditionsEditToolStripMenuItem.Enabled = false;
            modifyActionEditToolStripMenuItem.Enabled = false;
        }

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            DisableEditMenuItems();
            // rule file menu items
            if (ruleTreeView.Focused &&
                ruleTreeView.SelectedNode != null && ruleTreeView.SelectedNode.Tag != null)
            {
                object nodeTag = ruleTreeView.SelectedNode.Tag;
                if (nodeTag is Rule)
                {
                    removeRuleEditToolStripMenuItem.Enabled = true;
                }
                else if (this.IsRootConditionTreeNode(ruleTreeView.SelectedNode))
                {
                }
                else if (nodeTag is ICondition)
                {
                    if (nodeTag is AbstractCompositeCondition)
                    {
                        removeAllSubconditionsEditToolStripMenuItem.Enabled = true;
                    }
                    // first Add condition
                    if (ruleTreeView.SelectedNode.Parent != null &&
                    this.IsRootConditionTreeNode(ruleTreeView.SelectedNode.Parent))
                        removeConditionEditToolStripMenuItem.Enabled = false;
                    else
                        removeConditionEditToolStripMenuItem.Enabled = true;
                }
                else if (nodeTag is IAction)
                {
                    modifyActionEditToolStripMenuItem.Enabled = true;
                }
            }
        }

        private void addNewRuleFromHereEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IMatchTarget matchTarget = tlbTreeView.SelectedNode.Tag as IMatchTarget;
            addNewRuleFromHereOperation(matchTarget);
        }

        private void ruleTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            conditionInPlaceEditor.Visible = false;
            UpdateToolBar();
            UpdateConditionExpressionPanel();
        }

        private void UpdateConditionExpressionPanel()
        {
            TreeNode ruleNode = GetRuleNode(ruleTreeView.SelectedNode);
            if (ruleNode != null)
            {
                Rule selectedRule = ruleNode.Tag as Rule;
                richTextBoxConditionExp.UpdateText(selectedRule);
            }
            else
            {
                richTextBoxConditionExp.Clear();
            }
        }

        private void tlbTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateToolBar();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateToolBar();
            // new rule to start with.
            newRuleFileOperation();
            // let the form receive key events before the event is passed to the control that has focus.
            this.KeyPreview = true;
            // Add Default Tree View
            TreeNode root = TypeLib2TreeNodeProcessor.GetDefaultLibNode();
            root.Text = Resource.FormatString("Msg_TypeLibTreeRootDefaultText");
            TypeLib2TreeNodeProcessor.SetTlbTreeNodeImage(root);
            tlbTreeView.BeginUpdate();
            tlbTreeView.Nodes.Clear();
            tlbTreeView.Nodes.Add(root);
            tlbTreeView.EndUpdate();
        }

        private void addNewRuleToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                addNewRuleToolStripButton.Enabled = false;
                addNewRuleOperation();
            }
            finally
            {
                addNewRuleToolStripButton.Enabled = true;
            }
        }

        private void removeRuleToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                removeRuleToolStripButton.Enabled = false;
                removeRuleOperation();
            }
            finally
            {
                removeRuleToolStripButton.Enabled = true;
            }
        }

        private void removeAllRulesToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                removeAllRulesToolStripButton.Enabled = false;
                removeAllRulesOperation();
            }
            finally
            {
                removeAllRulesToolStripButton.Enabled = true;
            }
        }

        private void removeConditionToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                removeConditionToolStripButton.Enabled = false;
                removeConditionOperation(ruleTreeView.SelectedNode, true);
            }
            finally
            {
                removeConditionToolStripButton.Enabled = true;
            }
        }

        private void removeAllSubconditionsToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                removeAllSubconditionsToolStripButton.Enabled = false;
                removeAllSubconditionsOperation();
            }
            finally
            {
                removeAllSubconditionsToolStripButton.Enabled = true;
            }
        }

        private void modifyActionToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                modifyActionToolStripButton.Enabled = false;
                modifyActionOperation();
            }
            finally
            {
                modifyActionToolStripButton.Enabled = true;
            }
        }

        private void tlbTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TreeNode draggedNode = e.Item as TreeNode;
                if (draggedNode.Parent != null)
                    DoDragDrop(e.Item, DragDropEffects.Move);
            }
        }

        /// <summary>
        /// Make sure the insertionHighlight.Visible if false, after DragDrop.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ruleTreeView_DragDrop(object sender, DragEventArgs e)
        {
            Point targetPoint = ruleTreeView.PointToClient(new Point(e.X, e.Y));
            // Select the node at the mouse position.
            TreeNode node = ruleTreeView.GetNodeAt(targetPoint);
            if (node != null && !node.Bounds.Contains(targetPoint))
                node = null;
            // Retrieve the node that was dragged.
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
            if (draggedNode.Tag is IMatchTarget)
            {
                IMatchTarget matchTarget = draggedNode.Tag as IMatchTarget;
                if (node != null && (node.Tag is ICondition || IsEmptyConditionNode(node)))
                {
                    // Drag to the condition
                    addConditionToRuleFileTreeOperation(node, matchTarget);
                }
                else
                {
                    // Drag to the blank place or Rules node.
                    addNewRuleFromHereOperation(matchTarget);
                }
            }
            else
            {
                // ICondition
                copyConditionToRuleFileTreeOperation(node, draggedNode.Tag as ICondition);
                // If move, remove the dragged node.
                if (e.Effect == DragDropEffects.Move)
                {
                    removeConditionOperation(draggedNode, false);
                }
            }
            this.insertionHighlight.Visible = false;
        }

        private void addConditionToRuleFileTreeOperation(TreeNode node, IMatchTarget matchTarget)
        {
            AndCondition andCondition = ExtractDefaultConditions(matchTarget);
            TreeNode andConditionNode = RuleSet2TreeNodeProcessor.GetConditionTreeNode(
                    andCondition);
            andConditionNode.ExpandAll();
            if (insertionHighlight.Visible)
            {
                TreeNode parentNode = node.Parent;
                // Insert before node; not Append as the last sun of node.
                AbstractCompositeCondition compositeCondition = parentNode.Tag as AbstractCompositeCondition;
                compositeCondition.InsertConditionAt(andCondition, node.Index);
                parentNode.Nodes.Insert(node.Index, andConditionNode);
                if (!CanAddCondition(parentNode))
                    parentNode.Nodes.RemoveAt(parentNode.Nodes.Count - 1);
            }
            else
            {
                // Append as the last sun of node; not Insert before node.
                AbstractCompositeCondition compositeCondition = node.Tag as AbstractCompositeCondition;
                compositeCondition.AppendCondition(andCondition);
                node.Nodes.Insert(node.Nodes.Count - 1, andConditionNode);
                if (!CanAddCondition(node))
                    node.Nodes.RemoveAt(node.Nodes.Count - 1);
            }
            SetModified(true);
        }

        private void copyConditionToRuleFileTreeOperation(TreeNode node, ICondition condition)
        {
            TreeNode conditionNode = RuleSet2TreeNodeProcessor.GetConditionTreeNode(
                condition);
            conditionNode.ExpandAll();
            if (insertionHighlight.Visible)
            {
                TreeNode parentNode = node.Parent;
                AbstractCompositeCondition compositeCondition = parentNode.Tag as AbstractCompositeCondition;
                compositeCondition.InsertConditionAt(condition, node.Index);
                parentNode.Nodes.Insert(node.Index, conditionNode);
                if (!CanAddCondition(parentNode))
                    parentNode.Nodes.RemoveAt(parentNode.Nodes.Count - 1);
            }
            else
            {
                AbstractCompositeCondition compositeCondition = node.Tag as AbstractCompositeCondition;
                compositeCondition.AppendCondition(condition);
                node.Nodes.Insert(node.Nodes.Count - 1, conditionNode);
                if (!CanAddCondition(node))
                    node.Nodes.RemoveAt(node.Nodes.Count - 1);
            }
            SetModified(true);
        }

        private void ruleTreeView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void ruleTreeView_Scroll(object sender, RuleTreeView.ScrollEventArgs e)
        {
            if (conditionInPlaceEditor.Visible)
                UpdateConditionInPlaceEditorPosition();
        }

        private void UpdateConditionInPlaceEditorPosition()
        {
            TreeNode node = ruleTreeView.SelectedNode;
            if (node != null)
            {
                conditionInPlaceEditor.Location = new Point(node.Bounds.Location.X,
                    node.Bounds.Location.Y - 2);
            }
        }

        private void conditionInPlaceEditor_VisibleChanged(object sender, EventArgs e)
        {
            if (!conditionInPlaceEditor.Visible)
            {
                // visible -> invisible
                if (!conditionInPlaceEditor.IsProcesssed)
                {
                    conditionInPlaceEditor.IsProcesssed = true;
                    TreeNode modifiedTreeNode = conditionInPlaceEditor.ModifiedTreeNode;
                    if (IsEmptyConditionNode(modifiedTreeNode))
                    {
                        // recover first
                        modifiedTreeNode.Text = TreeNodeConstants.EmptyCondition;
                        ICondition newCondition = conditionInPlaceEditor.GetCondition();
                        if (newCondition == null)
                        {
                            if (conditionInPlaceEditor.GetConditionComboBoxSelectedItem() != null)
                                MessageBox.Show(Resource.FormatString("Wrn_InvalidCondition"));
                            return;
                        }
                        AbstractCompositeCondition compositeCondition =
                                    modifiedTreeNode.Parent.Tag as AbstractCompositeCondition;
                        compositeCondition.AppendCondition(newCondition);

                        // Replace tree node.
                        TreeNode newConditionNode =
                            RuleSet2TreeNodeProcessor.GetConditionTreeNode(newCondition);
                        newConditionNode.Expand();
                        modifiedTreeNode.Parent.Nodes.Insert(modifiedTreeNode.Index, newConditionNode);
                        // Remove empty condition.
                        if (!CanAddCondition(modifiedTreeNode.Parent))
                            modifiedTreeNode.Parent.Nodes.RemoveAt(modifiedTreeNode.Index);
                        SetModified(true);
                    }
                    else
                    {
                        ICondition oldCondition = modifiedTreeNode.Tag as ICondition;
                        // recover first
                        modifiedTreeNode.Text =
                            RuleSet2TreeNodeProcessor.GetConditionNodeText(oldCondition);
                        ICondition newCondition = conditionInPlaceEditor.GetCondition();
                        if (newCondition == null)
                        {
                            MessageBox.Show(Resource.FormatString("Wrn_InvalidCondition"));
                            return;
                        }
                        if (!newCondition.Equals(oldCondition))
                        {
                            if (IsConditionLoss(newCondition, oldCondition))
                            {
                                DialogResult response = MessageBox.Show(
                                    Resource.FormatString("Wrn_SubconditionLost"),
                                    Resource.FormatString("Wrn_SubconditionLostTitle"),
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button2);
                                if (response == DialogResult.No)
                                {
                                    return;
                                }
                            }
                            // Copy subconditions.
                            if (newCondition is AbstractCompositeCondition)
                            {
                                if (newCondition is AbstractSingleCondition)
                                {
                                    if (oldCondition is AbstractCompositeCondition)
                                    {
                                        AbstractCompositeCondition oldCompositeCondition =
                                            oldCondition as AbstractCompositeCondition;
                                        if (oldCompositeCondition.ConditionList.Count > 0)
                                        {
                                            AbstractSingleCondition newSingleCondition =
                                                newCondition as AbstractSingleCondition;
                                            newSingleCondition.AppendCondition(oldCompositeCondition.ConditionList[0]);
                                        }
                                    }
                                }
                                else
                                {
                                    // newCondition is AbstractMultipleCondition
                                    if (oldCondition is AbstractCompositeCondition)
                                    {
                                        AbstractCompositeCondition oldCompositeCondition =
                                            oldCondition as AbstractCompositeCondition;
                                        if (oldCompositeCondition.ConditionList.Count > 0)
                                        {
                                            AbstractMultipleCondition newCompositeCondition =
                                                newCondition as AbstractMultipleCondition;
                                            foreach (ICondition subCondition in oldCompositeCondition.ConditionList)
                                                newCompositeCondition.AppendCondition(subCondition);
                                        }
                                    }
                                }
                            }
                            AbstractCompositeCondition compositeCondition =
                                    modifiedTreeNode.Parent.Tag as AbstractCompositeCondition;
                            compositeCondition.ReplaceCondition(modifiedTreeNode.Index, newCondition);

                            // Replace tree node.
                            TreeNode newConditionNode =
                                RuleSet2TreeNodeProcessor.GetConditionTreeNode(newCondition);
                            newConditionNode.ExpandAll();
                            modifiedTreeNode.Parent.Nodes.Insert(modifiedTreeNode.Index, newConditionNode);
                            modifiedTreeNode.Remove();
                            SetModified(true);
                        }
                    }
                }
            }
            else
            {
                conditionInPlaceEditor.IsProcesssed = false;
            }
        }

        private static bool IsConditionLoss(ICondition newCondition, ICondition oldCondition)
        {
            if (newCondition is AbstractAtomicCondition)
            {
                if (oldCondition is AbstractCompositeCondition)
                {
                    AbstractCompositeCondition condition =
                        oldCondition as AbstractCompositeCondition;
                    if (condition.ConditionList.Count > 0)
                    {
                        return true;
                    }
                }
            }
            else if (newCondition is AbstractSingleCondition)
            {
                if (oldCondition is AbstractMultipleCondition)
                {
                    AbstractMultipleCondition condition =
                        oldCondition as AbstractMultipleCondition;
                    if (condition.ConditionList.Count > 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ruleTreeView_SizeChanged(object sender, EventArgs e)
        {
            if (conditionInPlaceEditor.Visible)
                UpdateConditionInPlaceEditorPosition();
        }

        private void ruleTreeView_DragOver(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the mouse position.
            Point targetPoint = ruleTreeView.PointToClient(new Point(e.X, e.Y));
            // Select the node at the mouse position.
            TreeNode node = ruleTreeView.GetNodeAt(targetPoint);
            if (node != null && !node.Bounds.Contains(targetPoint))
            {
                node = null;
            }
            // Retrieve the node that was dragged.
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
            if (draggedNode.Tag is IMatchTarget)
            {
                // drag from tlb tree.
                IMatchTarget matchTarget = draggedNode.Tag as IMatchTarget;

                if (node != null)
                {
                    if (node.Parent == null ||
                        (CanAddCondition(node) &&
                            matchTarget.GetCategory() == (GetRuleNode(node).Tag as Rule).Category) &&
                            !IsInsertToCompositeCondition(node, targetPoint))
                    {
                        ruleTreeView.SelectedNode = node;
                        this.insertionHighlight.Visible = false;
                        e.Effect = e.AllowedEffect;
                    }
                    else if (CanAddCondition(node.Parent) &&
                            matchTarget.GetCategory() == (GetRuleNode(node).Tag as Rule).Category)
                    {
                        ruleTreeView.SelectedNode = null;
                        this.insertionHighlight.Location = new Point(node.Bounds.X - 18,
                            node.Bounds.Y);
                        this.insertionHighlight.Visible = true;
                        e.Effect = e.AllowedEffect;
                    }
                    else
                    {
                        e.Effect = DragDropEffects.None;
                    }
                }
                else
                {
                    ruleTreeView.SelectedNode = null;
                    this.insertionHighlight.Visible = false;
                    e.Effect = e.AllowedEffect;
                }
            }
            else if (draggedNode.Tag is ICondition)
            {
                // drag from rule tree.
                ICondition draggedCondition = draggedNode.Tag as ICondition;

                if (node != null)
                {
                    if (CanAddCondition(node) &&
                            CanConditionApplyToCategory(draggedNode.Tag as ICondition,
                                (GetRuleNode(node).Tag as Rule).Category) &&
                            !IsInsertToCompositeCondition(node, targetPoint))
                    {
                        // Select the AND/OR/NOT node, instead of insertion line.
                        ruleTreeView.SelectedNode = node;
                        this.insertionHighlight.Visible = false;
                        if ((e.KeyState & CTRL) == CTRL)
                            e.Effect = DragDropEffects.Copy;
                        else
                            e.Effect = DragDropEffects.Move;
                    }
                    else if (CanAddCondition(node.Parent) &&
                            CanConditionApplyToCategory(draggedNode.Tag as ICondition,
                                (GetRuleNode(node).Tag as Rule).Category))
                    {
                        // display the insertion line and unselect the node to avoid misunderstanding.
                        ruleTreeView.SelectedNode = null;
                        this.insertionHighlight.Location = new Point(node.Bounds.X - 18,
                            node.Bounds.Y);
                        this.insertionHighlight.Visible = true;
                        if ((e.KeyState & CTRL) == CTRL)
                            e.Effect = DragDropEffects.Copy;
                        else
                            e.Effect = DragDropEffects.Move;
                    }
                    else
                    {
                        e.Effect = DragDropEffects.None;
                    }
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            if (e.Effect == DragDropEffects.None)
                this.insertionHighlight.Visible = false;
        }

        private bool IsInsertToCompositeCondition(TreeNode node, Point targetPoint)
        {
            return targetPoint.Y - node.Bounds.Y < 4;
        }

        private bool CanConditionApplyToCategory(ICondition condition, ICategory category)
        {
            if (condition is AbstractAtomicCondition)
                return condition.GetConditionDef().CanApplyToCategory(category);
            else
            {
                // AbstractCompositeCondition
                AbstractCompositeCondition compositeCondition = condition as AbstractCompositeCondition;
                foreach (ICondition subCondition in compositeCondition.ConditionList)
                {
                    if (!subCondition.GetConditionDef().CanApplyToCategory(category))
                        return false;
                }
                return condition.GetConditionDef().CanApplyToCategory(category);
            }
        }

        private void ruleTreeView_Leave(object sender, EventArgs e)
        {
            conditionInPlaceEditor.Visible = false;
        }

        private void ruleTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TreeNode draggedNode = e.Item as TreeNode;
                if (draggedNode.Tag is ICondition && !IsRootConditionTreeNode(draggedNode.Parent))
                    DoDragDrop(e.Item, DragDropEffects.Move | DragDropEffects.Copy);
            }
        }

        private void ruleTreeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            TreeNode node = ruleTreeView.SelectedNode;
            if (!(node.Tag is Rule))
            {
                e.CancelEdit = true;
            }
        }

        private void ruleTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (ruleTreeView.SelectedNode != null)
            {
                switch (e.KeyCode)
                {
                    case Keys.F2:
                        ruleTreeView.SelectedNode.BeginEdit();
                        break;
                    default:
                        break;
                }
            }
        }

        private void ruleTreeView_MouseClick(object sender, MouseEventArgs e)
        {
            TreeNode node = ruleTreeView.GetNodeAt(e.X, e.Y);
            if (node != null && node.Bounds.Contains(e.X, e.Y))
            {
                ruleTreeView.SelectedNode = node;
                if (e.Button == MouseButtons.Left)
                {
                    // Left mouse click.
                    if (IsEmptyConditionNode(node) ||
                        (node.Tag is ICondition && !this.IsRootConditionTreeNode(node.Parent)))
                    {
                        UpdateConditionInPlaceEditorPosition();
                        TreeNode ruleNode = GetRuleNode(node);
                        if (node.Tag is ICondition)
                            conditionInPlaceEditor.InitBeforeShow(node,
                                (ruleNode.Tag as Rule).Category, node.Tag as ICondition);
                        else
                            conditionInPlaceEditor.InitBeforeShow(node,
                                (ruleNode.Tag as Rule).Category, null);
                        node.Text = "";
                        conditionInPlaceEditor.Visible = true;
                    }
                }
            }
        }

        private int conditionExpressionPanelLength;
        private static readonly int CONDITION_EXPRESSION_PANEL_HEAD_LENGTH = 20;

        private void splitContainerConditionExp_Panel1_Click(object sender, EventArgs e)
        {
            if (splitContainerConditionExp.Panel2Collapsed)
            {
                splitContainerRuleFile.SplitterDistance -= conditionExpressionPanelLength -
                    CONDITION_EXPRESSION_PANEL_HEAD_LENGTH;
                splitContainerConditionExp.Panel2Collapsed = false;
                splitContainerConditionExp.SplitterDistance = CONDITION_EXPRESSION_PANEL_HEAD_LENGTH;
                splitContainerRuleFile.IsSplitterFixed = false;
                this.imageCloseButton.Visible = true;
            }
        }

        private void imageCloseButton_Click(object sender, EventArgs e)
        {
            if (!splitContainerConditionExp.Panel2Collapsed)
            {
                conditionExpressionPanelLength = splitContainerConditionExp.Height;
                splitContainerRuleFile.SplitterDistance += conditionExpressionPanelLength -
                    CONDITION_EXPRESSION_PANEL_HEAD_LENGTH;
                splitContainerConditionExp.Panel2Collapsed = true;
                splitContainerConditionExp.SplitterDistance = CONDITION_EXPRESSION_PANEL_HEAD_LENGTH;
                splitContainerRuleFile.IsSplitterFixed = true;
                this.imageCloseButton.Visible = false;
            }
        }

        private void ruleTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            TreeNode node = ruleTreeView.SelectedNode;
            if (node.Tag is Rule)
            {
                Rule rule = node.Tag as Rule;
                if (e.Label != null && !e.Label.Trim().Equals(""))
                {
                    rule.Name = e.Label;
                    SetModified(true);
                }
                else
                {
                    e.CancelEdit = true;
                }
            }
        }

        private void ruleTreeView_DragLeave(object sender, EventArgs e)
        {
            this.insertionHighlight.Visible = false;
        }

        private void aboutTlbImpRuleFileEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutDialog aboutDlg = new AboutDialog();
            aboutDlg.ShowDialog();
        }

        private void tlbTreeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (tlbTreeView.IsDefaultNodeSelected())
                openTlbToolStripButton.PerformClick();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                saveRuleFileOperation();
            }
        }

        private void ruleTreeView_DoubleClick(object sender, EventArgs e)
        {
            if (ruleTreeView.SelectedNode != null && ruleTreeView.SelectedNode.Parent != null &&
                ruleTreeView.SelectedNode.Parent.Tag is IAction)
            {
                modifyActionOperation();
            }
        }

    }
}