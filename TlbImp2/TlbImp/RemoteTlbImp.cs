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
using System.Collections;
using System.Runtime.InteropServices;
using CoreRuleEngine;

namespace TlbImpCode {

public class RemoteTlbImp : MarshalByRefObject
{
    public int Run(String               strTypeLibName,
                   String               strAssemblyName,
                   String               strAssemblyNamespace,
                   String               strOutputDir,
                   byte[]               aPublicKey,
                   StrongNameKeyPair    sKeyPair,
                   String               strAssemblyRefList,
                   String               strTypeLibRefList,
                   Version              AssemblyVersion,
                   TypeLibImporterFlags flags,
                   bool                 bNoLogo,
                   bool                 bSilentMode,
                   System.Collections.Generic.List<int> silenceList,
                   bool                 bVerboseMode,
                   bool                 bStrictRef,
                   bool                 bStrictRefNoPia,
                   bool                 bSearchPathSucceeded,
                   String               strProduct,
                   String               strProductVersion,
                   String               strCompany,
                   String               strCopyright,
                   String               strTrademark,
                   bool                 isVersion2,
                   bool                 isPreserveSig,
                   String               ruleSetFileName)
    {

        TlbImpOptions options = new TlbImpOptions();
        options.m_strTypeLibName = strTypeLibName;
        options.m_strAssemblyName = strAssemblyName;
        options.m_strAssemblyNamespace = strAssemblyNamespace;
        options.m_strOutputDir = strOutputDir;
        options.m_aPublicKey = aPublicKey;
        options.m_sKeyPair = sKeyPair;
        options.m_strAssemblyRefList = strAssemblyRefList;
        options.m_strTypeLibRefList = strTypeLibRefList;
        options.m_AssemblyVersion = AssemblyVersion;
        options.m_flags = flags;
        options.m_bNoLogo = bNoLogo;
        options.m_bSilentMode = bSilentMode;
        options.m_silenceList = silenceList;
        options.m_bVerboseMode = bVerboseMode;
        options.m_bStrictRef = bStrictRef;
        options.m_bStrictRefNoPia = bStrictRefNoPia;
        options.m_bSearchPathSucceeded = bSearchPathSucceeded;
        options.m_strProduct = strProduct;
        options.m_strProductVersion = strProductVersion;
        options.m_strCompany = strCompany;
        options.m_strCopyright = strCopyright;
        options.m_strTrademark = strTrademark;
        options.m_isVersion2 = isVersion2;
        options.m_isPreserveSig = isPreserveSig;
        options.m_ruleSetFileName = ruleSetFileName;
        
        return TlbImpCode.Run(options);
    }
}

}
