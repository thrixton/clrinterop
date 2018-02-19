using System;
using System.Collections.Generic;
using System.Text;
using CoreRuleEngine;
using System.Windows.Forms;
using TlbImpRuleEngine;
using System.Drawing;
using System.Resources;

namespace TlbImpRuleFileEditor
{
    class RuleSet2TreeNodeProcessor
    {
        private static Font s_emptyConditionFont = new Font("Verdana", 8,
                FontStyle.Italic);

        private const string IMAGE_KEY_RULES = "Rules";
        private const string IMAGE_KEY_RULE = "Rule";
        private const string IMAGE_KEY_CATEGORY = "Category";
        private const string IMAGE_KEY_CONDITION = "Condition";
        private const string IMAGE_KEY_SINGLE_CONDITION = "SingleCondition";
        private const string IMAGE_KEY_ACTION = "Action";
        private const string IMAGE_KEY_MULTI_CONDITION = "MultiCondition";

        private const string CATEGORY_NODE_TEXT = "Category";
        private const string ACTION_NODE_TEXT = "Action";

        public static TreeNode GetRuleSetTreeNode(RuleSet ruleSet)
        {
            TreeNode ruleSetNode = new TreeNode();
            List<Rule> ruleList = ruleSet.GetAllRules();
            foreach (Rule rule in ruleList)
            {
                TreeNode ruleNode = GetRuleTreeNode(rule);
                ruleSetNode.Nodes.Add(ruleNode);
            }
            ruleSetNode.Tag = ruleSet;
            SetRuleTreeNodeImage(ruleSetNode);
            return ruleSetNode;
        }

        private static void SetRuleTreeNodeImage(TreeNode treeNode)
        {
            treeNode.ImageKey = GetRuleTreeNodeImageKey(treeNode);
            treeNode.SelectedImageKey = treeNode.ImageKey;
            treeNode.StateImageKey = treeNode.ImageKey;
        }

        private static string GetRuleTreeNodeImageKey(TreeNode treeNode)
        {
            object tag = treeNode.Tag;
            if (tag is RuleSet)
            {
                return IMAGE_KEY_RULES;
            }
            else if (tag is Rule)
            {
                return IMAGE_KEY_RULE;
            }
            else if (tag is ICategory)
            {
                return IMAGE_KEY_CATEGORY;
            }
            else if (tag is string && tag.Equals(TreeNodeConstants.Condition))
            {
                return IMAGE_KEY_CONDITION;
            }
            else if (tag is string && tag.Equals(TreeNodeConstants.EmptyCondition))
            {
                return IMAGE_KEY_SINGLE_CONDITION;
            }
            else if (tag is IAction)
            {
                return IMAGE_KEY_ACTION;
            }
            else if (tag is ICondition)
            {
                if (tag is AbstractCompositeCondition)
                {
                    return IMAGE_KEY_MULTI_CONDITION;
                }
                else
                {
                    return IMAGE_KEY_SINGLE_CONDITION;
                }
            }
            return null;
        }

        public static TreeNode GetRuleTreeNode(Rule rule)
        {
            TreeNode ruleNode = new TreeNode();
            ruleNode.Text = rule.Name;
            TreeNode categoryNode = GetCategoryTreeNode(rule.Category);
            ruleNode.Nodes.Add(categoryNode);
            TreeNode conditionRootNode = GetConditionRootTreeNode(rule.Condition);
            ruleNode.Nodes.Add(conditionRootNode);
            TreeNode actionNode = GetActionTreeNode(rule.Action);
            ruleNode.Nodes.Add(actionNode);
            ruleNode.Tag = rule;
            SetRuleTreeNodeImage(ruleNode);
            return ruleNode;
        }

        private static TreeNode GetCategoryTreeNode(ICategory category)
        {
            TreeNode categoryNode = new TreeNode();
            categoryNode.Text = CATEGORY_NODE_TEXT + " : " + category.GetCategoryName();
            categoryNode.Tag = category;
            SetRuleTreeNodeImage(categoryNode);
            return categoryNode;
        }

