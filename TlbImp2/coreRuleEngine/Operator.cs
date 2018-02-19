using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRuleEngine
{
    /// <summary>
    /// Operator is the predicate (or verb) of an atomic condition.
    /// For example, in the atomic condition "NativeName Equal 'ABC'", the operator is "Equal" which
    /// is simply comparing two strings and giving the result that whether the two strings are equal.
    /// </summary>
    public interface IOperator
    {
        /// <summary>
        /// Usually, the name of an operator is its predicte word.
        /// </summary>
        string GetOperatorName();

        /// <summary>
        /// Test if the logic statement "subjectValue OperatorName objectValue" is true.
        /// </summary>
        bool IsTrue(string subjectValue, string objectValue);

        /// <summary>
        /// Operator is usually used in a Condition. So this method tells if this Operator can be applied
        /// to the specified kinds of Conditions.
        /// </summary>
        bool CanApplyToCondition(IConditionDef conditionDef);
    }

    /// <summary>
    /// The manager of all Operators.
    /// All Operators are registered in this manager. OperatorManager managers them using the Operator
    /// name. So, we can get the Operator instance by its name.
    /// </summary>
    public abstract class AbstractOperatorManager
    {
        private Dictionary<string, IOperator> m_registeredOperator;

        public AbstractOperatorManager()
        {
            m_registeredOperator = new Dictionary<string, IOperator>();
        }

        protected void RegisterOperator(string operatorName, IOperator Operator)
        {
            m_registeredOperator.Add(operatorName, Operator);
        }

        public IOperator GetOperator(string operatorName)
        {
            if (m_registeredOperator.ContainsKey(operatorName))
            {
                return m_registeredOperator[operatorName];
            }
            return null;
        }

        public List<IOperator> GetPossibleOperatorList(IConditionDef conditionDef)
        {
            List<IOperator> operatorList = new List<IOperator>();
            Dictionary<string, IOperator>.Enumerator enumerator =
                m_registeredOperator.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Value.CanApplyToCondition(conditionDef))
                    operatorList.Add(enumerator.Current.Value);
            }
            return operatorList;
        }

        public List<IOperator> GetAllOperatorList()
        {
            List<IOperator> operatorList = new List<IOperator>();
            Dictionary<string, IOperator>.Enumerator enumerator =
                m_registeredOperator.GetEnumerator();
            while (enumerator.MoveNext())
            {
                operatorList.Add(enumerator.Current.Value);
            }
            return operatorList;
        }
    }
}
