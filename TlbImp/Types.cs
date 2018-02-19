///////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation. All rights reserved.
///////////////////////////////////////////////////////////////////////////////
//
// Type Library Importer utility
//
// This program imports all the types in the type library into a interop assembly
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

// I created this file because System.Runtime.InteropServices.ComTypes is busted.

namespace tlbimp2
{
    [Serializable]
    public enum TYPEKIND
    {
        TKIND_ENUM = 0,
        TKIND_RECORD = TKIND_ENUM + 1,
        TKIND_MODULE = TKIND_RECORD + 1,
        TKIND_INTERFACE = TKIND_MODULE + 1,
        TKIND_DISPATCH = TKIND_INTERFACE + 1,
        TKIND_COCLASS = TKIND_DISPATCH + 1,
        TKIND_ALIAS = TKIND_COCLASS + 1,
        TKIND_UNION = TKIND_ALIAS + 1,
        TKIND_MAX = TKIND_UNION + 1
    }

    [Serializable, Flags()]
    public enum TYPEFLAGS : short
    {
        TYPEFLAG_FAPPOBJECT = 0x1,
        TYPEFLAG_FCANCREATE = 0x2,
        TYPEFLAG_FLICENSED = 0x4,
        TYPEFLAG_FPREDECLID = 0x8,
        TYPEFLAG_FHIDDEN = 0x10,
        TYPEFLAG_FCONTROL = 0x20,
        TYPEFLAG_FDUAL = 0x40,
        TYPEFLAG_FNONEXTENSIBLE = 0x80,
        TYPEFLAG_FOLEAUTOMATION = 0x100,
        TYPEFLAG_FRESTRICTED = 0x200,
        TYPEFLAG_FAGGREGATABLE = 0x400,
        TYPEFLAG_FREPLACEABLE = 0x800,
        TYPEFLAG_FDISPATCHABLE = 0x1000,
        TYPEFLAG_FREVERSEBIND = 0x2000,
        TYPEFLAG_FPROXY = 0x4000
    }

    [Serializable, Flags()]
    public enum IMPLTYPEFLAGS
    {
        IMPLTYPEFLAG_FDEFAULT = 0x1,
        IMPLTYPEFLAG_FSOURCE = 0x2,
        IMPLTYPEFLAG_FRESTRICTED = 0x4,
        IMPLTYPEFLAG_FDEFAULTVTABLE = 0x8,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    [Serializable]
    public struct TYPEATTR
    {
        // Constant used with the memid fields.
        public const int MEMBER_ID_NIL = unchecked((int)0xFFFFFFFF);

        // Actual fields of the TypeAttr struct.
        public Guid guid;
        public Int32 lcid;
        public Int32 dwReserved;
        public Int32 memidConstructor;
        public Int32 memidDestructor;
        public IntPtr lpstrSchema;
        public Int32 cbSizeInstance;
        public TYPEKIND typekind;
        public Int16 cFuncs;
        public Int16 cVars;
        public Int16 cImplTypes;
        public Int16 cbSizeVft;
        public Int16 cbAlignment;
        public TYPEFLAGS wTypeFlags;
        public Int16 wMajorVerNum;
        public Int16 wMinorVerNum;
        public TYPEDESC tdescAlias;
        public IDLDESC idldescType;
    }

    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct FUNCDESC
    {
        public int memid;                   //MEMBERID memid;
        public IntPtr lprgscode;            // /* [size_is(cScodes)] */ SCODE RPC_FAR *lprgscode;
        public IntPtr lprgelemdescParam;    // /* [size_is(cParams)] */ ELEMDESC __RPC_FAR *lprgelemdescParam;
        public FUNCKIND funckind;           //FUNCKIND funckind;
        public INVOKEKIND invkind;          //INVOKEKIND invkind;
        public CALLCONV callconv;           //CALLCONV callconv;
        public Int16 cParams;               //short cParams;
        public Int16 cParamsOpt;            //short cParamsOpt;
        public Int16 oVft;                  //short oVft;
        public Int16 cScodes;               //short cScodes;
        public ELEMDESC elemdescFunc;       //ELEMDESC elemdescFunc;
        public Int16 wFuncFlags;            //WORD wFuncFlags;
    }

