using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using CoreRuleEngine;
using TypeLibTypes.Interop;
using System.Globalization;

namespace TlbImpRuleEngine
{
    public class NativeParameterIndexConditionDef : AbstractAtomicConditionDef
    {
        private const string s_conditionName = "NativeParameterIndex";

        private static readonly List<string> s_mostPossibleValues = new List<string> { 
            "0", "1", "2",
        };

        private NativeParameterIndexConditionDef() { }

        private static NativeParameterIndexConditionDef s_paramIndexConditionDef
            = new NativeParameterIndexConditionDef();

        public static NativeParameterIndexConditionDef GetInstance()
        {
            return s_paramIndexConditionDef;
        }

        private static readonly List<ICategory> s_possibleCategoryList = new List<ICategory>
        {
            SignatureCategory.GetInstance(),
        };

        public override List<string> GetMostPossibleValues()
        {
            return s_mostPossibleValues;
        }

        public override ICondition Create()
        {
            return new NativeParameterIndexCondition();
        }

        public override bool CanApplyToCategory(ICategory category)
        {
            return s_possibleCategoryList.Contains(category);
        }

        public override string GetConditionName()
        {
            return s_conditionName;
        }
    }

    public class NativeParameterIndexCondition : AbstractAtomicCondition
    {
        public NativeParameterIndexCondition()
        {
        }

        public override bool Match(IMatchTarget matchTarget)
        {
            int parameterIndex = 0;
            if (matchTarget is SignatureInfoMatchTarget)
            {
                SignatureInfoMatchTarget sigInfoMatchTarget = matchTarget as SignatureInfoMatchTarget;
                parameterIndex = sigInfoMatchTarget.NativeParameterIndex;
            }
            else
            {
                throw new CannotApplyConditionToMatchTargetException(this, matchTarget);
            }
            return m_operator.IsTrue("" + parameterIndex, m_value);
        }

        public override IConditionDef GetConditionDef()
        {
            return NativeParameterIndexConditionDef.GetInstance();
        }

        public override string GetExpression()
        {
            return GetConditionDef().GetConditionName() + " " +
                m_operator.GetOperatorName() + " '" +
                GetParameterIndexText(
                    Int32.Parse(m_value, CultureInfo.InvariantCulture)) + "'";
        }

        public static string GetParameterIndexText(int index)
        {
            if (index == 0)
            {
                return "return";
            }
            else if (index % 10 == 1 && index != 11)
            {
                return index + "st parameter";
            }
            else if (index % 10 == 2 && index != 12)
            {
                return index + "nd parameter";
            }
            else if (index % 10 == 3 && index != 13)
            {
                return index + "rd parameter";
            }
            else
            {
                return index + "th parameter";
            }
        }
    }
}
