using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TypeLibTypes.Interop;
using TlbImpRuleEngine;
using CoreRuleEngine;

namespace TypeLibraryTreeView
{
    public class TypeLib2TreeNodeProcessor
    {
        public const string DEFAULT_TREE_ROOT_TAG = "TlbRoot";

        const string IMAGE_KEY_LIB = "Lib";

        internal static TreeNode GetTypeLibNode(TypeLib tlb, DisplayLevel displayLevel)
        {
            string typeLibName = tlb.GetDocumentation();
            TreeNode root = new TreeNode(typeLibName);
            root.Tag = tlb;

            int nCount = tlb.GetTypeInfoCount();
            for (int n = 0; n < nCount; ++n)
            {
                TypeInfo type = tlb.GetTypeInfo(n);
                //string typeTypeName = type.GetDocumentation();
                //NativeType2String.AddNativeUserDefinedType(typeTypeName);

                // For dual interfaces, it has a "funky" TKIND_DISPATCH|TKIND_DUAL interface with a parter of TKIND_INTERFACE|TKIND_DUAL interface
                // The first one is pretty bad and has duplicated all the interface members of its parent, which is not we want
                // We want the second v-table interface
                // So, if we indeed has seen this kind of interface, prefer its partner
                // However, we should not blindly get the partner because those two interfaces partners with each other
                // So we need to first test to see if the interface is both dispatch & dual, and then get its partner interface
                using (TypeAttr attr = type.GetTypeAttr())
                {
                    if (attr.IsDual && attr.IsDispatch)
                    {
                        TypeInfo typeReferencedType = type.GetRefTypeNoComThrow();
                        if (typeReferencedType != null)
                        {
                            type = typeReferencedType;
                        }
                    }
                }
                TreeNode typeInfoNode = new TreeNode();
                TypeInfoMatchTarget typeInfoMatchTarget = null;
                root.Nodes.Add(typeInfoNode);
                using (TypeAttr attr = type.GetTypeAttr())
                {
                    TYPEKIND kind = attr.typekind;
                    typeInfoMatchTarget = new TypeInfoMatchTarget(tlb, type, kind);
                    if (displayLevel == DisplayLevel.All)
                    {
                        ProcessFunctions(type, typeInfoNode);
                        ProcessFields(type, typeInfoNode);
                    }
                }
                typeInfoNode.Text = typeInfoMatchTarget.Name + ": " +
                                typeInfoMatchTarget.Type;
                typeInfoNode.Tag = typeInfoMatchTarget;
                SetTlbTreeNodeImage(typeInfoNode);
            }
            return root;
        }

        public static void SetTlbTreeNodeImage(TreeNode treeNode)
        {
            treeNode.ImageKey = GetTlbTreeNodeImageKey(treeNode);
            treeNode.SelectedImageKey = treeNode.ImageKey;
            treeNode.StateImageKey = treeNode.ImageKey;
        }