        private static TreeNode GetConditionRootTreeNode(ICondition condition)
        {
            TreeNode matchNode = new TreeNode(TreeNodeConstants.Condition);
            if (condition != null)
            {
                TreeNode conditionNode = GetConditionTreeNode(condition);
                matchNode.Nodes.Add(conditionNode);
            }
            matchNode.Tag = TreeNodeConstants.Condition;
            SetRuleTreeNodeImage(matchNode);
            return matchNode;
        }

        public static TreeNode GetConditionTreeNode(ICondition condition)
        {
            TreeNode conditionNode = new TreeNode();
            if (condition is AbstractCompositeCondition)
            {
                AbstractCompositeCondition compositeCondition = condition as AbstractCompositeCondition;
                
                foreach (ICondition subCondition in compositeCondition.ConditionList)
                {
                    conditionNode.Nodes.Add(GetConditionTreeNode(subCondition));
                }

                // Append the "<Empty>" node.
                AbstractCompositeConditionDef compositeConditionDef =
                    condition.GetConditionDef() as AbstractCompositeConditionDef;
                if (conditionNode.Nodes.Count < compositeConditionDef.GetMaxSubconditionNumber())
                {
                    conditionNode.Nodes.Add(GetEmptyConditionTreeNode());
                }
            }
            conditionNode.Text = GetConditionNodeText(condition);
            conditionNode.Tag = condition;
            SetRuleTreeNodeImage(conditionNode);
            return conditionNode;
        }

        public static TreeNode GetEmptyConditionTreeNode()
        {
            TreeNode conditionNode = new TreeNode();
            conditionNode.Text = TreeNodeConstants.EmptyCondition;
            conditionNode.ForeColor = Color.LightGray;
            conditionNode.NodeFont = s_emptyConditionFont;

            conditionNode.Tag = TreeNodeConstants.EmptyCondition;
            SetRuleTreeNodeImage(conditionNode);
            return conditionNode;
        }

        public static string GetConditionNodeText(ICondition condition)
        {
            if (condition is AbstractCompositeCondition)
            {
                AbstractCompositeCondition compositeCondition = condition as AbstractCompositeCondition;
                return compositeCondition.GetConditionDef().GetConditionName();
            }
            else if (condition is AbstractAtomicCondition)
            {
                AbstractAtomicCondition singleCondition = condition as AbstractAtomicCondition;
                return singleCondition.GetExpression();
            }
            else
            {
                return TreeNodeConstants.EmptyCondition;
            }
        }

        public static TreeNode GetActionTreeNode(IAction action)
        {
            TreeNode actionNode = new TreeNode();
            actionNode.Text = ACTION_NODE_TEXT + " : " + action.GetActionDef().GetActionName();
            if (!action.IsInitialized)
            {
                actionNode.Text += " <" + Resource.FormatString("Msg_EditUninitializedActionTip") + ">";
            }
            actionNode.Tag = action;

            IActionDef actionDef =
                RuleEngine.GetActionManager().GetActionDef(action.GetActionDef().GetActionName());
            List<string> paramNameList = actionDef.GetParameterNames();
            foreach (string paramName in paramNameList)
            {
                TreeNode actionParameterNode = new TreeNode();
                StringBuilder sb = new StringBuilder();
                sb.Append(paramName);
                sb.Append(" = '");
                if (action.IsInitialized)
                    sb.Append(action.GetParameterValue(paramName));
                else
                    sb.Append(TreeNodeConstants.Uninitialized_Value);
                sb.Append("'");
                actionParameterNode.Text = sb.ToString();
                actionParameterNode.Tag = action;
                SetRuleTreeNodeImage(actionParameterNode);
                actionNode.Nodes.Add(actionParameterNode);
            }

            SetRuleTreeNodeImage(actionNode);
            return actionNode;
        }

        private static string GetActionParamString(IAction action)
        {
            StringBuilder sb = new StringBuilder();
            IActionDef actionDef =
                RuleEngine.GetActionManager().GetActionDef(action.GetActionDef().GetActionName());
            List<string> paramNameList = actionDef.GetParameterNames();
            foreach (string paramName in paramNameList)
            {
                if (sb.Length != 0)
                    sb.Append(", ");
                sb.Append(paramName);
                sb.Append("='");
                sb.Append(action.GetParameterValue(paramName));
                sb.Append("'");
            }
            return sb.ToString();
        }

    }
}
