using System;
using System.Collections.Generic;
using System.Text;

namespace TypeLibTypes.Interop
{
    public class TypeLibUtility
    {
        public static string TypeKind2String(TypeLibTypes.Interop.TYPEKIND typeKind)
        {
            string typeKindString = null;
            switch (typeKind)
            {
                case TypeLibTypes.Interop.TYPEKIND.TKIND_ALIAS:
                    typeKindString = "Alias";
                    break;
                case TypeLibTypes.Interop.TYPEKIND.TKIND_COCLASS:
                    typeKindString = "CoClass";
                    break;
                case TypeLibTypes.Interop.TYPEKIND.TKIND_DISPATCH:
                    typeKindString = "DispatchInterface";
                    break;
                case TypeLibTypes.Interop.TYPEKIND.TKIND_ENUM:
                    typeKindString = "Enum";
                    break;
                case TypeLibTypes.Interop.TYPEKIND.TKIND_INTERFACE:
                    typeKindString = "Interface";
                    break;
                case TypeLibTypes.Interop.TYPEKIND.TKIND_MODULE:
                    typeKindString = "Module";
                    break;
                case TypeLibTypes.Interop.TYPEKIND.TKIND_RECORD:
                    typeKindString = "Struct";
                    break;
                case TypeLibTypes.Interop.TYPEKIND.TKIND_UNION:
                    typeKindString = "Union";
                    break;
            }
            return typeKindString;
        }
    }
}
