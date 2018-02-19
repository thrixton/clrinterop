///////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation. All rights reserved.
///////////////////////////////////////////////////////////////////////////////
//
// Type Library Importer utility
//
// This program imports all the types in the type library into a interop assembly
//
///////////////////////////////////////////////////////////////////////////////

// Comments on Error handling:
// Our build tools all expect error messages to be in a particular
// format.  The grammar is slightly complex with a number of optional
// parameters.  But these methods below should conform with this spec.
// Talk to the xmake team (msbuild?) for more details on the exact
// spec build tools like ResGen should follow.  -- BrianGru

// To sum up their spec:
// Error and warning messages consist of strings like the following:
// Origin : [subcategory] category code : text
// like this:
// cl : Command line warning D4024 : Unrecognized source file type
// Origin can be the tool name, file name, or file(line,pos).  Origin
// must not be localized if it is a tool or file name.
// Subcategory is optional and should be localized.
// Category is "warning" or "error" & must not be localized.
// Code must not contain spaces & be non-localized.
// Text is the localized error message


using System;
using tlbimp2;

namespace FormattedOutput
{
    internal class Output
    {
        // Use this for general resgen errors with no specific file info
        public static void WriteError(String message, ErrorCode errorCode)
        {
            String errorFormat = "TlbImp : error TI{1:0000} : {0}";
            Console.Error.WriteLine(errorFormat, message, (int)errorCode);
        }

        // Use this for a general error w.r.t. a file, like a missing file.
        public static void WriteError(String message, String fileName)
        {
            WriteError(message, fileName, 0);
        }

        // Use this for a general error w.r.t. a file, like a missing file.
        public static void WriteError(String message, String fileName, int errorNumber)
        {
            String errorFormat = "{0} : error TI{1:0000} : {2}";
            Console.Error.WriteLine(errorFormat, fileName, errorNumber, message);
        }

        // For specific errors about the contents of a file and you know where
        // the error occurred.
        public static void WriteError(String message, String fileName, int line, int column)
        {
            WriteError(message, fileName, line, column, 0);
        }

        // For specific errors about the contents of a file and you know where
        // the error occurred.
        public static void WriteError(String message, String fileName, int line, int column, int errorNumber)
        {
            String errorFormat = "{0}({1},{2}): error TI{3:0000} : {4}";
            Console.Error.WriteLine(errorFormat, fileName, line, column, errorNumber, message);
        }

        public static void WriteError(String strPrefix, Exception e)
        {
            WriteError(strPrefix, e, 0);
        }

        public static void WriteError(String strPrefix, Exception e, ErrorCode errorCode)
        {
            String strErrorMsg = "";
            if (strPrefix != null)
                strErrorMsg = strPrefix;

            strErrorMsg += e.GetType().ToString();
            if (e.Message != null)
            {
                strErrorMsg += " - " + e.Message;
            }

            if (e.InnerException != null)
            {
                strErrorMsg += " : " + e.InnerException.GetType().ToString();
                if (e.InnerException.Message != null)
                {
                    strErrorMsg += " - " + e.InnerException.Message;
                }
            }

            WriteError(strErrorMsg, errorCode);
        }

        public static void WriteTlbimpGeneralException(TlbImpGeneralException tge)
        {
            WriteError(tge.Message, (ErrorCode)tge.ErrorID);
        }

        public static void WriteError(Exception e)
        {
            WriteError(null, e);
        }

        public static void WriteError(Exception e, ErrorCode errorID)
        {
            WriteError(null, e, errorID);
        }

        // General warnings
        // Note that the warningNumber corresponds to an HRESULT in src\inc\corerror.xml
        public static void WriteWarning(string message, WarningCode warningCode)
        {
            if (!CheckSilent((int)warningCode))
            {
                String warningFormat = "TlbImp : warning TI{1:0000} : {0}";
                Console.Error.WriteLine(warningFormat, message, (int)warningCode);
            }
        }

        public static void Write(String message)
        {
            if (!s_bSilent)
            {
                Console.WriteLine(message);
            }            
        }

        public static void WriteInfo(String message, MessageCode code)
        {
            if (!s_bSilent)
            {
                // We've decided to still use TlbImp prefix to tell user that this message is outputed by TlbImp
                // and we hide the message code
                String messageFormat = "TlbImp : {0}";
                Console.WriteLine(messageFormat, message);
            }
        }

        public static void SetSilent(bool silent)
        {
            if (s_silenceList.Count != 0)
            {
                SilentExclusive();
            }
            s_bSilent = silent;
        }
        public static void Silence(int warningNumber)
        {
            if(s_bSilent)
            {
                SilentExclusive();
            }
            s_silenceList.Add(warningNumber);
        }
        public static void Silence(System.Collections.Generic.List<int> silenceList)
        {
            if(s_bSilent && silenceList != null && silenceList.Count > 0)
            {
                SilentExclusive();
            }
            if (silenceList != null)
            {
                s_silenceList = silenceList;
            }
        }
        private static bool CheckSilent(int warningNumber)
        {
            if (s_bSilent || s_silenceList.Contains(warningNumber))
            {
                return true;
            }
            return false;
        }
        private static void SilentExclusive()
        {
            throw new TlbImpGeneralException(Resource.FormatString("Err_SilentExclusive"), ErrorCode.Err_SilentExclusive);
        }

        private static bool s_bSilent = false;
        private static System.Collections.Generic.List<int> s_silenceList = new System.Collections.Generic.List<int>();
    }
}
