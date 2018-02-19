using System;
using System.Collections.Generic;
using System.Text;

namespace TypeLibTypes.Interop
{
    /// <summary>
    /// TypeLibResourceManager manages the IDaemon the application used.
    /// </summary>
    public class TypeLibResourceManager
    {
        private static IDaemon s_daemon;

        private static object m_lock = new object();

        public static bool InitTypeLibResourceManager(IDaemon daemon)
        {
            bool success = false;
            lock (m_lock)
            {
                if (s_daemon == null)
                {
                    s_daemon = daemon;
                    success = true;
                }
            }
            return success;
        }


        /// <summary>
        /// When daemon is not initialized, the DefaultDaemon is used.
        /// </summary>
        public static IDaemon GetDaemon()
        {
            lock (m_lock)
            {
                if (s_daemon == null)
                    s_daemon = new DefaultDaemon();
            }
            return s_daemon;
        }

    }
}
