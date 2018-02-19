using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using CoreRuleEngine;

namespace TlbImpRuleEngine
{
    public class NativeParentTypeNameConditionDef : AbstractParentNativeNameConditionDef
    {
        private const string s_conditionName = "NativeParentTypeName";

        private NativeParentTypeNameConditionDef() { }

        private static NativeParentTypeNameConditionDef s_nativeParentTypeNameConditionDef
            = new NativeParentTypeNameConditionDef();

        public static NativeParentTypeNameConditionDef GetInstance()
        {
            return s_nativeParentTypeNameConditionDef;
        }

        private static readonly List<ICategory> s_possibleCategoryList = new List<ICategory>
        {
            FunctionCategory.GetInstance(),
            FieldCategory.GetInstance(),
        };

        public override ICondition Create()
        {
            return new NativeParentTypeNameCondition();
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

    public class NativeParentTypeNameCondition : AbstractParentNativeNameCondition
    {
        public NativeParentTypeNameCondition()
        {
        }

        public override IConditionDef GetConditionDef()
        {
            return NativeParentTypeNameConditionDef.GetInstance();
        }
    }

}
