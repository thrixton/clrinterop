using System;
using System.Collections.Generic;
using System.Text;
using CoreRuleEngine;
using System.Text.RegularExpressions;

namespace TlbImpRuleEngine
{
    public class TlbImpOperatorManager : AbstractOperatorManager
    {
        public TlbImpOperatorManager()
        {
            EqualOperator equalOperator = EqualOperator.GetInstance();
            RegisterOperator(equalOperator.GetOperatorName(), equalOperator);

            NotEqualOperator notEqualOperator = NotEqualOperator.GetInstance();
            RegisterOperator(notEqualOperator.GetOperatorName(), notEqualOperator);

            ContainsOperator containsOperator = ContainsOperator.GetInstance();
            RegisterOperator(containsOperator.GetOperatorName(), containsOperator);

            NotContainsOperator notContainsOperator = NotContainsOperator.GetInstance();
            RegisterOperator(notContainsOperator.GetOperatorName(), notContainsOperator);

            EqualAnyOperator equalAnyOperator = EqualAnyOperator.GetInstance();
            RegisterOperator(equalAnyOperator.GetOperatorName(), equalAnyOperator);

            MatchOperator matchOperator = MatchOperator.GetInstance();
            RegisterOperator(matchOperator.GetOperatorName(), matchOperator);
        }
    }

    public class EqualAnyOperator : IOperator
    {
        private EqualAnyOperator() { }

        private static EqualAnyOperator s_equalAnyOperator = new EqualAnyOperator();

        public static EqualAnyOperator GetInstance()
        {
            return s_equalAnyOperator;
        }

        private static readonly List<IConditionDef> possibleConditionDefList =
            new List<IConditionDef>
        {
            NativeNameConditionDef.GetInstance(),
            NativeParentTypeNameConditionDef.GetInstance(),
            NativeParentFunctionNameConditionDef.GetInstance(),
        };

        private const string OperatorName = "EqualAny";

        #region IOperator Members

        public string GetOperatorName()
        {
            return OperatorName;
        }

        public bool IsTrue(string subjectValue, string objectValue)
        {
            string[] isAnyStringArray = objectValue.Split(new char[] { ';' });
            foreach (string oneValue in isAnyStringArray)
            {
                if (oneValue.Equals(subjectValue))
                    return true;
            }
            return false;
        }

        public bool CanApplyToCondition(IConditionDef conditionCreator)
        {
            return possibleConditionDefList.Contains(conditionCreator);
        }

        #endregion
    }

    public class EqualOperator : IOperator
    {
        private EqualOperator() { }

        private static EqualOperator s_equalOperator = new EqualOperator();

        public static EqualOperator GetInstance()
        {
            return s_equalOperator;
        }

        private static readonly List<IConditionDef> possibleConditionDefList =
            new List<IConditionDef>
        {
            NativeNameConditionDef.GetInstance(),
            TypeKindConditionDef.GetInstance(),
            GuidConditionDef.GetInstance(),
            NativeParentTypeNameConditionDef.GetInstance(),
            NativeParentFunctionNameConditionDef.GetInstance(),
            NativeParameterIndexConditionDef.GetInstance(),
            NativeSignatureConditionDef.GetInstance(),
        };

        private const string operatorName = "Equal";

        #region IOperator Members

        public string GetOperatorName()
        {
            return operatorName;
        }

        public bool IsTrue(string subjectValue, string objectValue)
        {
            if (subjectValue != null && subjectValue.Equals(objectValue))
                return true;
            return false;
        }

        public bool CanApplyToCondition(IConditionDef conditionCreator)
        {
            return possibleConditionDefList.Contains(conditionCreator);
        }

        #endregion
    }

    public class ContainsOperator : IOperator
    {
        private ContainsOperator() { }

        private static ContainsOperator s_containsOperator = new ContainsOperator();

        public static ContainsOperator GetInstance()
        {
            return s_containsOperator;
        }

