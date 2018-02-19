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

namespace tlbimp2
{
    /// <summary>
    /// A wrapper for VARDESC
    /// It used to implement IDisposable because we hope the resource/memory could be released ASAP
    /// However we do need the TypeDesc/ParamDesc/... types that are holding a IntPtr to function correctly,
    /// so change this to rely on GC instead, assuming that TypeLib API doesn't attach any value resources
    /// to FUNCDESC/VARDESC/TYPEATTR/TYPELIBATTR
    /// Note: We still keep IDispoable for now in order to minimize source code change
    /// </summary>
    class VarDesc : IDisposable
    {
        public VarDesc(ITypeInfo typeinfo, int index)
        {
            m_typeinfo = typeinfo;
            typeinfo.GetVarDesc(index, out m_ipVarDesc);
            m_vardesc = (VARDESC)Marshal.PtrToStructure(m_ipVarDesc, typeof(VARDESC));
        }
        ~VarDesc()
        {
            if (m_ipVarDesc != IntPtr.Zero)
                m_typeinfo.ReleaseVarDesc(m_ipVarDesc);
            m_typeinfo = null;
        }

        #region IDisposable Members

        public void Dispose()
        {
            // Do nothing. 
            // We keep this to minimize source code change for now
        }

        #endregion

        public int memid { get { return m_vardesc.memid; } }
        public int oInst { get { return m_vardesc.desc.oInst; } }
        public IntPtr lpvarValue { get { return m_vardesc.desc.lpvarValue; } }
        public ElemDesc elemdescVar { get { return new ElemDesc(this, m_vardesc.elemdescVar); } }
        public int wVarFlags { get { return m_vardesc.wVarFlags; } }
        public VARKIND varkind { get { return m_vardesc.varkind; } }

        // Expand out VARKIND
        public bool IsPerInstance { get { return varkind == VARKIND.VAR_PERINSTANCE; } }
        public bool IsStatic { get { return varkind == VARKIND.VAR_STATIC; } }
        public bool IsConst { get { return varkind == VARKIND.VAR_CONST; } }
        public bool IsDispath { get { return varkind == VARKIND.VAR_DISPATCH; } }

        // Expand out wVarFlags
        public bool IsReadOnly { get { return ((int)wVarFlags & (int)VARFLAGS.VARFLAG_FREADONLY) != 0; } }
        public bool IsSource { get { return ((int)wVarFlags & (int)VARFLAGS.VARFLAG_FSOURCE) != 0; } }
        public bool IsBindable { get { return ((int)wVarFlags & (int)VARFLAGS.VARFLAG_FBINDABLE) != 0; } }
        public bool IsRequestEdit { get { return ((int)wVarFlags & (int)VARFLAGS.VARFLAG_FREQUESTEDIT) != 0; } }
        public bool IsDisplayBind { get { return ((int)wVarFlags & (int)VARFLAGS.VARFLAG_FDISPLAYBIND) != 0; } }
        public bool IsDefaultBind { get { return ((int)wVarFlags & (int)VARFLAGS.VARFLAG_FDEFAULTBIND) != 0; } }
        public bool IsHidden { get { return ((int)wVarFlags & (int)VARFLAGS.VARFLAG_FHIDDEN) != 0; } }
        public bool IsRestricted { get { return ((int)wVarFlags & (int)VARFLAGS.VARFLAG_FRESTRICTED) != 0; } }
        public bool IsDefaultCollElem { get { return ((int)wVarFlags & (int)VARFLAGS.VARFLAG_FDEFAULTCOLLELEM) != 0; } }
        public bool IsUIDefault { get { return ((int)wVarFlags & (int)VARFLAGS.VARFLAG_FUIDEFAULT) != 0; } }
        public bool IsNonBrowsable { get { return ((int)wVarFlags & (int)VARFLAGS.VARFLAG_FNONBROWSABLE) != 0; } }
        public bool IsReplaceable { get { return ((int)wVarFlags & (int)VARFLAGS.VARFLAG_FREPLACEABLE) != 0; } }
        public bool IsImmediateBind { get { return ((int)wVarFlags & (int)VARFLAGS.VARFLAG_FIMMEDIATEBIND) != 0; } }

        private ITypeInfo m_typeinfo;
        private IntPtr m_ipVarDesc;
        private VARDESC m_vardesc;
    }
}
