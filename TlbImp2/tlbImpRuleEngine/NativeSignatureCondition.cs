using System;
using System.Collections.Generic;
using System.Text;
using CoreRuleEngine;
using TypeLibTypes.Interop;

namespace TlbImpRuleEngine
{

    public class NativeSignatureConditionDef : AbstractAtomicConditionDef
    {
        private const string s_conditionName = "NativeSignature";

        private NativeSignatureConditionDef() { }

        private static NativeSignatureConditionDef s_nativeSignatureConditionDef
            = new NativeSignatureConditionDef();

        public static NativeSignatureConditionDef GetInstance()
        {
            return s_nativeSignatureConditionDef;
        }

        private static readonly List<ICategory> s_possibleCategoryList = new List<ICategory>
        {
            SignatureCategory.GetInstance(),
        };

        public override List<string> GetMostPossibleValues()
        {
            return null;
        }

        public override ICondition Create()
        {
            return new NativeSignatureCondition();
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

    public class NativeSignatureCondition : AbstractAtomicCondition
    {
        public NativeSignatureCondition()
        {
        }

        public override bool Match(IMatchTarget matchTarget)
        {
            string nativeSignature;
            if (matchTarget is SignatureInfoMatchTarget)
            {
                SignatureInfoMatchTarget sigInfoMatchTarget = matchTarget as SignatureInfoMatchTarget;
                nativeSignature = sigInfoMatchTarget.NativeSignature;
            }
            else
            {
                throw new CannotApplyConditionToMatchTargetException(this, matchTarget);
            }
            return m_operator.IsTrue("" + nativeSignature, m_value);
        }

        public override IConditionDef GetConditionDef()
        {
            return NativeSignatureConditionDef.GetInstance();
        }
    }

}