        private static readonly List<IConditionDef> possibleConditionDefList =
            new List<IConditionDef>
        {
            NativeNameConditionDef.GetInstance(),
            NativeParentTypeNameConditionDef.GetInstance(),
            NativeParentFunctionNameConditionDef.GetInstance(),
        };

        private const string OperatorName = "Contains";

        #region IVerb Members

        public string GetOperatorName()
        {
            return OperatorName;
        }

        public bool IsTrue(string subjectValue, string objectValue)
        {
            return subjectValue.Contains(objectValue);
        }

        public bool CanApplyToCondition(IConditionDef conditionCreator)
        {
            return possibleConditionDefList.Contains(conditionCreator);
        }

        #endregion
    }

    public class MatchOperator : IOperator
    {
        private MatchOperator() { }

        private static MatchOperator s_matchOperator = new MatchOperator();

        public static MatchOperator GetInstance()
        {
            return s_matchOperator;
        }

        private static readonly List<IConditionDef> possibleConditionDefList =
            new List<IConditionDef>
        {
            NativeNameConditionDef.GetInstance(),
            GuidConditionDef.GetInstance(),
            NativeParentTypeNameConditionDef.GetInstance(),
            NativeParentFunctionNameConditionDef.GetInstance(),
        };

        private const string operatorName = "Match";

        #region IOperator Members

        public string GetOperatorName()
        {
            return operatorName;
        }

        public bool IsTrue(string subjectValue, string objectValue)
        {
            if (subjectValue != null && objectValue != null)
            {
                Regex regex = new Regex(objectValue);
                Match match = regex.Match(subjectValue);
                return match.Success;
            }
            return false;
        }

        public bool CanApplyToCondition(IConditionDef conditionCreator)
        {
            return possibleConditionDefList.Contains(conditionCreator);
        }

        #endregion
    }

    public class NotEqualOperator : IOperator
    {
        private NotEqualOperator() { }

        private static NotEqualOperator s_notEqualOperator = new NotEqualOperator();

        public static NotEqualOperator GetInstance()
        {
            return s_notEqualOperator;
        }

        private static readonly List<IConditionDef> possibleConditionDefList =
            new List<IConditionDef>
        {
            NativeNameConditionDef.GetInstance(),
            TypeKindConditionDef.GetInstance(),
            GuidConditionDef.GetInstance(),
            NativeParentTypeNameConditionDef.GetInstance(),
            NativeParentFunctionNameConditionDef.GetInstance(),
            NativeParameterIndexConditionDef.GetInstance(),
            NativeSignatureConditionDef.GetInstance(),
        };

        private const string operatorName = "Not Equal";

        #region IOperator Members

        public string GetOperatorName()
        {
            return operatorName;
        }

        public bool IsTrue(string subjectValue, string objectValue)
        {
            if (subjectValue != null && subjectValue.Equals(objectValue))
                return false;
            return true;
        }

        public bool CanApplyToCondition(IConditionDef conditionCreator)
        {
            return possibleConditionDefList.Contains(conditionCreator);
        }

        #endregion
    }

    public class NotContainsOperator : IOperator
    {
        private NotContainsOperator() { }

        private static NotContainsOperator s_notContainsOperator = new NotContainsOperator();

        public static NotContainsOperator GetInstance()
        {
            return s_notContainsOperator;
        }

        private static readonly List<IConditionDef> possibleConditionDefList =
            new List<IConditionDef>
        {
            NativeNameConditionDef.GetInstance(),
            NativeParentTypeNameConditionDef.GetInstance(),
            NativeParentFunctionNameConditionDef.GetInstance(),
        };

        private const string OperatorName = "Not Contains";

        #region IVerb Members

        public string GetOperatorName()
        {
            return OperatorName;
        }

        public bool IsTrue(string subjectValue, string objectValue)
        {
            return !subjectValue.Contains(objectValue);
        }

        public bool CanApplyToCondition(IConditionDef conditionCreator)
        {
            return possibleConditionDefList.Contains(conditionCreator);
        }

        #endregion
    }
}
