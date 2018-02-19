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
    internal static class NameSpaceExtractor
    {
        private static char NameSpaceSeperator = '.';

        public static String ExtractNameSpace(String FullyQualifiedTypeName)
        {
            int TypeNameStartPos = FullyQualifiedTypeName.LastIndexOf(NameSpaceSeperator);
            if (TypeNameStartPos == -1)
                return "";
            else
                return FullyQualifiedTypeName.Substring(0, TypeNameStartPos);
        }
    }
}
