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
using System.Diagnostics;

namespace tlbimp2
{
    class Utils
    {
        public static IntPtr MovePointer(IntPtr p, int offset)
        {
            return new IntPtr(p.ToInt64() + offset);
        }
    }

    class ParamDesc
    {
        public ParamDesc(Object parent, IntPtr lpVarValue, PARAMFLAG flag)
        {
            m_lpVarValue = lpVarValue;
            m_paramflag = flag;
            m_parent = parent;
        }

        public PARAMFLAG wParamFlags { get { return m_paramflag; } }
        public IntPtr lpVarValue { get { return m_lpVarValue; } }

        public bool IsIn { get { return (PARAMFLAG.PARAMFLAG_FIN & wParamFlags) != 0; } }
        public bool IsOut { get { return (PARAMFLAG.PARAMFLAG_FOUT & wParamFlags) != 0; } }
        public bool IsLCID { get { return (PARAMFLAG.PARAMFLAG_FLCID & wParamFlags) != 0; } }
        public bool IsRetval { get { return (PARAMFLAG.PARAMFLAG_FRETVAL & wParamFlags) != 0; } }
        public bool IsOpt { get { return (PARAMFLAG.PARAMFLAG_FOPT & wParamFlags) != 0; } }
        public bool HasDefault { get { return (PARAMFLAG.PARAMFLAG_FHASDEFAULT & wParamFlags) != 0; } }
        public bool HasCustData { get { return (PARAMFLAG.PARAMFLAG_FHASCUSTDATA & wParamFlags) != 0; } }

        private IntPtr m_lpVarValue;
        private PARAMFLAG m_paramflag;
        private Object m_parent;                    // hold on to parent to avoid GC hole 
                                                    // because we need the memory pointed by IntPtr to be alive
    }

    class IdlDesc
    {
        public IdlDesc(IDLFLAG flags)
        {
            m_wIDLFlags = flags;
        }
        public IDLFLAG wIDLFlags { get { return m_wIDLFlags; } }

        public bool IsIn { get { return (IDLFLAG.IDLFLAG_FIN & wIDLFlags) != 0; } }
        public bool IsOut { get { return (IDLFLAG.IDLFLAG_FOUT & wIDLFlags) != 0; } }
        public bool IsLCID { get { return (IDLFLAG.IDLFLAG_FLCID & wIDLFlags) != 0; } }
        public bool IsRetval { get { return (IDLFLAG.IDLFLAG_FRETVAL & wIDLFlags) != 0; } }

        private IDLFLAG m_wIDLFlags;
    }

    class TypeDesc
    {
        public TypeDesc(Object parent, IntPtr typeDesc, int vt)
        {
            m_parent = parent;
            m_lptdesc = typeDesc;
            m_vt = vt;
        }

        public TypeDesc lptdesc
        {
            get
            {
                // The reason we need to get the TYPEDESC here is that we don't know whether the typeDesc is valid or not until now
                TYPEDESC typeDesc = (TYPEDESC)Marshal.PtrToStructure(m_lptdesc, typeof(TYPEDESC));
                return new TypeDesc(this, typeDesc.desc.lptdesc, typeDesc.vt);
            }
        }

        public ArrayDesc lpadesc
        {
            get
            {
                return new ArrayDesc(this, m_lptdesc);
            }
        }
        public int hreftype
        {
            get
            {
                return (int)(m_lptdesc.ToInt64() & 0xffffffff);
            }
        }

        public TypeInfo GetUserDefinedTypeInfo(TypeInfo typeinfo)
        {
            return typeinfo.GetRefTypeInfo(hreftype);        
        }

        public int vt { get { return m_vt; } }

        IntPtr m_lptdesc;
        private int m_vt;
        private Object  m_parent;                   // hold on to parent to avoid GC hole 
                                                    // because we need the memory pointed by IntPtr to be alive
    }

    class ArrayDesc
    {
        public ArrayDesc(Object parent, IntPtr pArrayDesc)
        {
            m_parent = parent;
            m_lpValue = pArrayDesc;
        }