        private static string GetTlbTreeNodeImageKey(TreeNode node)
        {
            object tag = node.Tag;
            if (tag is TypeLib || (tag is string && tag.Equals(DEFAULT_TREE_ROOT_TAG)))
            {
                return IMAGE_KEY_LIB;
            }
            else if (tag is IMatchTarget)
            {
                if (tag is IGetTypeLibElementCommonInfo)
                {
                    return (tag as IGetTypeLibElementCommonInfo).Type;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        private static void ProcessFields(TypeInfo parentTypeInfo, TreeNode parentTreeNode)
        {
            //
            // Walk through all vars (including elements from structs and disp interface, ...)
            //
            using (TypeAttr attr = parentTypeInfo.GetTypeAttr())
            {
                for (int i = 0; i < attr.cVars; ++i)
                {
                    FieldInfoMatchTarget variableInfo = new FieldInfoMatchTarget(parentTypeInfo, i);
                    TreeNode dispVarTreeNode = new TreeNode();
                    dispVarTreeNode.Text = variableInfo.Name + ": " + variableInfo.Type;
                    dispVarTreeNode.Tag = variableInfo;
                    SetTlbTreeNodeImage(dispVarTreeNode);
                    parentTreeNode.Nodes.Add(dispVarTreeNode);
                }
            }
        }

        private static void ProcessFunctions(TypeInfo parentTypeInfo, TreeNode parentTreeNode)
        {
            using (TypeAttr attr = parentTypeInfo.GetTypeAttr())
            {
                //
                // Walk through all the function/propput/propget/propref properties
                //
                for (int i = ConvCommon2.GetIndexOfFirstMethod(parentTypeInfo, attr);
                     i < attr.cFuncs; ++i)
                {
                    FunctionInfoMatchTarget functionInfo =
                        new FunctionInfoMatchTarget(parentTypeInfo, (short) i);
                    TreeNode funcTreeNode = new TreeNode();
                    if (functionInfo.FuncDesc.IsPropertyGet)
                    {
                        funcTreeNode.Text = functionInfo.Name + " (getter)" + ": " + functionInfo.Type;
                    }
                    else if (functionInfo.FuncDesc.IsPropertyPut || functionInfo.FuncDesc.IsPropertyPutRef)
                    {
                        funcTreeNode.Text = functionInfo.Name + " (setter)" + ": " + functionInfo.Type;
                    }
                    else
                    {
                        funcTreeNode.Text = functionInfo.Name + ": " + functionInfo.Type;
                    }
                    funcTreeNode.Tag = functionInfo;
                    SetTlbTreeNodeImage(funcTreeNode);
                    parentTreeNode.Nodes.Add(funcTreeNode);
                    ProcessFuncParams(parentTypeInfo, functionInfo.Index, funcTreeNode);
                }
            }
        }

        private static void ProcessFuncParams(TypeInfo interfaceTypeInfo,
            int funcIndex, TreeNode parentTreeNode)
        {
            int paramIndex = 0;
            FuncDesc funcDesc = interfaceTypeInfo.GetFuncDesc(funcIndex);
            ElemDesc retElemDesc = funcDesc.elemdescFunc;
            SignatureInfoMatchTarget retSignatureInfo = new SignatureInfoMatchTarget(interfaceTypeInfo,
                funcIndex, retElemDesc, paramIndex);
            TreeNode retTreeNode = new TreeNode();
            string typeString =
                    (new TlbType2String(interfaceTypeInfo, retElemDesc.tdesc)).GetTypeString();
            retTreeNode.Text = typeString + "  " + retSignatureInfo.Name +
                    ": " + retSignatureInfo.Type;
            retTreeNode.Tag = retSignatureInfo;
            SetTlbTreeNodeImage(retTreeNode);
            parentTreeNode.Nodes.Add(retTreeNode);
            ++paramIndex;
            // Parameters
            //string[] signatureNames = interfaceTypeInfo.GetNames(funcDesc.memid, funcDesc.cParams + 1);
            for (int i = 0; i < funcDesc.cParams; ++i)
            {
                ElemDesc paramElemDesc = funcDesc.GetElemDesc(i);

                typeString =
                    (new TlbType2String(interfaceTypeInfo, paramElemDesc.tdesc)).GetTypeString();

                //string signatureName = signatureNames[i + 1];
                //if (signatureName.Trim().Equals(""))
                //    signatureName = "_unnamed_arg_" + paramIndex;
                SignatureInfoMatchTarget paramSignatureInfo = new SignatureInfoMatchTarget(
                    interfaceTypeInfo, funcIndex, paramElemDesc, paramIndex);
                TreeNode paramTreeNode = new TreeNode();
                paramTreeNode.Text = typeString + "  " + paramSignatureInfo.Name +
                    ": " + paramSignatureInfo.Type;
                ++paramIndex;
                paramTreeNode.Tag = paramSignatureInfo;
                SetTlbTreeNodeImage(paramTreeNode);
                parentTreeNode.Nodes.Add(paramTreeNode);
            }
        }

        public static TreeNode GetDefaultLibNode()
        {
            TreeNode root = new TreeNode();
            root.Tag = TypeLib2TreeNodeProcessor.DEFAULT_TREE_ROOT_TAG;
            return root;
        }

    }
}
