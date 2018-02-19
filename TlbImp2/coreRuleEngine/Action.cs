using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using System.Reflection;

namespace CoreRuleEngine
{
    /// <summary>
    /// The definition of Action.
    /// Every IAction class has the correponding IActionDef class.
    /// Different IActionDefs have different Action Names. Actions that the same IActionDef creates, share
    /// the same Action Name.
    /// </summary>
    public interface IActionDef
    {
        /// <summary>
        /// Get the action name of this IActionDef.
        /// This action name is unique, and is usually used as key of this IActionDef.
        /// </summary>
        string GetActionName();

        /// <summary>
        /// Get all parameter names of this IActionDef. Actions that this IActionDef creates, share the
        /// same parameter name list.
        /// </summary>
        List<string> GetParameterNames();

        /// <summary>
        /// Create an Action with this IActionDef instance, using the Dictionary "parameters" to initialize
        /// the paramters of the Action instance.
        /// </summary>
        IAction Create(Dictionary<string, string> parameters);

        /// <summary>
        /// Create an Action without initializing.
        /// </summary>
        IAction Create();

        /// <summary>
        /// Tell if this IActionDef can be applied to the "category".
        /// </summary>
        bool CanApplyToCategory(ICategory category);
    }

    /// <summary>
    /// All action classes implement IAction interface.
    /// Action tells what the rule will perform on the element, when the element satisfied the condition
    /// of the rule.
    /// Actions can have parameters, Name/Value pairs. Parameters are usually stored in a Dictionary
    /// collection.
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Get the corresponding IActionDef of this IAction.
        /// </summary>
        IActionDef GetActionDef();

        /// <summary>
        /// Get the parameter value of the parameter name specified.
        /// </summary>
        string GetParameterValue(string parameterName);

        /// <summary>
        /// Set the new value of parameter specified by "parameterName".
        /// Return true if succeed, otherwise false.
        /// </summary>
        bool SetParameterValue(string parameterName, string value);

        /// <summary>
        /// Tell if parameters of this Action are all set with valid values.
        /// </summary>
        bool IsInitialized
        {
            get;
            set;
        }
    }

    /// <summary>
    /// The mananger of the all ActionDefs.
    /// All IActionDefs are registered in this manager. Use the action name as the key to get the
    /// corresponding IActionDef.
    /// </summary>
    public abstract class AbstractActionManager
    {
        private Dictionary<string, IActionDef> m_registeredActionDef;

        public AbstractActionManager()
        {
            m_registeredActionDef = new Dictionary<string, IActionDef>();
        }

        /// <summary>
        /// Register an action in this manager.
        /// The action name is used as the key of IActionDef in the register collection.
        /// Usually, this method is called in the ctor of AbstractActionManager's sub classes.
        /// <param name="actionDef"></param>
        protected void RegisterAction(string actionName, IActionDef actionDef)
        {
            m_registeredActionDef.Add(actionName, actionDef);
        }

        /// <summary>
        /// Get all IActionDefs, whose corresponding IActions can be applied to category.
        /// </summary>
        public List<IActionDef> GetPossibleActionDefList(ICategory category)
        {
            List<IActionDef> actionDefList = new List<IActionDef>();
            Dictionary<string, IActionDef>.Enumerator enumerator =
                m_registeredActionDef.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Value.CanApplyToCategory(category))
                    actionDefList.Add(enumerator.Current.Value);
            }
            return actionDefList;
        }

        /// <summary>
        /// Get the IActionDef by action name.
        /// The action name of an IActionDef is used as the key of IActionDef.
        /// </summary>
        public IActionDef GetActionDef(string actionName)
        {
            if (m_registeredActionDef.ContainsKey(actionName))
                return m_registeredActionDef[actionName];
            else
                return null;
        }

        /// <summary>
        /// Create action using the IActionDef whose name is "actionName".
        /// Use "parameters" to initialize the action.
        /// The return action's IsInitialized property is set true.
        /// </summary>
        public IAction CreateAction(string actionName, Dictionary<string, string> parameters)
        {
            if (m_registeredActionDef.ContainsKey(actionName))
            {
                IActionDef actionDef = m_registeredActionDef[actionName];
                IAction action = actionDef.Create(parameters);
                action.IsInitialized = true;
                return action;
            }
            return null;
        }

        /// <summary>
        /// Create action using the IActionDef whose name is "actionName".
        /// The return action's IsInitialized property is set false.
        /// <returns></returns>
        public IAction CreateAction(string actionName)
        {
            if (m_registeredActionDef.ContainsKey(actionName))
            {
                IActionDef actionDef = m_registeredActionDef[actionName];
                IAction action = actionDef.Create();
                action.IsInitialized = false;
                return action;
            }
            return null;
        }
    }
}