        public SAFEARRAYBOUND[] Bounds
        {
            get
            {
                IntPtr pArrayDesc = m_lpValue;

                ushort cDims = (ushort)Marshal.ReadInt16(Utils.MovePointer(pArrayDesc, Marshal.SizeOf(typeof(TYPEDESC))));

                pArrayDesc = Utils.MovePointer(pArrayDesc, Marshal.SizeOf(typeof(ARRAYDESC)));

                List<SAFEARRAYBOUND> bounds = new List<SAFEARRAYBOUND>();
                int safeArrayBoundSize = Marshal.SizeOf(typeof(SAFEARRAYBOUND));
                for( int i = 0; i < cDims; ++i)
                {                        
                    SAFEARRAYBOUND bound = (SAFEARRAYBOUND)Marshal.PtrToStructure(pArrayDesc, typeof(SAFEARRAYBOUND));
                    bounds.Add(bound);

                    pArrayDesc = Utils.MovePointer(pArrayDesc, safeArrayBoundSize);
                }

                return bounds.ToArray();
            }
        }

        public TypeDesc tdescElem
        {
            get
            {
                TYPEDESC typeDesc = (TYPEDESC)Marshal.PtrToStructure(m_lpValue, typeof(TYPEDESC));
                return new TypeDesc(this, typeDesc.desc.lptdesc, typeDesc.vt);
            }
        }

        private IntPtr m_lpValue;
        private Object m_parent;                    // hold on to parent to avoid GC hole 
                                                    // because we need the memory pointed by IntPtr to be alive
    }

    class ElemDesc
    {
        public ElemDesc(FuncDesc funcdesc, int index)
        {
            m_parent = funcdesc;
            IntPtr pParam = funcdesc.m_funcdesc.lprgelemdescParam;
            IntPtr pElemDesc = Utils.MovePointer(pParam, index * Marshal.SizeOf(typeof(ELEMDESC)));
             
            Init( (ELEMDESC) Marshal.PtrToStructure(pElemDesc, typeof(ELEMDESC)));
        }

        public ElemDesc(VarDesc varDesc, ELEMDESC elemdesc)        
        {
            m_parent = varDesc;        
            Init(elemdesc);
        }

        public ElemDesc(FuncDesc funcdesc, ELEMDESC elemdesc)
        {
            m_parent = funcdesc;
            Init(elemdesc);
        }

        private void Init(ELEMDESC elemDesc)
        {
            m_lptdesc = elemDesc.tdesc.desc.lptdesc;
            m_vt = elemDesc.tdesc.vt;
            m_lpVarValue = elemDesc.desc.paramdesc.lpVarValue;
            m_flags = (int)elemDesc.desc.paramdesc.wParamFlags;
        }

        public TypeDesc tdesc { get { return new TypeDesc(this, m_lptdesc, m_vt); } }
        public ParamDesc paramdesc { get { return new ParamDesc(this, m_lpVarValue, (PARAMFLAG)m_flags); } }
        public IdlDesc idldesc { get { return new IdlDesc((IDLFLAG)m_flags); } }

        IntPtr m_lptdesc;
        int m_vt;
        IntPtr m_lpVarValue;
        int m_flags;
        Object m_parent;                            // hold on to parent to avoid GC hole 
                                                    // because we need the memory pointed by IntPtr to be alive
    }

    /// <summary>
    /// A wrapper for FUNCDESC
    /// It used to implement IDisposable because we hope the resource/memory could be released ASAP
    /// However we do need the TypeDesc/ParamDesc/... types that are holding a IntPtr to function correctly,
    /// so change this to rely on GC instead, assuming that TypeLib API doesn't attach any value resources
    /// to FUNCDESC/VARDESC/TYPEATTR/TYPELIBATTR
    /// Note: We still keep IDispoable for now in order to minimize source code change
    /// </summary>
    class FuncDesc : IDisposable
    {
        public FuncDesc(int index, ITypeInfo typeinfo)
        {
            m_typeinfo = typeinfo;
            typeinfo.GetFuncDesc(index, out m_ipFuncDesc);
            m_funcdesc = (FUNCDESC)Marshal.PtrToStructure(m_ipFuncDesc, typeof(FUNCDESC));
        }
        ~FuncDesc()
        {
            if (m_ipFuncDesc != IntPtr.Zero)
                m_typeinfo.ReleaseFuncDesc(m_ipFuncDesc);
            m_typeinfo = null;
        }

        #region IDisposable Members

        public void Dispose()
        {
            // Do nothing. 
            // We keep this to minimize source code change for now
        }

        #endregion