    [Serializable, Flags()]
    public enum IDLFLAG : short
    {
        IDLFLAG_NONE = PARAMFLAG.PARAMFLAG_NONE,
        IDLFLAG_FIN = PARAMFLAG.PARAMFLAG_FIN,
        IDLFLAG_FOUT = PARAMFLAG.PARAMFLAG_FOUT,
        IDLFLAG_FLCID = PARAMFLAG.PARAMFLAG_FLCID,
        IDLFLAG_FRETVAL = PARAMFLAG.PARAMFLAG_FRETVAL
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    [Serializable]
    public struct IDLDESC
    {
        public IntPtr dwReserved;
        public IDLFLAG wIDLFlags;
    }

    [Serializable, Flags()]
    public enum PARAMFLAG : short
    {
        PARAMFLAG_NONE = 0,
        PARAMFLAG_FIN = 0x1,
        PARAMFLAG_FOUT = 0x2,
        PARAMFLAG_FLCID = 0x4,
        PARAMFLAG_FRETVAL = 0x8,
        PARAMFLAG_FOPT = 0x10,
        PARAMFLAG_FHASDEFAULT = 0x20,
        PARAMFLAG_FHASCUSTDATA = 0x40
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    [Serializable]
    public struct PARAMDESC
    {
        public IntPtr lpVarValue;
        public PARAMFLAG wParamFlags;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    [Serializable]
    public struct TYPEDESC
    {
        [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        [Serializable]
        public struct DESCUNION
        {
            [FieldOffset(0)]
            public IntPtr lptdesc;    // VT_SAFEARRAY or VT_PTR, this points to a TYPEDESC *
            [FieldOffset(0)]
            public IntPtr lpadesc;    // ARRAYDESC if VT==VT_CARRAY
            [FieldOffset(0)]
            public int hreftype;    // VT_USERDEFINED, this is HREFTYPE to be passed to ITypeInfo::GetRefTypeInfo
        };
        public DESCUNION desc;
        public Int16 vt;
    }

    /// <summary>
    /// ARRAYDESC
    /// It has multiple entries for SAFEARRAYBOUND that cannot be represented in managed code
    /// So omit the SAFEARRAYBOUND and we only use ARRAYDESC to calculate the offset to avoid any platform dependencies
    /// such as alignment
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    [Serializable]
    public struct ARRAYDESC
    {
        public TYPEDESC tdescElem;
        public short cDims;
    }

