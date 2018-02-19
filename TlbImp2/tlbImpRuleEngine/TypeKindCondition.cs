using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using CoreRuleEngine;
using TypeLibTypes.Interop;

namespace TlbImpRuleEngine
{
    public class TypeKindConditionDef : AbstractAtomicConditionDef
    {
        private const string s_conditionName = "TypeKind";

        private static readonly List<string> s_mostPossibleValues = new List<string> { 
            "Interface", "DispatchInterface", "CoClass", "Struct",
            "Union", "Enum", "Module", "Alias", 
        };

        private TypeKindConditionDef() { }

        private static TypeKindConditionDef s_typeKindConditionDef
            = new TypeKindConditionDef();

        public static TypeKindConditionDef GetInstance()
        {
            return s_typeKindConditionDef;
        }

        private static readonly List<ICategory> s_possibleCategoryList = new List<ICategory>
        {
            TypeCategory.GetInstance(),
        };

        public override List<string> GetMostPossibleValues()
        {
            return s_mostPossibleValues;
        }

        public override ICondition Create()
        {
            return new TypeKindCondition();
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

    public class TypeKindCondition : AbstractAtomicCondition
    {
        public TypeKindCondition()
        {
        }

        public override bool Match(IMatchTarget matchTarget)
        {
            TYPEKIND typeKind;
            if (matchTarget is TypeInfoMatchTarget)
            {
                TypeInfoMatchTarget typeInfoMatchTarget = matchTarget as TypeInfoMatchTarget;
                typeKind = typeInfoMatchTarget.TypeKind;
            }
            else
            {
                throw new CannotApplyConditionToMatchTargetException(this, matchTarget);
            }
            return m_operator.IsTrue(TypeLibUtility.TypeKind2String(typeKind), m_value);
        }

        public override IConditionDef GetConditionDef()
        {
            return TypeKindConditionDef.GetInstance();
        }
    }

}
