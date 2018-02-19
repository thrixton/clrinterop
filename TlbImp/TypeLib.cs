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
    class TypeLib
    {
        [DllImport("oleaut32.dll")]
        public static extern int LoadTypeLib(
            [MarshalAs(UnmanagedType.BStr)] string strFile,
            out ITypeLib ppTypeLib);

        [DllImport("oleaut32.dll")]
        public static extern int QueryPathOfRegTypeLib([In]ref Guid guid, ushort wVerMajor, ushort wVerMinor,
            int lcid, [MarshalAs(UnmanagedType.BStr)] out string pathName);

        public static TypeLib Load(string strFile)
        {
            ITypeLib typelib;
            LoadTypeLib(strFile, out typelib);
            return new TypeLib(typelib);
        }

        public TypeLib(ITypeLib typelib)
        {
            m_typelib = typelib;
            m_typeLib2 = typelib as ITypeLib2;
        }
        public int GetTypeInfoCount()
        {
            return m_typelib.GetTypeInfoCount();
        }
        public TypeInfo GetTypeInfo(int index)
        {
            ITypeInfo typeinfo;
            m_typelib.GetTypeInfo(index, out typeinfo);
            return new TypeInfo(typeinfo);
        }
        public TYPEKIND GetTypeInfoType(int index)
        {
            TYPEKIND type;
            m_typelib.GetTypeInfoType(index, out type);
            return type;
        }
        public TypeInfo GetTypeInfoOfGuid(ref Guid guid)
        {
            ITypeInfo typeinfo;
            m_typelib.GetTypeInfoOfGuid(ref guid, out typeinfo);
            return new TypeInfo(typeinfo);
        }
        public TypeLibAttr GetLibAttr()
        {
            return new TypeLibAttr(m_typelib);
        }
        public void GetDocumentation(int index, out String strName, out String strDocString, out int dwHelpContext, out string strHelpFile)
        {
            m_typelib.GetDocumentation(index, out strName, out strDocString, out dwHelpContext, out strHelpFile);
        }
        public String GetDocumentation(int index)
        {
            String strName;
            String strDocString;
            int dwHelpContext;
            String strHelpFile;
            GetDocumentation(index, out strName, out strDocString, out dwHelpContext, out strHelpFile);
            return strName;
        }
        public String GetDocumentation()
        {
            return GetDocumentation(TYPEATTR.MEMBER_ID_NIL);
        }

        public object GetCustData(Guid guid)
        {
            if (m_typeLib2 != null)
            {
                object val;
                m_typeLib2.GetCustData(ref guid, out val);
                return val;
            }

            return null;
        }

        public ITypeLib GetTypeLib() { return m_typelib; }

        private ITypeLib m_typelib;
        private ITypeLib2 m_typeLib2;
    }
}
