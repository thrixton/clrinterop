///////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation. All rights reserved.
///////////////////////////////////////////////////////////////////////////////
//
// Type Library Importer utility
//
// This program imports all the types in the type library into a interop assembly
//
///////////////////////////////////////////////////////////////////////////////

namespace tlbimp2.Event
{

    using System;
    using System.Reflection;
    using System.Collections;

    internal class EventItfInfo
    {
        public EventItfInfo(String strEventItfName,
                            String strSrcItfName,
                            String strEventProviderName,
                            Type eventItfType,
                            Type srcItfType)
        {
            m_strEventItfName = strEventItfName;
            m_strSrcItfName = strSrcItfName;
            m_strEventProviderName = strEventProviderName;
            m_eventItfType = eventItfType;
            m_srcItfType = srcItfType;
        }

        public Type GetEventItfType()
        {
            return m_eventItfType;
        }

        public Type GetSrcItfType()
        {
            return m_srcItfType;
        }

        public String GetEventProviderName()
        {
            return m_strEventProviderName;
        }

        private String m_strEventItfName;
        private String m_strSrcItfName;
        private String m_strEventProviderName;
        private Type m_eventItfType;
        private Type m_srcItfType;
    }
}
