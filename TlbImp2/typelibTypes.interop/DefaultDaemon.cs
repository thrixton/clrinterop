using System;
using System.Collections.Generic;
using System.Text;

namespace TypeLibTypes.Interop
{
    /// <summary>
    /// A DefaultDaemon calls the type library API directly. While, Multi-Thread application
    /// using this component can implement the IDaemon interface, substitute the DefaultDaemon
    /// with its own one by calling TypeLibResourceManager.InitTypeLibResourceManager, and keep the
    /// call of the interface is in the same apartment to avoid the problem of native resource’s
    /// construction/destruction.
    /// </summary>
    class DefaultDaemon : IDaemon
    {
        #region IDaemon Members

        public IntPtr GetTypeLibAttr(ITypeLib typeLib)
        {
            IntPtr ret;
            typeLib.GetLibAttr(out ret);
            return ret;
        }

        public void ReleaseTypeLibAttr(ITypeLib typeLib, IntPtr typeLibAttr)
        {
            typeLib.ReleaseTLibAttr(typeLibAttr);
        }

        public IntPtr GetTypeAttr(ITypeInfo typeInfo)
        {
            IntPtr ret;
            typeInfo.GetTypeAttr(out ret);
            return ret;
        }

        public void ReleaseTypeAttr(ITypeInfo typeInfo, IntPtr typeAttr)
        {
            typeInfo.ReleaseTypeAttr(typeAttr);
        }

        public IntPtr GetFuncDesc(ITypeInfo typeInfo, int index)
        {
            IntPtr ret;
            typeInfo.GetFuncDesc(index, out ret);
            return ret;
        }

        public void ReleaseFuncDesc(ITypeInfo typeInfo, IntPtr funcDesc)
        {
            typeInfo.ReleaseFuncDesc(funcDesc);
        }

        public IntPtr GetVarDesc(ITypeInfo typeInfo, int index)
        {
            IntPtr ret;
            typeInfo.GetVarDesc(index, out ret);
            return ret;
        }

        public void ReleaseVarDesc(ITypeInfo typeInfo, IntPtr varDesc)
        {
            typeInfo.ReleaseVarDesc(varDesc);
        }

        #endregion
    }
}
