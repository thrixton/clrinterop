using System;
using System.Collections.Generic;
using System.Text;
using TypeLibTypes.Interop;

namespace TypeLibraryTreeView
{
    /// <summary>
    /// Because TypeLibraryTreeView.dll may be used in a Multi-Thread Application, it is very important
    /// to make sure that the type library API’s native resource construction and destruction is in
    /// the same apartment. So, we introduce the FormDaemon to implement the IDaemon interface, and
    /// use a DaemonForm to make sure all IDaemon’s methods are called in the same UI thread.
    /// 
    /// FormDaemon holds a reference to a simple invisible Form (DaemonForm) instance. All GetXXX and
    /// ReleaseXXX methods in the IDaemon interface is implemented by calling form.Invoke method. In this
    /// way, all construction/destruction work is performed in the DaemonForm's UI thread, in other words,
    /// in the same apartment.
    /// </summary>
    public class FormDaemon : IDaemon
    {
        delegate IntPtr GetTypeLibAttrDelegate(ITypeLib typeLib);

        private static GetTypeLibAttrDelegate s_getTypeLibAttrDelegate =
                new GetTypeLibAttrDelegate(GetTypeLibAttr_Proxy);

        delegate void ReleaseTypeLibAttrDelegate(ITypeLib typeLib, IntPtr libAttr);

        private static ReleaseTypeLibAttrDelegate s_releaseTypeLibAttrDelegate =
                new ReleaseTypeLibAttrDelegate(ReleaseTypeLibAttr_Proxy);

        delegate IntPtr GetTypeAttrDelegate(ITypeInfo typeInfo);

        private static GetTypeAttrDelegate s_getTypeAttrDelegate =
                new GetTypeAttrDelegate(GetTypeAttr_Proxy);

        delegate void ReleaseTypeAttrDelegate(ITypeInfo typeInfo, IntPtr typeAttr);

        private static ReleaseTypeAttrDelegate s_releaseTypeAttrDelegate =
                new ReleaseTypeAttrDelegate(ReleaseTypeAttr_Proxy);

        delegate IntPtr GetFuncDescDelegate(ITypeInfo typeInfo, int index);

        private static GetFuncDescDelegate s_getFuncDescDelegate =
                new GetFuncDescDelegate(GetFuncDesc_Proxy);

        delegate void ReleaseFuncDescDelegate(ITypeInfo typeInfo, IntPtr funcDesc);
        
        private static ReleaseFuncDescDelegate s_releaseFuncDescDelegate =
                new ReleaseFuncDescDelegate(ReleaseFuncDesc_Proxy);

        delegate IntPtr GetVarDescDelegate(ITypeInfo typeInfo, int index);

        private static GetVarDescDelegate s_getVarDescDelegate =
                new GetVarDescDelegate(GetVarDesc_Proxy);

        delegate void ReleaseVarDescDelegate(ITypeInfo typeInfo, IntPtr varDesc);

        private static ReleaseVarDescDelegate s_releaseVarDescDelegate =
               new ReleaseVarDescDelegate(ReleaseVarDesc_Proxy);

        DaemonForm m_form;

        public DaemonForm DaemonForm
        {
            get
            {
                return m_form;
            }
            set
            {
                m_form = value;
            }
        }

        public FormDaemon(DaemonForm form)
        {
            m_form = form;
        }

        #region IDaemon Members

        public IntPtr GetTypeLibAttr(ITypeLib typeLib)
        {
            object invokeReturn = m_form.Invoke(s_getTypeLibAttrDelegate, new object[] { typeLib });
            return (IntPtr)invokeReturn;
        }

        public void ReleaseTypeLibAttr(ITypeLib typeLib, IntPtr libAttr)
        {
            if (m_form != null)
                m_form.Invoke(s_releaseTypeLibAttrDelegate, new object[] { typeLib, libAttr });
        }

        public IntPtr GetTypeAttr(ITypeInfo typeInfo)
        {
            object invokeReturn = m_form.Invoke(s_getTypeAttrDelegate, new object[] { typeInfo });
            return (IntPtr)invokeReturn;
        }

        public void ReleaseTypeAttr(ITypeInfo typeInfo, IntPtr typeAttr)
        {
            if (m_form != null)
                m_form.Invoke(s_releaseTypeAttrDelegate, new object[] { typeInfo, typeAttr });
        }

        public IntPtr GetFuncDesc(ITypeInfo typeInfo, int index)
        {
            object invokeReturn = m_form.Invoke(s_getFuncDescDelegate,
                new object[] { typeInfo, index });
            return (IntPtr)invokeReturn;
        }

        public void ReleaseFuncDesc(ITypeInfo typeInfo, IntPtr funcDesc)
        {
            if (m_form != null)
                m_form.Invoke(s_releaseFuncDescDelegate, new object[] { typeInfo, funcDesc });
        }

        public IntPtr GetVarDesc(ITypeInfo typeInfo, int index)
        {
            object invokeReturn = m_form.Invoke(s_getVarDescDelegate,
                new object[] { typeInfo, index });
            return (IntPtr)invokeReturn;
        }

        public void ReleaseVarDesc(ITypeInfo typeInfo, IntPtr varDesc)
        {
            if (m_form != null)
                m_form.Invoke(s_releaseVarDescDelegate, new object[] { typeInfo, varDesc });
        }

        #endregion


        private static IntPtr GetTypeLibAttr_Proxy(ITypeLib typeLib)
        {
            IntPtr libAttr;
            typeLib.GetLibAttr(out libAttr);
            return libAttr;
        }

        private static void ReleaseTypeLibAttr_Proxy(ITypeLib typeLib, IntPtr libAttr)
        {
            typeLib.ReleaseTLibAttr(libAttr);
        }

        private static IntPtr GetTypeAttr_Proxy(ITypeInfo typeInfo)
        {
            IntPtr typeAttr;
            typeInfo.GetTypeAttr(out typeAttr);
            return typeAttr;
        }

        private static void ReleaseTypeAttr_Proxy(ITypeInfo typeInfo, IntPtr typeAttr)
        {
            typeInfo.ReleaseTypeAttr(typeAttr);
        }

        private static IntPtr GetFuncDesc_Proxy(ITypeInfo typeInfo, int index)
        {
            IntPtr funcDesc;
            typeInfo.GetFuncDesc(index, out funcDesc);
            return funcDesc;
        }

        private static void ReleaseFuncDesc_Proxy(ITypeInfo typeInfo, IntPtr funcDesc)
        {
            typeInfo.ReleaseFuncDesc(funcDesc);
        }

        private static IntPtr GetVarDesc_Proxy(ITypeInfo typeInfo, int index)
        {
            IntPtr varDesc;
            typeInfo.GetVarDesc(index, out varDesc);
            return varDesc;
        }

        private static void ReleaseVarDesc_Proxy(ITypeInfo typeInfo, IntPtr varDesc)
        {
            typeInfo.ReleaseVarDesc(varDesc);
        }

    }
}