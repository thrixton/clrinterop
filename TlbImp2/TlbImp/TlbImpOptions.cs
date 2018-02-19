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
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using CoreRuleEngine;

namespace TlbImpCode {

[Serializable()] 
public sealed class TlbImpOptions
{
    public String               m_strTypeLibName = null;
    public String               m_strAssemblyName = null;
    public String               m_strAssemblyNamespace = null;
    public String               m_strOutputDir = null;
    public byte[]               m_aPublicKey = null;
    public StrongNameKeyPair    m_sKeyPair = null;
    public String               m_strAssemblyRefList = null;
    public String               m_strTypeLibRefList = null;
    public Version              m_AssemblyVersion = null;
    public TypeLibImporterFlags m_flags = 0;
    public bool                 m_bNoLogo = false;
    public bool                 m_bSilentMode = false;
    public System.Collections.Generic.List<int> m_silenceList = new System.Collections.Generic.List<int>();
    public bool                 m_bVerboseMode = false;
    public bool                 m_bStrictRef = false;
    public bool                 m_bStrictRefNoPia = false;
    public bool                 m_bSearchPathSucceeded = false;
    public String               m_strProduct = null;
    public String               m_strProductVersion = null;
    public String               m_strCompany = null;
    public String               m_strCopyright = null;
    public String               m_strTrademark = null;
    public bool                 m_isVersion2 = false;
    public bool                 m_isPreserveSig = false;
    public String               m_ruleSetFileName = null;
}

}
