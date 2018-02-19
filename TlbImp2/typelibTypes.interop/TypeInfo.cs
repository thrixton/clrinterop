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

namespace TypeLibTypes.Interop
{

    public class TypeInfo
    {
#if DEBUG
        string _Name { get { return GetDocumentation(); } }
        TypeInfo _Parent { get { return GetRefType(0); } }
#endif 

        public TypeInfo(ITypeInfo typeinfo)
        {
            m_typeInfo = typeinfo;
            m_typeInfo2 = m_typeInfo as ITypeInfo2;
        }

        public TypeAttr GetTypeAttr()
        {
            return new TypeAttr(m_typeInfo);
        }
        public FuncDesc GetFuncDesc(int index)
        {
            return new FuncDesc(index, m_typeInfo);
        }
        public VarDesc GetVarDesc(int index)
        {
            return new VarDesc(m_typeInfo, index);
        }
        public string[] GetNames(int memid, int len)
        {
            string[] strarray = new string[len];
            IntPtr[] ptrArray = new IntPtr[len];
            GCHandle gch = GCHandle.Alloc(ptrArray, GCHandleType.Pinned);
            m_typeInfo.GetNames(memid, gch.AddrOfPinnedObject(), strarray.Length, out len);
            gch.Free();
            for (int n = 0; n < len; ++n)
            {
                if (ptrArray[n] == IntPtr.Zero)
                    strarray[n] = String.Empty;
                else
                    // This API doesn't support null BSTR, which it should
                    strarray[n] = Marshal.PtrToStringBSTR(ptrArray[n]);
            }

            return strarray;
        }

        public TypeInfo GetRefTypeNoComThrow(int index)
        {
            int href;
            if (m_typeInfo.GetRefTypeOfImplType(index, out href) != 0)
                return null;
            ITypeInfo typeinfo;
            m_typeInfo.GetRefTypeInfo(href, out typeinfo);
            return new TypeInfo(typeinfo);
        }
        
        public TypeInfo GetRefType(int index)
        {
            int href;
            int hr;
            hr = m_typeInfo.GetRefTypeOfImplType(index, out href);
            if (hr != 0)
                throw Marshal.GetExceptionForHR(hr);
            ITypeInfo typeinfo;
            m_typeInfo.GetRefTypeInfo(href, out typeinfo);
            return new TypeInfo(typeinfo);
        }

        public TypeInfo GetRefType()
        {
            return GetRefType(-1);
        }

        public TypeInfo GetRefTypeNoComThrow()
        {
            return GetRefTypeNoComThrow(-1);
        }

        public TypeInfo GetRefTypeInfo(int href)
        {
            ITypeInfo typeinfo;
            m_typeInfo.GetRefTypeInfo(href, out typeinfo);
            return new TypeInfo(typeinfo);
        }
        public IMPLTYPEFLAGS GetImplTypeFlags(int index)
        {
            IMPLTYPEFLAGS flags;
            m_typeInfo.GetImplTypeFlags(index, out flags);
            return flags;
        }
        public void GetDocumentation(int index, out String strName)
        {
            // The reason why we want to pass NULL (IntPtr.Zero) is that ITypeInfo2::GetDocumentation will
            // try to load up the corresponding DLL and look for GetDocumentation entry point and ask it for documentation, which
            // will probably fail. As a result, GetDocumentation will fail.
            // To avoid this issue, always pass NULL for the last 3 arguments which we don't need
            m_typeInfo.GetDocumentation(index, out strName, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }
        public String GetDocumentation(int index)
        {
            String strName;
            GetDocumentation(index, out strName);
            return strName;
        }
        public String GetDocumentation()
        {
            return GetDocumentation(TYPEATTR.MEMBER_ID_NIL);
        }
        public string GetMops(int memid)
        {
            String mops;
            m_typeInfo.GetMops(memid, out mops);
            return mops;
        }
        public TypeLib GetContainingTypeLib()
        {
            int index;
            ITypeLib typelib;
            m_typeInfo.GetContainingTypeLib(out typelib, out index);
            return new TypeLib(typelib);
        }
        public Object GetCustData(Guid guid)
        {
            if (m_typeInfo2 != null)
            {
                Object obj;
                if (m_typeInfo2.GetCustData(ref guid, out obj) != 0)
                    obj = null;
                return obj;
            }
            else
                return null;
        }

        public Object GetFuncCustData(int index, Guid guid)
        {
            if (m_typeInfo2 != null)
            {

                Object obj;
                if (m_typeInfo2.GetFuncCustData(index, ref guid, out obj) != 0)
                    obj = null;
                return obj;
            }
            else
                return null;
        }

        public Object GetParamCustData(int indexFunc, int indexParam, Guid guid)
        {
            if (m_typeInfo2 != null)
            {

                Object obj;
                if (m_typeInfo2.GetParamCustData(indexFunc, indexParam, ref guid, out obj) != 0)
                    obj = null;
                return obj;
            }
            else
                return null;
        }

        public Object GetVarCustData(int index, Guid guid)
        {
            if (m_typeInfo2 != null)
            {
                Object obj;
                if (m_typeInfo2.GetVarCustData(index, ref guid, out obj) != 0)
                    obj = null;

                return obj;
            }
            else
                return null;
        }

        public Object GetImplTypeCustData(int index, ref Guid guid)
        {
            if (m_typeInfo2 != null)
            {
                Object obj;
                if (m_typeInfo2.GetImplTypeCustData(index, ref guid, out obj) != 0)
                    obj = null;
                return obj;
            }
            else
                return null;
        }

        private ITypeInfo m_typeInfo;
        private ITypeInfo2 m_typeInfo2;
    }

}