        public int memid { get { return m_funcdesc.memid; } }
        public FUNCKIND funckind { get { return m_funcdesc.funckind; } }
        public INVOKEKIND invkind { get { return m_funcdesc.invkind; } }
        public CALLCONV callconv { get { return m_funcdesc.callconv; } }
        public int cParams { get { return m_funcdesc.cParams; } }
        public int cParamsOpt { get { return m_funcdesc.cParamsOpt; } }
        public int oVft { get { return m_funcdesc.oVft; } }
        public int cScodes { get { return m_funcdesc.cScodes; } }
        public ElemDesc elemdescFunc { get { return new ElemDesc(this, m_funcdesc.elemdescFunc); } }
        public int wFuncFlags { get { return m_funcdesc.wFuncFlags; } }

        public ElemDesc GetElemDesc(int index)
        {
            return new ElemDesc(this, index);
        }

        // Expand out FUNCKIND
        public bool IsVirtual { get { return funckind == FUNCKIND.FUNC_VIRTUAL; } }
        public bool IsPureVirtual { get { return funckind == FUNCKIND.FUNC_PUREVIRTUAL; } }
        public bool IsNonVirtual { get { return funckind == FUNCKIND.FUNC_NONVIRTUAL; } }
        public bool IsStatic { get { return funckind == FUNCKIND.FUNC_STATIC; } }
        public bool IsDispatch { get { return funckind == FUNCKIND.FUNC_DISPATCH; } }

        // Expand out INVOKEKIND
        public bool IsFunc { get { return invkind == INVOKEKIND.INVOKE_FUNC; } }
        public bool IsPropertyGet { get { return invkind == INVOKEKIND.INVOKE_PROPERTYGET; } }
        public bool IsPropertyPut { get { return invkind == INVOKEKIND.INVOKE_PROPERTYPUT; } }
        public bool IsPropertyPutRef { get { return invkind == INVOKEKIND.INVOKE_PROPERTYPUTREF; } }

        // Expand out CALLCONV
        public bool IsCdecl { get { return callconv == CALLCONV.CC_CDECL; } }
        public bool IsPascal { get { return callconv == CALLCONV.CC_PASCAL; } }
        public bool IsStdCall { get { return callconv == CALLCONV.CC_STDCALL; } }
        public bool IsSysCall { get { return callconv == CALLCONV.CC_SYSCALL; } }

        // Expand out FUNCFLAGS
        public bool IsRestricted { get { return ((int)wFuncFlags & (int)FUNCFLAGS.FUNCFLAG_FRESTRICTED) != 0; } }
        public bool IsSource { get { return ((int)wFuncFlags & (int)FUNCFLAGS.FUNCFLAG_FSOURCE) != 0; } }
        public bool IsBindable { get { return ((int)wFuncFlags & (int)FUNCFLAGS.FUNCFLAG_FBINDABLE) != 0; } }
        public bool IsRequestEdit { get { return ((int)wFuncFlags & (int)FUNCFLAGS.FUNCFLAG_FREQUESTEDIT) != 0; } }
        public bool IsDisplayBind { get { return ((int)wFuncFlags & (int)FUNCFLAGS.FUNCFLAG_FDISPLAYBIND) != 0; } }
        public bool IsDefaultBind { get { return ((int)wFuncFlags & (int)FUNCFLAGS.FUNCFLAG_FDEFAULTBIND) != 0; } }
        public bool IsHidden { get { return ((int)wFuncFlags & (int)FUNCFLAGS.FUNCFLAG_FHIDDEN) != 0; } }
        public bool IsUsesGetLastError { get { return ((int)wFuncFlags & (int)FUNCFLAGS.FUNCFLAG_FUSESGETLASTERROR) != 0; } }
        public bool IsDefaultCollElem { get { return ((int)wFuncFlags & (int)FUNCFLAGS.FUNCFLAG_FDEFAULTCOLLELEM) != 0; } }
        public bool IsUIDefault { get { return ((int)wFuncFlags & (int)FUNCFLAGS.FUNCFLAG_FUIDEFAULT) != 0; } }
        public bool IsNonBrowsable { get { return ((int)wFuncFlags & (int)FUNCFLAGS.FUNCFLAG_FNONBROWSABLE) != 0; } }
        public bool IsReplaceable { get { return ((int)wFuncFlags & (int)FUNCFLAGS.FUNCFLAG_FREPLACEABLE) != 0; } }
        public bool IsImmediateBind { get { return ((int)wFuncFlags & (int)FUNCFLAGS.FUNCFLAG_FIMMEDIATEBIND) != 0; } }

        public IntPtr m_ipFuncDesc;
        public FUNCDESC m_funcdesc;
        private ITypeInfo m_typeinfo;
    }
}
