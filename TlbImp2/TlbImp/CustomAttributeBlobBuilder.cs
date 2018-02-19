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
using System.Diagnostics;

namespace tlbimp2
{
    /// <summary>
    /// Build the byte blob for a custom attribute
    /// Please refer to Common Language Runtime Infrastructure Annotated Standard for details about
    /// the structure of the byte blob
    /// Only support string at this moment
    /// </summary>
    class CustomAttributeBlobBuilder
    {
        /// <summary>
        /// Fixed args in the custom attribute blob
        /// </summary>
        private List<Object> m_fixedArgs;

        /// <summary>
        /// Optional named args in the custom attribute
        /// </summary>
        private List<Object> m_namedArgs;

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomAttributeBlobBuilder()
        {
            m_fixedArgs = new List<object>();
            m_namedArgs = new List<object>();
        }

        /// <summary>
        /// Adds a string as fixed argument
        /// </summary>
        /// <param name="arg">The string as fixed arg</param>
        public void AddFixedArg(string arg)
        {
            m_fixedArgs.Add(arg);
        }

        /// <summary>
        /// Return the blob in bytes for all the fixed args & named args so far
        /// </summary>
        /// <returns>The blob in bytes</returns>
        public Byte[] GetBlob()
        {
            List<byte> bytes = new List<byte>();

            AppendProlog(bytes);
            
            //
            // Write fixed args
            //
            foreach (object fixedArg in m_fixedArgs)
            {
                if (fixedArg is string)
                {
                    AppendFixedArg(bytes, (string)fixedArg);
                }
            }

            // 
            // Write named args
            //
            AppendNumNamedArgs(bytes, m_namedArgs.Count);
            foreach (object namedArg in m_namedArgs)
            {

            }

            return bytes.ToArray();
        }

        #region Private functions for building blob

        private void AppendProlog(List<byte> bytes)
        {
            bytes.Add(0x01);
            bytes.Add(0x00);
        }

        private void AppendFixedArg(List<byte> bytes, string arg)
        {
            if (arg != null)
            {
                byte[] utf8 = Encoding.UTF8.GetBytes(arg);
                AppendPackedLen(bytes, utf8.Length);
                bytes.AddRange(utf8);
            }
            else
                bytes.Add(0xff);
        }

        private void AppendNumNamedArgs(List<byte> bytes, int len)
        {
            Debug.Assert(len < short.MaxValue);
            bytes.Add((byte)(len & 0xff));
            bytes.Add((byte)(len >> 8));
        }

        private void AppendPackedLen(List<byte> bytes, int len)
        {
            if (len <= 0x7F)
            {
                bytes.Add((byte)len);
                return;
            }

            if (len <= 0x3FFF)
            {
                bytes.Add((byte)((len >> 8) | 0x80));
                bytes.Add((byte)(len & 0xFF));
                return;
            }

            Debug.Assert(len <= 0x1FFFFFFF);

            bytes.Add((byte)((len >> 24) | 0xC0));
            bytes.Add((byte)((len >> 16) & 0xFF)); 
            bytes.Add((byte)((len >> 8) & 0xFF));
            bytes.Add((byte)(len & 0xFF));
        }
        #endregion

    }
}
