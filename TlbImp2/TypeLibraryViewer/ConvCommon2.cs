using System;
using System.Collections.Generic;
using System.Text;
using TypeLibTypes.Interop;
using System.Runtime.InteropServices;

namespace TypeLibraryTreeView
{
    public class ConvCommon2
    {
        /// <summary>
        /// This function is used to workaround around the fact that the TypeInfo might return IUnknown/IDispatch methods (in the case of dual interfaces)
        /// So we should always call this function to get the first index for different TypeInfo and never save the id
        /// </summary>
        static public int GetIndexOfFirstMethod(TypeInfo type, TypeAttr attr)
        {
            if (attr.typekind != TypeLibTypes.Interop.TYPEKIND.TKIND_DISPATCH) return 0;

            int nIndex = 0;
            if (attr.cFuncs >= 3)
            {
                // Check for IUnknown first
                using (FuncDesc func = type.GetFuncDesc(0))
                {
                    if (func.memid == 0x60000000 &&
                       func.elemdescFunc.tdesc.vt == (int)VarEnum.VT_VOID &&
                       func.cParams == 2 &&
                       func.GetElemDesc(0).tdesc.vt == (int)VarEnum.VT_PTR &&
                       func.GetElemDesc(1).tdesc.vt == (int)VarEnum.VT_PTR &&
                       "QueryInterface" == type.GetDocumentation(func.memid))
                    {
                        nIndex = 3;
                    }
                }

                if (attr.cFuncs >= 7)
                {
                    using (FuncDesc func = type.GetFuncDesc(3))
                    {
                        // Check IDispatch
                        if (func.memid == 0x60010000 &&
                            func.elemdescFunc.tdesc.vt == (int)VarEnum.VT_VOID &&
                            func.cParams == 1 &&
                            func.GetElemDesc(0).tdesc.vt == (int)VarEnum.VT_PTR &&
                            "GetTypeInfoCount" == type.GetDocumentation(func.memid))
                        {
                            nIndex = 7;
                        }
                    }
                }
            }
            return nIndex;
        }

        /// <summary>
        /// If the type is aliased, return the ultimated non-aliased type if the type is user-defined, otherwise, return
        /// the aliased type directly. So the result could still be aliased to a built-in type.
        /// If the type is not aliased, just return the type directly
        /// </summary>
        static public void ResolveAlias(TypeInfo type, TypeDesc typeDesc, out TypeInfo realType, out TypeAttr realAttr)
        {
            if ((VarEnum)typeDesc.vt != VarEnum.VT_USERDEFINED)
            {
                // Already resolved
                realType = type;
                realAttr = type.GetTypeAttr();
                return;
            }
            else
            {
                TypeInfo refType = type.GetRefTypeInfo(typeDesc.hreftype);
                TypeAttr refAttr = refType.GetTypeAttr();

                // If the userdefined typeinfo is not itself an alias, then it is what the alias aliases.
                // Also, if the userdefined typeinfo is an alias to a builtin type, then the builtin
                // type is what the alias aliases.
                if (refAttr.typekind != TypeLibTypes.Interop.TYPEKIND.TKIND_ALIAS || (VarEnum)refAttr.tdescAlias.vt != VarEnum.VT_USERDEFINED)
                {
                    // Resolved
                    realType = refType;
                    realAttr = refAttr;
                }
                else
                {
                    // Continue resolving the type
                    ResolveAlias(refType, refAttr.tdescAlias, out realType, out realAttr);
                }
            }
        }
    }
}
