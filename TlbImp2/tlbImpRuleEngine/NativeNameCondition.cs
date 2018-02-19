using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using CoreRuleEngine;
using TypeLibTypes.Interop;

namespace TlbImpRuleEngine
{
    public class NativeNameConditionDef : AbstractAtomicConditionDef
    {
        private const string s_conditionName = "NativeName";

        private NativeNameConditionDef() { }

        private static NativeNameConditionDef s_nativeNameConditionDef
            = new NativeNameConditionDef();

        public static NativeNameConditionDef GetInstance()
        {
            return s_nativeNameConditionDef;
        }

        private static readonly List<ICategory> s_possibleCategoryList = new List<ICategory>
        {
            FunctionCategory.GetInstance(),
            FieldCategory.GetInstance(),
            TypeCategory.GetInstance(),
        };

        public override List<string> GetMostPossibleValues()
        {
            return null;
        }

        public override ICondition Create()
        {
            return new NativeNameCondition();
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

    public class NativeNameCondition : AbstractAtomicCondition
    {
        public NativeNameCondition()
        {
        }

        public override bool Match(IMatchTarget matchTarget)
        {
            string name;
            if (matchTarget is IGetTypeLibElementCommonInfo)
            {
                name = (matchTarget as IGetTypeLibElementCommonInfo).Name;
            }
            else
            {
                throw new CannotApplyConditionToMatchTargetException(this, matchTarget);
            }
            return m_operator.IsTrue(name, m_value);
        }

        public override IConditionDef GetConditionDef()
        {
            return NativeNameConditionDef.GetInstance();
        }
    }

}