    /*
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ARRAYDESC
    {
        TYPEDESC tdescelem;
        ushort cDims;
        [MarshalAs(UnmanagedType.ByValArray, Size
        SAFEARRAYBOUND[]
    }
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct SAFEARRAYBOUND
    {
        public uint cElements;
        public int lBound;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    [Serializable]
    public struct ELEMDESC
    {
        public TYPEDESC tdesc;

        [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        [Serializable]
        public struct DESCUNION
        {
            [FieldOffset(0)]
            public IDLDESC idldesc;
            [FieldOffset(0)]
            public PARAMDESC paramdesc;
        };
        public DESCUNION desc;
    }

    [Serializable]
    public enum VARKIND : int
    {
        VAR_PERINSTANCE = 0x0,
        VAR_STATIC = 0x1,
        VAR_CONST = 0x2,
        VAR_DISPATCH = 0x3
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    [Serializable]
    public struct VARDESC
    {
        public int memid;
        public IntPtr lpstrSchema;

        [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        [Serializable]
        public struct DESCUNION
        {
            [FieldOffset(0)]
            public int oInst;
            [FieldOffset(0)]
            public IntPtr lpvarValue;
        };

        public DESCUNION desc;

        public ELEMDESC elemdescVar;
        public short wVarFlags;
        public VARKIND varkind;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    [Serializable]
    public struct DISPPARAMS
    {
        public IntPtr rgvarg;
        public IntPtr rgdispidNamedArgs;
        public int cArgs;
        public int cNamedArgs;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    [Serializable]
    public struct EXCEPINFO
    {
        public Int16 wCode;
        public Int16 wReserved;
        public IntPtr bstrSource;
        public IntPtr bstrDescription;
        public IntPtr bstrHelpFile;
        public int dwHelpContext;
        public IntPtr pvReserved;
        public IntPtr pfnDeferredFillIn;
        public Int32 scode;
    }

    [Serializable]
    public enum FUNCKIND : int
    {
        FUNC_VIRTUAL = 0,
        FUNC_PUREVIRTUAL = 1,
        FUNC_NONVIRTUAL = 2,
        FUNC_STATIC = 3,
        FUNC_DISPATCH = 4
    }

    [Serializable, Flags]
    public enum INVOKEKIND : int
    {
        INVOKE_FUNC = 0x1,
        INVOKE_PROPERTYGET = 0x2,
        INVOKE_PROPERTYPUT = 0x4,
        INVOKE_PROPERTYPUTREF = 0x8
    }

    [Serializable]
    public enum CALLCONV : int
    {
        CC_CDECL = 1,
        CC_MSCPASCAL = 2,
        CC_PASCAL = CC_MSCPASCAL,
        CC_MACPASCAL = 3,
        CC_STDCALL = 4,
        CC_RESERVED = 5,
        CC_SYSCALL = 6,
        CC_MPWCDECL = 7,
        CC_MPWPASCAL = 8,
        CC_MAX = 9
    }

    [Serializable, Flags()]
    public enum FUNCFLAGS : short
    {
        FUNCFLAG_FRESTRICTED = 0x1,
        FUNCFLAG_FSOURCE = 0x2,
        FUNCFLAG_FBINDABLE = 0x4,
        FUNCFLAG_FREQUESTEDIT = 0x8,
        FUNCFLAG_FDISPLAYBIND = 0x10,
        FUNCFLAG_FDEFAULTBIND = 0x20,
        FUNCFLAG_FHIDDEN = 0x40,
        FUNCFLAG_FUSESGETLASTERROR = 0x80,
        FUNCFLAG_FDEFAULTCOLLELEM = 0x100,
        FUNCFLAG_FUIDEFAULT = 0x200,
        FUNCFLAG_FNONBROWSABLE = 0x400,
        FUNCFLAG_FREPLACEABLE = 0x800,
        FUNCFLAG_FIMMEDIATEBIND = 0x1000
    }

    [Serializable, Flags()]
    public enum VARFLAGS : short
    {
        VARFLAG_FREADONLY = 0x1,
        VARFLAG_FSOURCE = 0x2,
        VARFLAG_FBINDABLE = 0x4,
        VARFLAG_FREQUESTEDIT = 0x8,
        VARFLAG_FDISPLAYBIND = 0x10,
        VARFLAG_FDEFAULTBIND = 0x20,
        VARFLAG_FHIDDEN = 0x40,
        VARFLAG_FRESTRICTED = 0x80,
        VARFLAG_FDEFAULTCOLLELEM = 0x100,
        VARFLAG_FUIDEFAULT = 0x200,
        VARFLAG_FNONBROWSABLE = 0x400,
        VARFLAG_FREPLACEABLE = 0x800,
        VARFLAG_FIMMEDIATEBIND = 0x1000
    }

    [Guid("00020401-0000-0000-C000-000000000046")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ITypeInfo
    {
        void GetTypeAttr(out IntPtr ppTypeAttr);
        void GetTypeComp(out ITypeComp ppTComp);
        void GetFuncDesc(int index, out IntPtr ppFuncDesc);
        void GetVarDesc(int index, out IntPtr ppVarDesc);
        void GetNames(int memid, IntPtr rgBstrNames, int cMaxNames, out int pcNames);

        // Avoid exception being throw because we need to try to see if we have "partner" interfaces and we want to avoid try/catch in this case
        [PreserveSig] 
        int GetRefTypeOfImplType(int index, out int href);
        void GetImplTypeFlags(int index, out IMPLTYPEFLAGS pImplTypeFlags);
        void GetIDsOfNames([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1), In] String[] rgszNames, int cNames, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1), Out] int[] pMemId);
        void Invoke([MarshalAs(UnmanagedType.IUnknown)] Object pvInstance, int memid, Int16 wFlag, ref DISPPARAMS pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, out int puArgErr);
        // In order to be able to pass NULL to GetDocumentation, use IntPtr instead of out <type>
        void GetDocumentation(int index, out String strName, IntPtr pstrDocString, IntPtr pdwHelpContext, IntPtr pstrHelpFile);
        void GetDllEntry(int memid, INVOKEKIND invKind, IntPtr pBstrDllName, IntPtr pBstrName, IntPtr pwOrdinal);
        void GetRefTypeInfo(int hRef, out ITypeInfo ppTI);
        void AddressOfMember(int memid, INVOKEKIND invKind, out IntPtr ppv);
        void CreateInstance([MarshalAs(UnmanagedType.IUnknown)] Object pUnkOuter, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown), Out] out Object ppvObj);
        void GetMops(int memid, out String pBstrMops);
        void GetContainingTypeLib(out ITypeLib ppTLB, out int pIndex);
        [PreserveSig]
        void ReleaseTypeAttr(IntPtr pTypeAttr);
        [PreserveSig]
        void ReleaseFuncDesc(IntPtr pFuncDesc);
        [PreserveSig]
        void ReleaseVarDesc(IntPtr pVarDesc);
    }

    [Guid("00020412-0000-0000-C000-000000000046")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ITypeInfo2 : ITypeInfo
    {
        new void GetTypeAttr(out IntPtr ppTypeAttr);
        new void GetTypeComp(out ITypeComp ppTComp);
        new void GetFuncDesc(int index, out IntPtr ppFuncDesc);
        new void GetVarDesc(int index, out IntPtr ppVarDesc);
        new void GetNames(int memid, IntPtr rgBstrNames, int cMaxNames, out int pcNames);
        // Avoid exception being throw because we need to try to see if we have "partner" interfaces and we want to avoid try/catch in this case
        [PreserveSig]
        new int GetRefTypeOfImplType(int index, out int href);
        new void GetImplTypeFlags(int index, out IMPLTYPEFLAGS pImplTypeFlags);
        new void GetIDsOfNames([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1), In] String[] rgszNames, int cNames, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1), Out] int[] pMemId);
        new void Invoke([MarshalAs(UnmanagedType.IUnknown)] Object pvInstance, int memid, Int16 wFlags, ref DISPPARAMS pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, out int puArgErr);
        // In order to be able to pass NULL to GetDocumentation, use IntPtr instead of out <type>
        new void GetDocumentation(int index, out String strName, IntPtr pstrDocString, IntPtr pdwHelpContext, IntPtr pstrHelpFile);
        new void GetDllEntry(int memid, INVOKEKIND invKind, IntPtr pBstrDllName, IntPtr pBstrName, IntPtr pwOrdinal);
        new void GetRefTypeInfo(int hRef, out ITypeInfo ppTI);
        new void AddressOfMember(int memid, INVOKEKIND invKind, out IntPtr ppv);
        new void CreateInstance([MarshalAs(UnmanagedType.IUnknown)] Object pUnkOuter, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown), Out] out Object ppvObj);
        new void GetMops(int memid, out String pBstrMops);
        new void GetContainingTypeLib(out ITypeLib ppTLB, out int pIndex);
        [PreserveSig]
        new void ReleaseTypeAttr(IntPtr pTypeAttr);
        [PreserveSig]
        new void ReleaseFuncDesc(IntPtr pFuncDesc);
        [PreserveSig]
        new void ReleaseVarDesc(IntPtr pVarDesc);
        void GetTypeKind(out TYPEKIND pTypeKind);
        void GetTypeFlags(out int pTypeFlags);
        void GetFuncIndexOfMemId(int memid, INVOKEKIND invKind, out int pFuncIndex);
        void GetVarIndexOfMemId(int memid, out int pVarIndex);
        [PreserveSig]
        int GetCustData(ref Guid guid, out Object pVarVal);
        [PreserveSig]
        int GetFuncCustData(int index, ref Guid guid, out Object pVarVal);
        [PreserveSig]
        int GetParamCustData(int indexFunc, int indexParam, ref Guid guid, out Object pVarVal);
        [PreserveSig]
        int GetVarCustData(int index, ref Guid guid, out Object pVarVal);
        [PreserveSig]
        int GetImplTypeCustData(int index, ref Guid guid, out Object pVarVal);
        [LCIDConversionAttribute(1)]
        void GetDocumentation2(int memid, out String pbstrHelpString, out int pdwHelpStringContext, out String pbstrHelpStringDll);
        void GetAllCustData(IntPtr pCustData);
        void GetAllFuncCustData(int index, IntPtr pCustData);
        void GetAllParamCustData(int indexFunc, int indexParam, IntPtr pCustData);
        void GetAllVarCustData(int index, IntPtr pCustData);
        void GetAllImplTypeCustData(int index, IntPtr pCustData);
    }

    [Serializable]
    public enum SYSKIND
    {
        SYS_WIN16 = 0,
        SYS_WIN32 = SYS_WIN16 + 1,
        SYS_MAC = SYS_WIN32 + 1,
        SYS_WIN64 = SYS_MAC + 1
    }

    [Serializable, Flags()]
    public enum LIBFLAGS : short
    {
        LIBFLAG_FRESTRICTED = 0x1,
        LIBFLAG_FCONTROL = 0x2,
        LIBFLAG_FHIDDEN = 0x4,
        LIBFLAG_FHASDISKIMAGE = 0x8
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    [Serializable]
    public struct TYPELIBATTR
    {
        public Guid guid;
        public int lcid;
        public SYSKIND syskind;
        public Int16 wMajorVerNum;
        public Int16 wMinorVerNum;
        public LIBFLAGS wLibFlags;
    }

    [Guid("00020402-0000-0000-C000-000000000046")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ITypeLib
    {
        [PreserveSig]
        int GetTypeInfoCount();
        void GetTypeInfo(int index, out ITypeInfo ppTI);
        void GetTypeInfoType(int index, out TYPEKIND pTKind);
        void GetTypeInfoOfGuid(ref Guid guid, out ITypeInfo ppTInfo);
        void GetLibAttr(out IntPtr ppTLibAttr);
        void GetTypeComp(out ITypeComp ppTComp);
        void GetDocumentation(int index, out String strName, out String strDocString, out int dwHelpContext, out String strHelpFile);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsName([MarshalAs(UnmanagedType.LPWStr)] String szNameBuf, int lHashVal);
        void FindName([MarshalAs(UnmanagedType.LPWStr)] String szNameBuf, int lHashVal, [MarshalAs(UnmanagedType.LPArray), Out] ITypeInfo[] ppTInfo, [MarshalAs(UnmanagedType.LPArray), Out] int[] rgMemId, ref Int16 pcFound);
        [PreserveSig]
        void ReleaseTLibAttr(IntPtr pTLibAttr);
    }

    [Guid("00020411-0000-0000-C000-000000000046")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ITypeLib2 : ITypeLib
    {
        #region ITypeLib members
        [PreserveSig]
        new int GetTypeInfoCount();
        new void GetTypeInfo(int index, out ITypeInfo ppTI);
        new void GetTypeInfoType(int index, out TYPEKIND pTKind);
        new void GetTypeInfoOfGuid(ref Guid guid, out ITypeInfo ppTInfo);
        new void GetLibAttr(out IntPtr ppTLibAttr);
        new void GetTypeComp(out ITypeComp ppTComp);
        new void GetDocumentation(int index, out String strName, out String strDocString, out int dwHelpContext, out String strHelpFile);
        [return: MarshalAs(UnmanagedType.Bool)]
        new bool IsName([MarshalAs(UnmanagedType.LPWStr)] String szNameBuf, int lHashVal);
        new void FindName([MarshalAs(UnmanagedType.LPWStr)] String szNameBuf, int lHashVal, [MarshalAs(UnmanagedType.LPArray), Out] ITypeInfo[] ppTInfo, [MarshalAs(UnmanagedType.LPArray), Out] int[] rgMemId, ref Int16 pcFound);
        [PreserveSig]
        new void ReleaseTLibAttr(IntPtr pTLibAttr);
        #endregion

        #region ITypeLib2 members
        void GetCustData(ref Guid guid, out object pVarVal);
        // ... omit
        #endregion
    }
}
