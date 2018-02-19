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

/// <summary>
/// The following enums define the code of four categories:
/// 1. Error
/// 2. Warning
/// 3. Command line Error
/// 4. Message
/// We followed the convention of cl (c++ compiler) to seperate command line error from general error
/// </summary>
namespace tlbimp2
{
    public enum ErrorCode : int
    {
        // Error code: 1000~1999
        Err_UnexpectedException = 1000,
        Err_InputFileNotFound = 1001, 
        Err_InputFileNotValidTypeLib = 1002,
        Err_TypeLibLoad = 1003,
        Err_InvalidExtension = 1004,
        Err_OutputFileNameTooLong = 1005,
        Err_OutputWouldOverwriteInput = 1006,
        Err_InvalidOutputDirectory = 1007,
        Err_TypeLoadExceptions = 1008,
        Err_ErrorWhileOpenningFile = 1009,
        Err_ResourceNotFound = 1010,
        Err_RegisteredPIANotPIA = 1011,
        Err_ReferencedPIANotPIA = 1012,
        Err_RefAssemblyCantLoad = 1013,
        Err_RefAssemblyNotFound = 1014,
        Err_RefAssemblyInvalid = 1015,
        Err_RefNotInList = 1016,
        Err_CannotCreateAppDomain = 1017,
        Err_CannotCreateRemoteTlbImp = 1018,
        Err_OutputCannotBeDirectory = 1019,
        Err_InvalidStrongName = 1020,
        Err_PIAMustBeStrongNamed = 1021,
        Err_MultipleVersionsOfAssembly = 1022,
        Err_ReferenceNotFound = 1023,
        Err_RefTlbCantLoad = 1024,
        Err_NoPIARegistered = 1025,
        Err_RefAsmOverwrittenByOutput = 1026,
        Err_ExistingAsmOverwrittenByRefAsm = 1027,
        Err_ExistingFileOverwrittenByRefAsm = 1028,
        Err_CircularImport = 1029,
        Err_PermissionException = 1030,
        Err_FatalErrorInConversion_Named = 1031,
        Err_FatalErrorInConversion_Unnamed = 1032,
        Err_CanotFindReferencedType = 1033,
        Err_RefTypeLibMissing = 1034,

        // Command line error code: 2000~2999
        Err_AmbigousOption = 2001,
        Err_UnknownOption = 2002,
        Err_NoValueRequired = 2003,
        Err_ValueRequired = 2004,
        Err_NoInputFile = 2005,
        Err_TooManyKeys = 2006,
        Err_UnknownStrictRefOpt = 2007,
        Err_InvalidTransform = 2008,
        Err_InvalidMachine = 2009,
        Err_BadMachineSwitch = 2010,
        Err_SilentExclusive = 2011,
        Err_InvalidVersion = 2012,
    }

    public enum WarningCode : int
    {
        // Warning code: 3000~3999
        Wrn_PIARegisteredForTlb = 3001,
        Wrn_AgnosticAssembly = 3002,
        Wrn_AmbiguousReturn = 3003,
        Wrn_BadVtType = 3004,
        Wrn_IEnumCustomAttributeOnIUnknown = 3005,
        Wrn_DuplicateTypeName = 3006,
        Wrn_EventWithNewEnum = 3007,
        Wrn_InvalidTypeInfo = 3008,
        Wrn_InvalidTypeInfo_Unnamed = 3009,
        Wrn_MultipleLcids = 3010,
        Wrn_NotIUnknown = 3011,
        Wrn_PropgetWithoutReturn = 3012,
        Wrn_NoPropsInEvents = 3013,
        Wrn_NonIntegralCustomAttributeType = 3014,
        Wrn_UnconvertableArgs = 3015,
        Wrn_UnconvertableField = 3016,
        Wrn_MultiNewEnum = 3017,
        Wrn_CircularReference = 3018,
        Wrn_DualNotDispatch = 3019,
        Wrn_InvalidNamespace = 3020,
        Wrn_ParamErrorNamed = 3021,
        Wrn_ParamErrorUnnamed = 3022,
        Wrn_BadVTable = 3023,
    }

    public enum MessageCode : int
    {
        // Message code: 4000~4999
        Msg_TypeLibImported = 4001,
        Msg_RefFoundInAsmRefList = 4002,
        Msg_ResolvedFile = 4003,
        Msg_ResolvedRefToPIA = 4004,
        Msg_ResolvingRef = 4005,
        Msg_AssemblyLoaded = 4006,
        Msg_AssemblyResolved = 4007,
        Msg_AutoImportingTypeLib = 4008,
        Msg_DisplayException = 4009,
        Msg_DisplayNestedException = 4010,
        Msg_AsmRefLookupMatchProblem = 4011,
        Msg_TypeInfoImported = 4012,
        Msg_TypeLibRefResolved = 4013,
        Msg_TypeLibRefResolvedInRegistry = 4014,
        Msg_TypeLibRefMismatch = 4015,
        Msg_TypeLibRefResolveFailed = 4016,
        Msg_PropertyIsOmitted = 4017
    }
}
