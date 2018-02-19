using System;
using System.Collections.Generic;
using System.Text;
using CoreRuleEngine;

namespace TlbImpRuleEngine
{
    public class CannotApplyConditionToMatchTargetException : Exception
    {
        private ICondition m_condition;
        
        private IMatchTarget m_target;

        public CannotApplyConditionToMatchTargetException(ICondition condition, IMatchTarget target)
        {
            m_condition = condition;
            m_target = target;
        }
    }

    public class IGetParentNativeNameUnsupportedException : Exception
    {
        private object m_matchTarget;

        public IGetParentNativeNameUnsupportedException(object matchTarget)
        {
            m_matchTarget = matchTarget;
        }

        public object MatchTarget
        {
            get
            {
                return m_matchTarget;
            }
        }
    }

    public class NoActionParameterException : Exception
    {
        private string m_actionName;

        private string m_missingParameterName;

        public NoActionParameterException(string actionName, string missingParameterName)
        {
            m_actionName = actionName;
            m_missingParameterName = missingParameterName;
        }

        public string ActionName
        {
            get
            {
                return m_actionName;
            }
        }

        public string MissingParameterName
        {
            get
            {
                return m_missingParameterName;
            }
        }
    }
    
}
