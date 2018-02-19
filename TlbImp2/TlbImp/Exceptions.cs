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
using System.Reflection;
using TypeLibTypes.Interop;

namespace tlbimp2
{
    /// <summary>
    /// Used to signal that a resolve ref operation has failed
    /// </summary>
    class TlbImpResolveRefFailWrapperException : ApplicationException
    {
        public TlbImpResolveRefFailWrapperException(Exception ex)
            : base(String.Empty, ex)
        {
        }
    }

    /// <summary>
    /// Used to signal that something went wrong in the type conversion process 
    /// </summary>
    class TlbImpInvalidTypeConversionException : ApplicationException
    {
        private TypeInfo m_type;

        public TlbImpInvalidTypeConversionException(TypeInfo type)
        {
            m_type = type;
        }

        public TypeInfo Type
        {
            get
            {
                return m_type;
            }
        }
    }

    /// <summary>
    /// The Exception class contain the error ID and whether we need to print out the logo information
    /// </summary>
    class TlbImpGeneralException : ApplicationException
    {
        private ErrorCode m_errorID;
        private bool m_needToPrintLogo;

        public TlbImpGeneralException(string str, ErrorCode errorID)
            : this(str, errorID, false)
        {
        }

        public TlbImpGeneralException(string str, ErrorCode errorID, bool needToPrintLogo)
            : base(str)
        {
            m_errorID = errorID;
            m_needToPrintLogo = needToPrintLogo;
        }

        public ErrorCode ErrorID
        {
            get
            {
                return m_errorID;
            }
        }

        public bool NeedToPrintLogo 
        {
            get
            {
                return m_needToPrintLogo;
            }
        }
    }

    /// <summary>
    /// The resource cannot be found
    /// </summary>
    class TlbImpResourceNotFoundException : ApplicationException
    {
        public TlbImpResourceNotFoundException(string msg)
            : base(msg)
        {
        }
    }
}
