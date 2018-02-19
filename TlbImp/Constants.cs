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

namespace tlbimp2
{
    /// <summary>
    /// List of well known guids
    /// </summary>
    class WellKnownGuids
    {
        public static readonly Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
        public static readonly Guid IID_IDispatch = new Guid("00020400-0000-0000-C000-000000000046");
        public static readonly Guid IID_IDispatchEx = new Guid("A6EF9860-C720-11d0-9337-00A0C90DCAA9");
        public static readonly Guid IID_IEnumVARIANT = new Guid("00020404-0000-0000-C000-000000000046");
        public static readonly Guid IID_ITypeInfo = new Guid("00020401-0000-0000-C000-000000000046");
        public static readonly Guid IID_ITypeInfo2 = new Guid("00020412-0000-0000-C000-000000000046");
        public static readonly Guid IID_IEnumerable = new Guid("496b0abe-cdee-11d3-88e8-00902754c43a");
        public static readonly Guid TYPELIBID_STDOLE2 = new Guid("00020430-0000-0000-C000-000000000046");
    }

    /// <summary>
    /// List of guids for all the custom attributes
    /// </summary>
    class CustomAttributeGuids
    {
        public static readonly Guid GUID_ManagedName = new Guid("{0F21F359-AB84-41E8-9A78-36D110E6D2F9}");
        public static readonly Guid GUID_ExportedFromComPlus = new Guid("{90883F05-3D28-11D2-8F17-00A0C9A6186D}");
        public static readonly Guid GUID_ForceIEnumerable = new Guid("{B64784EB-D8D4-4d9b-9ACD-0E30806426F7}");
        public static readonly Guid GUID_DispIdOverride = new Guid("{CD2BC5C9-F452-4326-B714-F9C539D4DA58}");
        public static readonly Guid GUID_PropGetCA = new Guid("{2941FF83-88D8-4F73-B6A9-BDF8712D000D}");
        public static readonly Guid GUID_PropPutCA = new Guid("{29533527-3683-4364-ABC0-DB1ADD822FA2}");
        public static readonly Guid GUID_Function2Getter = new Guid("{54FC8F55-38DE-4703-9C4E-250351302B1C}");
    }

    /// <summary>
    /// List of well known DISP_ID
    /// </summary>
    class WellKnownDispId
    {
        public const int DISPID_VALUE = 0;
        public const int DISPID_NEWENUM = -4;
    }

    /// <summary>
    /// List of HRESULTs
    /// </summary>
    class HResults
    {
        public const uint TYPE_E_CANTLOADLIBRARY = 0x80029c4a;
    }
}