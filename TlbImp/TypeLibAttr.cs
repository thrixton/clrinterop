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
    /// Wrapper for TYPELIBATTR
    /// It used to implement IDisposable because we hope the resource/memory could be released ASAP
    /// However we do need the TypeDesc/ParamDesc/... types that are holding a IntPtr to function correctly,
    /// so change this to rely on GC instead, assuming that TypeLib API doesn't attach any value resources
    /// to FUNCDESC/VARDESC/TYPEATTR/TYPELIBATTR
    /// Note: We still keep IDispoable for now in order to minimize source code change
    /// </summary>
    class TypeLibAttr : IDisposable
    {
        public TypeLibAttr(ITypeLib typelib)
        {
            m_typelib = typelib;
            typelib.GetLibAttr(out m_ipAttr);
            m_attr = (TYPELIBATTR)Marshal.PtrToStructure(m_ipAttr, typeof(TYPELIBATTR));
        }
        ~TypeLibAttr()
        {
            Release();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Release();
            GC.SuppressFinalize(this);
        }

        private void Release()
        {
            if (m_ipAttr != IntPtr.Zero)
                m_typelib.ReleaseTLibAttr(m_ipAttr);
            m_typelib = null;
        }

        #endregion

        public Guid guid { get { return m_attr.guid; } }
        public int lcid { get { return m_attr.lcid; } }
        public SYSKIND syskind { get { return m_attr.syskind; } }
        public int wMajorVerNum { get { return m_attr.wMajorVerNum; } }
        public int wMinorVerNum { get { return m_attr.wMinorVerNum; } }
        public LIBFLAGS wLibFlags { get { return m_attr.wLibFlags; } }

        // Expand out SYSKIND
        public bool IsWin16 { get { return syskind == SYSKIND.SYS_WIN16; } }
        public bool IsWin32 { get { return syskind == SYSKIND.SYS_WIN32; } }
        public bool IsMac { get { return syskind == SYSKIND.SYS_MAC; } }
        public bool IsWin64 { get { return syskind == SYSKIND.SYS_WIN64; } }

        // Expand out LIBFLAGS
        public bool IsRestricted { get { return (wLibFlags & LIBFLAGS.LIBFLAG_FRESTRICTED) != 0; } }
        public bool IsControl { get { return (wLibFlags & LIBFLAGS.LIBFLAG_FCONTROL) != 0; } }
        public bool IsHidden { get { return (wLibFlags & LIBFLAGS.LIBFLAG_FHIDDEN) != 0; } }
        public bool IsHasDiskImage { get { return (wLibFlags & LIBFLAGS.LIBFLAG_FHASDISKIMAGE) != 0; } }

        private ITypeLib m_typelib;
        private IntPtr m_ipAttr;
        private TYPELIBATTR m_attr;
    }
}
