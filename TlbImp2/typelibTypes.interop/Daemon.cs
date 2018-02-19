using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TypeLibTypes.Interop
{
    /// <summary>
    /// Because the type library API’s native resource (such as FuncDesc, TypeAttr, …)
    /// should be constructed and destructed in the same apartment, otherwise, problem will occur.
    /// Meanwhile, we cannot make the assumption that the application using this Dll component will
    /// run as a Single Thread Application. So, we introduce the IDaemon interface and all native
    /// resource construction/destruction will go through in this interface.
    /// In this way, IDaemon offered us an opportunity to keep the construction/destruction in one
    /// apartment, when the Dll is used in a MultiThread Application.
    /// </summary>
    public interface IDaemon
    {
        IntPtr GetTypeLibAttr(ITypeLib typelib);

        void ReleaseTypeLibAttr(ITypeLib m_typelib, IntPtr m_ipAttr);

        IntPtr GetTypeAttr(ITypeInfo typeInfo);

        void ReleaseTypeAttr(ITypeInfo typeInfo, IntPtr typeAttr);

        IntPtr GetFuncDesc(ITypeInfo typeinfo, int index);

        void ReleaseFuncDesc(ITypeInfo typeInfo, IntPtr funcDesc);

        IntPtr GetVarDesc(ITypeInfo typeinfo, int index);

        void ReleaseVarDesc(ITypeInfo m_typeinfo, IntPtr m_ipVarDesc);
    }
}
