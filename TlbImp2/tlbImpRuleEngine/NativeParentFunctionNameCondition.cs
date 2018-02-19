using System;
using System.Collections.Generic;
using System.Text;
using CoreRuleEngine;

namespace TlbImpRuleEngine
{
    public class NativeParentFunctionNameConditionDef : AbstractParentNativeNameConditionDef
    {
        private const string s_conditionName = "NativeParentFunctionName";

        private NativeParentFunctionNameConditionDef() { }

        private static NativeParentFunctionNameConditionDef s_nativeParentFunctionNameConditionDef
            = new NativeParentFunctionNameConditionDef();

        public static NativeParentFunctionNameConditionDef GetInstance()
        {
            return s_nativeParentFunctionNameConditionDef;
        }

        private static readonly List<ICategory> s_possibleCategoryList = new List<ICategory>
        {
            SignatureCategory.GetInstance(),
        };

        public override ICondition Create()
        {
            return new NativeParentFunctionNameCondition();
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

    public class NativeParentFunctionNameCondition : AbstractParentNativeNameCondition
    {
        public NativeParentFunctionNameCondition()
        {
        }

        public override IConditionDef GetConditionDef()
        {
            return NativeParentFunctionNameConditionDef.GetInstance();
        }
    }
}
