using System;
using System.Collections.Generic;
using System.Text;
using TypeLibTypes.Interop;
using System.Runtime.InteropServices;

namespace TlbImpRuleFileEditor
{
    //******************************************************************************
    // Enum passed in to LoadTypeLibEx.
    //******************************************************************************
    [Flags]
    public enum REGKIND
    {
        REGKIND_DEFAULT = 0,
        REGKIND_REGISTER = 1,
        REGKIND_NONE = 2,
        REGKIND_LOAD_TLB_AS_32BIT = 0x20,
        REGKIND_LOAD_TLB_AS_64BIT = 0x40,
    }

    public class APIHelper
    {
        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void LoadTypeLibEx(String strTypeLibName,
            REGKIND regKind, out System.Runtime.InteropServices.ComTypes.ITypeLib TypeLib);
    }
}
