using System;
using System.Collections.Generic;
using System.Text;
using CoreRuleEngine;

namespace TlbImpRuleEngine
{
    public class ChangeManagedNameActionDef : IActionDef
    {
        private const string ActionName = "ChangeManagedName";

        private ChangeManagedNameActionDef() { }

        private static ChangeManagedNameActionDef s_changeManagedNameActionDef =
            new ChangeManagedNameActionDef();

        public static ChangeManagedNameActionDef GetInstance()
        {
            return s_changeManagedNameActionDef;
        }

        public const string ParameterNewName = "NewName";

        private static readonly List<string> parameterNames = new List<string> { ParameterNewName };

        private static readonly List<ICategory> possibleCategoryList = new List<ICategory>
        {
            TypeCategory.GetInstance(),
        };

        #region IActionDef Members

        public IAction Create()
        {
            ChangeManagedNameAction changeManagedNameAction = new ChangeManagedNameAction();
            return changeManagedNameAction;
        }

        public IAction Create(Dictionary<string, string> parameters)
        {
            ChangeManagedNameAction changeManagedNameAction = new ChangeManagedNameAction();
            if (parameters.ContainsKey(ParameterNewName))
            {
                changeManagedNameAction.NewName = parameters[ParameterNewName];
            }
            else
            {
                throw new NoActionParameterException(ActionName, ParameterNewName);
            }
            return changeManagedNameAction;
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

    public class ChangeManagedNameAction : IAction
    {
        private bool m_isInitialized;

        private string m_newName;

        public ChangeManagedNameAction()
        {
        }

        public string NewName
        {
            get
            {
                return m_newName;
            }
            set
            {
                m_newName = value;
            }
        }

        #region IAction Members

        public string GetParameterValue(string parameterName)
        {
            if (parameterName.Equals(ChangeManagedNameActionDef.ParameterNewName))
            {
                return m_newName;
            }
            else
            {
                return null;
            }
        }

        public bool SetParameterValue(string parameterName, string value)
        {
            if (parameterName.Equals(ChangeManagedNameActionDef.ParameterNewName))
            {
                if (value != null)
                {
                    m_newName = value;
                    return true;
                }
            }
            return false;
        }

        public IActionDef GetActionDef()
        {
            return ChangeManagedNameActionDef.GetInstance();
        }

        public bool IsInitialized
        {
            get
            {
                return m_isInitialized;
            }
            set
            {
                m_isInitialized = value;
            }
        }

        #endregion
    }
}
