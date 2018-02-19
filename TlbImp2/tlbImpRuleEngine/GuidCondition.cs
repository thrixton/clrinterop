using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using CoreRuleEngine;
using TypeLibTypes.Interop;
using System.Globalization;

namespace TlbImpRuleEngine
{
    public class GuidConditionDef : AbstractAtomicConditionDef
    {
        private const string s_conditionName = "GUID";

        private GuidConditionDef() { }

        private static GuidConditionDef s_guidConditionDef
            = new GuidConditionDef();

        public static GuidConditionDef GetInstance()
        {
            return s_guidConditionDef;
        }

        private static readonly List<ICategory> s_possibleCategoryList = new List<ICategory>
        {
            TypeCategory.GetInstance(),
        };

        private static readonly List<string> s_mostPossibleValues = new List<string> { 
            "00000000-0000-0000-0000-000000000000"
        };

        public override List<string> GetMostPossibleValues()
        {
            return s_mostPossibleValues;
        }

        public override ICondition Create()
        {
            return new GuidCondition();
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

    public class GuidCondition : AbstractAtomicCondition
    {
        public GuidCondition()
        {
        }

        public override bool Match(IMatchTarget matchTarget)
        {
            Guid guid;
            if (matchTarget is TypeInfoMatchTarget)
            {
                TypeInfoMatchTarget typeInfoMatchTarget = matchTarget as TypeInfoMatchTarget;
                guid = typeInfoMatchTarget.GUID;
            }
            else
            {
                throw new CannotApplyConditionToMatchTargetException(this, matchTarget);
            }
            return m_operator.IsTrue(
                guid.ToString().ToUpper(CultureInfo.InvariantCulture),
                m_value.ToUpper(CultureInfo.InvariantCulture));
        }

        public override IConditionDef GetConditionDef()
        {
            return GuidConditionDef.GetInstance();
        }
    }

}
