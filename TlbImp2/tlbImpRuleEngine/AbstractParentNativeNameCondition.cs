using System;
using System.Collections.Generic;
using System.Text;
using CoreRuleEngine;
using TypeLibTypes.Interop;

namespace TlbImpRuleEngine
{
    public abstract class AbstractParentNativeNameConditionDef : AbstractAtomicConditionDef
    {
        public override List<string> GetMostPossibleValues()
        {
            return null;
        }
    }

    public abstract class AbstractParentNativeNameCondition : AbstractAtomicCondition
    {
        public override bool Match(IMatchTarget matchTarget)
        {
            string parentName;
            if (matchTarget is IGetNativeParentName)
            {
                IGetNativeParentName getParentNativeName =
                    matchTarget as IGetNativeParentName;
                parentName = getParentNativeName.GetNativeParentName();
            }
            else
            {
                throw new IGetParentNativeNameUnsupportedException(matchTarget);
            }
            return m_operator.IsTrue(parentName, m_value);
        }
    }
}
