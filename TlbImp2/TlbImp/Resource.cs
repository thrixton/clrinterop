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
using System.Resources;
using System.Reflection;

namespace tlbimp2
{

internal class Resource
{
    // For string resources located in a file:
    private static ResourceManager _resmgr;
    
    private static void InitResourceManager()
    {
        if(_resmgr == null)
        {
            _resmgr = new ResourceManager("TlbImp2.Resources", 
                                          Assembly.GetExecutingAssembly());
        }
    }
    
    internal static String GetString(String key)
    {
        string s = null;
        s = GetStringIfExists(key);
        
        if (s == null) 
            // We are not localizing this stringas this is for invalid resource scenario
            throw new TlbImpResourceNotFoundException("The required resource string cannot be found");

        return(s);
    }

    internal static String GetStringIfExists(String key)
    {
        String s;
        try
        {
            InitResourceManager();
            s = _resmgr.GetString(key, null);
        }
        catch (System.Exception)
        {
            return null;        	
        }

        return s;
    }
    
    internal static String FormatString(String key)
    {
        return(GetString(key));
    }
    
    internal static String FormatString(String key, Object a1)
    {
        return(String.Format(GetString(key), a1));
    }
    
    internal static String FormatString(String key, Object a1, Object a2)
    {
        return(String.Format(GetString(key), a1, a2));
    }

    internal static String FormatString(String key, Object[] a)
    {
        return (String.Format(System.Globalization.CultureInfo.CurrentCulture, GetString(key), a));
    }
}

}
