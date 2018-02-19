using System;
using System.Collections.Generic;
using System.Text;
using CoreRuleEngine;

namespace TlbImpRuleEngine
{
    public class TlbImpConditionManager : AbstractConditionManager
    {
        public TlbImpConditionManager()
        {
            NativeNameConditionDef nativeNameConditionDef = NativeNameConditionDef.GetInstance();
            RegisterCondition(nativeNameConditionDef.GetConditionName(), nativeNameConditionDef);
            
            TypeKindConditionDef typeKindConditionDef = TypeKindConditionDef.GetInstance();
            RegisterCondition(typeKindConditionDef.GetConditionName(), typeKindConditionDef);
            
            GuidConditionDef guidConditionDef = GuidConditionDef.GetInstance();
            RegisterCondition(guidConditionDef.GetConditionName(), guidConditionDef);
            
            NativeParentTypeNameConditionDef nativeParentTypeNameConditionDef = 
                NativeParentTypeNameConditionDef.GetInstance();
            RegisterCondition(nativeParentTypeNameConditionDef.GetConditionName(),
                nativeParentTypeNameConditionDef);

            NativeParentFunctionNameConditionDef nativeParentFunctionNameConditionDef =
                NativeParentFunctionNameConditionDef.GetInstance();
            RegisterCondition(nativeParentFunctionNameConditionDef.GetConditionName(),
                nativeParentFunctionNameConditionDef);

            NativeParameterIndexConditionDef parameterIndexConditionDef =
                NativeParameterIndexConditionDef.GetInstance();
            RegisterCondition(parameterIndexConditionDef.GetConditionName(),
                parameterIndexConditionDef);

            NativeSignatureConditionDef nativeSignatureConditionDef =
                NativeSignatureConditionDef.GetInstance();
            RegisterCondition(nativeSignatureConditionDef.GetConditionName(),
                nativeSignatureConditionDef);
        }
    }

}
