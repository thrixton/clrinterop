using System;
using System.Collections.Generic;
using System.Text;
using CoreRuleEngine;
using System.Globalization;

namespace TlbImpRuleEngine
{
    public class ResolveToActionDef : IActionDef
    {
        private const string ActionName = "ResolveTo";

        private ResolveToActionDef() { }

        private static ResolveToActionDef s_resolveToActionDef =
            new ResolveToActionDef();

        public static ResolveToActionDef GetInstance()
        {
            return s_resolveToActionDef;
        }

        public const string ParameterAssemblyName = "AssemblyName";
        public const string ParameterManagedTypeFullName = "ManagedTypeFullName";

        private static readonly List<string> parameterNames = new List<string> {
            ParameterAssemblyName, ParameterManagedTypeFullName };

        private static readonly List<ICategory> possibleCategoryList = new List<ICategory>
        {
            TypeCategory.GetInstance(),
        };

        #region IActionCreator Members

        public IAction Create()
        {
            ResolveToAction resolveToAction = new ResolveToAction();
            return resolveToAction;
        }

        public IAction Create(Dictionary<string, string> parameters)
        {
            ResolveToAction resolveToAction = new ResolveToAction();
            foreach (string parameterName in parameterNames)
            {
                if (parameters.ContainsKey(parameterName))
                {
                    resolveToAction.SetParameterValue(parameterName, parameters[parameterName]);
                }
                else
                {
                    throw new NoActionParameterException(ActionName, parameterName);
                }
            }
            return resolveToAction;
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

    public class ResolveToAction : IAction
    {
        private bool m_isInitialized;

        private string m_assemblyName;

        private string m_managedTypeFullName;

        public string AssemblyName
        {
            get
            {
                return m_assemblyName;
            }
            set
            {
                m_assemblyName = value;
            }
        }

        public string ManagedTypeFullName
        {
            get
            {
                return m_managedTypeFullName;
            }
            set
            {
                m_managedTypeFullName = value;
            }
        }

        #region IAction Members

        public string GetParameterValue(string parameterName)
        {
            if (parameterName.Equals(ResolveToActionDef.ParameterAssemblyName))
            {
                return m_assemblyName;
            }
            else if (parameterName.Equals(ResolveToActionDef.ParameterManagedTypeFullName))
            {
                return m_managedTypeFullName;
            }
            else
            {
                return null;
            }
        }

        public bool SetParameterValue(string parameterName, string value)
        {
            if (parameterName.Equals(ResolveToActionDef.ParameterAssemblyName))
            {
                if (value != null)
                {
                    if (value.ToUpper(CultureInfo.InvariantCulture).EndsWith(".DLL",
                        StringComparison.InvariantCulture))
                        value = value.Substring(0, value.Length - 4);
                    else if (value.ToUpper(CultureInfo.InvariantCulture).EndsWith(".EXE",
                        StringComparison.InvariantCulture))
                        value = value.Substring(0, value.Length - 4);
                    m_assemblyName = value;
                    return true;
                }
            }
            else if (parameterName.Equals(ResolveToActionDef.ParameterManagedTypeFullName))
            {
                if (value != null)
                {
                    m_managedTypeFullName = value;
                    return true;
                }
            }
            return false;
        }

        public IActionDef GetActionDef()
        {
            return ResolveToActionDef.GetInstance();
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
