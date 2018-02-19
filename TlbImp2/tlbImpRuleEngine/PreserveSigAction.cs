using System;
using System.Collections.Generic;
using System.Text;
using CoreRuleEngine;

namespace TlbImpRuleEngine
{
    public class PreserveSigActionDef : IActionDef
    {
        private const string ActionName = "PreserveSig";

        private PreserveSigActionDef() { }

        private static PreserveSigActionDef s_PreserveSigActionDef =
            new PreserveSigActionDef();

        public static PreserveSigActionDef GetInstance()
        {
            return s_PreserveSigActionDef;
        }

        private static readonly List<string> parameterNames = new List<string> { };

        private static readonly List<ICategory> possibleCategoryList = new List<ICategory>
        {
            FunctionCategory.GetInstance(),
        };

        #region IActionCreator Members

        public IAction Create()
        {
            PreserveSigAction preserveSigAction = new PreserveSigAction();
            return preserveSigAction;
        }

        public IAction Create(Dictionary<string, string> parameters)
        {
            return Create();
        }

        public string GetActionName()
        {
            return ActionName;
        }

        public List<string> GetParameterNames()
        {
            return parameterNames;
        }

        public bool CanApplyToCategory(ICategory category)
        {
            return possibleCategoryList.Contains(category);
        }

        #endregion
    }

    public class PreserveSigAction : IAction
    {
        #region IAction Members

        public string GetParameterValue(string parameterName)
        {
            return null;
        }

        public bool SetParameterValue(string parameterName, string value)
        {
            return false;
        }

        public IActionDef GetActionDef()
        {
            return PreserveSigActionDef.GetInstance();
        }

        public bool IsInitialized
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        #endregion
    }
}
