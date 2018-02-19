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
using System.Runtime.InteropServices;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace tlbimp2
{
    /// <summary>
    /// Helper class used to create CustomAttributeBuilder for different kinds of CustomAttributes
    /// We can also speed up performance by saving instances of ConstructorInfo or CustomAttributeBuilder
    /// </summary>
    class CustomAttributeHelper
    {
        public static CustomAttributeBuilder GetBuilderForGuid(Guid guid)
        {            
            return GetBuilderForGuid(guid.ToString());
        }

        public static CustomAttributeBuilder GetBuilderForGuid(string guid)
        {
            ConstructorInfo ctorGuid = typeof(GuidAttribute).GetConstructor(new Type[] { typeof(string) });
            // TlbImpv1 will some times output upper case, and some times will output lower case
            // TlbImp2 always emits upper case
            return new CustomAttributeBuilder(ctorGuid, new Object[] { guid.ToUpper() });
        }

        public static CustomAttributeBuilder GetBuilderForInterfaceType(ComInterfaceType interfaceType)
        {
            ConstructorInfo ctorInterfaceType = typeof(InterfaceTypeAttribute).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(ComInterfaceType) },
                null);
            return new CustomAttributeBuilder(ctorInterfaceType, new Object[] { interfaceType });
        }

        public static CustomAttributeBuilder GetBuilderForComVisible(bool isVisible)
        {
            ConstructorInfo ctorComVisible = typeof(ComVisibleAttribute).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(bool) },
                null);
            return new CustomAttributeBuilder(ctorComVisible, new Object[] { isVisible });

        }

        public static CustomAttributeBuilder GetBuilderForClassInterface(ClassInterfaceType classInterfaceType)
        {
            ConstructorInfo ctorClassInterface = typeof(ClassInterfaceAttribute).GetConstructor(new Type[] { typeof(ClassInterfaceType) });
            return new CustomAttributeBuilder(ctorClassInterface, new Object[] { classInterfaceType });
        }

        public static CustomAttributeBuilder GetBuilderForTypeLibType(TypeLibTypeFlags flags)
        {
            ConstructorInfo ctorTypeLibType = typeof(TypeLibTypeAttribute).GetConstructor(new Type[] { typeof(TypeLibTypeFlags) });
            return new CustomAttributeBuilder(ctorTypeLibType, new Object[] { flags });
        }

        public static CustomAttributeBuilder GetBuilderForTypeLibVar(TypeLibVarFlags flags)
        {
            ConstructorInfo ctorTypeLibVar = typeof(TypeLibVarAttribute).GetConstructor(new Type[] { typeof(TypeLibVarFlags) });
            return new CustomAttributeBuilder(ctorTypeLibVar, new Object[] { flags });
        }

        public static CustomAttributeBuilder GetBuilderForTypeLibFunc(TypeLibFuncFlags flags)
        {
            ConstructorInfo ctorTypeLibFunc = typeof(TypeLibFuncAttribute).GetConstructor(new Type[] { typeof(TypeLibFuncFlags) });
            return new CustomAttributeBuilder(ctorTypeLibFunc, new Object[] { flags });
        }

        public static CustomAttributeBuilder GetBuilderForComSourceInterfaces(string sourceInterfaceNames)
        {
            ConstructorInfo ctorComSourceInterfaces = typeof(ComSourceInterfacesAttribute).GetConstructor(new Type[] { typeof(string) });
            return new CustomAttributeBuilder(ctorComSourceInterfaces, new Object[] { sourceInterfaceNames });
        }

        public static CustomAttributeBuilder GetBuilderForIn()
        {
            ConstructorInfo ctorIn = typeof(InAttribute).GetConstructor(new Type[] { });
            return new CustomAttributeBuilder(ctorIn, new Object[] { });
        }

        public static CustomAttributeBuilder GetBuilderForOut()
        {
            ConstructorInfo ctorOut = typeof(OutAttribute).GetConstructor(new Type[] { });
            return new CustomAttributeBuilder(ctorOut, new Object[] { });
        }

        public static CustomAttributeBuilder GetBuilderForMarshalAs(UnmanagedType unmanagedType)
        {
            ConstructorInfo ctorMarshalAs = typeof(MarshalAsAttribute).GetConstructor(new Type[] { typeof(UnmanagedType) });
            return new CustomAttributeBuilder(ctorMarshalAs, new Object[] { unmanagedType });
        }

        public static CustomAttributeBuilder GetBuilderForMarshalAsConstArray(UnmanagedType unmanagedType, int length)
        {
            ConstructorInfo ctorMarshalAs = typeof(MarshalAsAttribute).GetConstructor(new Type[] { typeof(UnmanagedType) });
            FieldInfo fieldSizeConst = typeof(MarshalAsAttribute).GetField("SizeConst");
            return new CustomAttributeBuilder(
                ctorMarshalAs, 
                new Object[] { unmanagedType },
                new FieldInfo[] { fieldSizeConst },
                new Object[] { length }
                );
        }

        public static CustomAttributeBuilder GetBuilderForMarshalAsConstArray(UnmanagedType unmanagedType, int length, UnmanagedType arraySubType)
        {
            ConstructorInfo ctorMarshalAs = typeof(MarshalAsAttribute).GetConstructor(new Type[] { typeof(UnmanagedType) });
            FieldInfo fieldSizeConst = typeof(MarshalAsAttribute).GetField("SizeConst");
            FieldInfo fieldArraySubType = typeof(MarshalAsAttribute).GetField("ArraySubType");
            return new CustomAttributeBuilder(
                ctorMarshalAs,
                new Object[] { unmanagedType },
                new FieldInfo[] { fieldSizeConst, fieldArraySubType },
                new Object[] { length, arraySubType }
                );
        }

        public static CustomAttributeBuilder GetBuilderForMarshalAsCustomMarshaler(Type customMarshaler, string marshalCookie)
        {
            ConstructorInfo ctorMarshalAs = typeof(MarshalAsAttribute).GetConstructor(new Type[] { typeof(UnmanagedType) });
            FieldInfo fieldMarshalTypeRef = typeof(MarshalAsAttribute).GetField("MarshalTypeRef");
            FieldInfo fieldMarshalCookie = typeof(MarshalAsAttribute).GetField("MarshalCookie");

            return new CustomAttributeBuilder(
                ctorMarshalAs,
                new Object[] { UnmanagedType.CustomMarshaler },
                new FieldInfo[] { fieldMarshalTypeRef, fieldMarshalCookie },
                new Object[] { customMarshaler, marshalCookie }
                );
        }

        public static CustomAttributeBuilder GetBuilderForMarshalAsSafeArray(VarEnum safeArraySubType)
        {
            ConstructorInfo ctorMarshalAs = typeof(MarshalAsAttribute).GetConstructor(new Type[] { typeof(UnmanagedType) });
            FieldInfo fieldSafeArraySubType = typeof(MarshalAsAttribute).GetField("SafeArraySubType");
            return new CustomAttributeBuilder(
                ctorMarshalAs, 
                new Object[] { UnmanagedType.SafeArray },
                new FieldInfo[] { fieldSafeArraySubType },
                new Object[] { safeArraySubType }
                );
        }

        public static CustomAttributeBuilder GetBuilderForMarshalAsSafeArrayAndUserDefinedSubType(VarEnum safeArraySubType, Type safeArrayUserDefinedSubType)
        {
            ConstructorInfo ctorMarshalAs = typeof(MarshalAsAttribute).GetConstructor(new Type[] { typeof(UnmanagedType) });
            FieldInfo fieldSafeArraySubType = typeof(MarshalAsAttribute).GetField("SafeArraySubType");
            FieldInfo fieldSafeArrayUserDefinedSubType = typeof(MarshalAsAttribute).GetField("SafeArrayUserDefinedSubType");
            return new CustomAttributeBuilder(
                ctorMarshalAs,
                new Object[] { UnmanagedType.SafeArray },
                new FieldInfo[] { fieldSafeArraySubType, fieldSafeArrayUserDefinedSubType },
                new Object[] { safeArraySubType, safeArrayUserDefinedSubType }
                );
        }

        public static CustomAttributeBuilder GetBuilderForParamArray()
        {
            ConstructorInfo ctorParamArray = typeof(ParamArrayAttribute).GetConstructor(new Type[] { });
            return new CustomAttributeBuilder(ctorParamArray, new Object[] { });
        }

        public static CustomAttributeBuilder GetBuilderForImportedFromTypeLib(string tlbFile)
        {
            ConstructorInfo ctorImportedFromTypeLib = typeof(ImportedFromTypeLibAttribute).GetConstructor(new Type[] { typeof(string) });
            return new CustomAttributeBuilder(ctorImportedFromTypeLib, new Object[] { tlbFile });
        }

        public static CustomAttributeBuilder GetBuilderForTypeLibVersion(int majorVer, int minorVer)
        {
            ConstructorInfo ctorTypeLibVersion = typeof(TypeLibVersionAttribute).GetConstructor(new Type[] { typeof(int), typeof(int) });
            return new CustomAttributeBuilder(ctorTypeLibVersion, new Object[] { majorVer, minorVer });
        }

        public static CustomAttributeBuilder GetBuilderForFieldOffset(int offset)
        {
            ConstructorInfo ctorFieldOffset = typeof(FieldOffsetAttribute).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(int) },
                null);
            return new CustomAttributeBuilder(ctorFieldOffset, new Object[] { (int)0 });
        }

        public static CustomAttributeBuilder GetBuilderForDispId(int dispId)
        {
            ConstructorInfo ctorDispId = typeof(DispIdAttribute).GetConstructor(new Type[] { typeof(int) });
            return new CustomAttributeBuilder(ctorDispId, new Object[] { dispId });
        }

        public static CustomAttributeBuilder GetBuilderForCoClass(Type coclass)
        {
            ConstructorInfo ctorCoClass = typeof(CoClassAttribute).GetConstructor(new Type[] { typeof(Type) });
            return new CustomAttributeBuilder(ctorCoClass, new Object[] { coclass });
        }

        public static CustomAttributeBuilder GetBuilderForStructLayout(LayoutKind layoutKind)
        {
            ConstructorInfo ctorStructLayout = typeof(StructLayoutAttribute).GetConstructor(new Type[] { typeof(LayoutKind) });
            return new CustomAttributeBuilder(ctorStructLayout, new Object[] { layoutKind });
        }

        public static CustomAttributeBuilder GetBuilderForStructLayout(LayoutKind layoutKind, int pack)
        {
            ConstructorInfo ctorStructLayout = typeof(StructLayoutAttribute).GetConstructor(new Type[] { typeof(LayoutKind) });
            FieldInfo fieldPack = typeof(StructLayoutAttribute).GetField("Pack");
            return new CustomAttributeBuilder(
                ctorStructLayout,
                new Object[] { layoutKind },
                new FieldInfo[] { fieldPack },
                new Object[] { pack });
        }

        public static CustomAttributeBuilder GetBuilderForStructLayout(LayoutKind layoutKind, int pack, int size)
        {
            ConstructorInfo ctorStructLayout = typeof(StructLayoutAttribute).GetConstructor(new Type[] { typeof(LayoutKind) });
            FieldInfo fieldPack = typeof(StructLayoutAttribute).GetField("Pack");
            FieldInfo fieldSize = typeof(StructLayoutAttribute).GetField("Size");
            return new CustomAttributeBuilder(
                ctorStructLayout, 
                new Object[] { layoutKind },
                new FieldInfo[] { fieldPack, fieldSize },
                new Object[] { pack, size });
        }

        public static CustomAttributeBuilder GetBuilderForLCIDConversion(int lcidArgPosition)
        {
            ConstructorInfo ctorLCIDConversion = typeof(LCIDConversionAttribute).GetConstructor(new Type[] { typeof(int) });
            return new CustomAttributeBuilder(ctorLCIDConversion, new Object[] { lcidArgPosition });
        }

        public static CustomAttributeBuilder GetBuilderForComConversionLoss()
        {
            ConstructorInfo ctorComConversionLoss = typeof(ComConversionLossAttribute).GetConstructor(new Type[] { });
            return new CustomAttributeBuilder(ctorComConversionLoss, new Object[] { });
        }

        public static CustomAttributeBuilder GetBuilderForSuppressUnmanagedCodeSecurity()
        {
            ConstructorInfo ctorSuppressUnmanagedCodeSecurity = typeof(System.Security.SuppressUnmanagedCodeSecurityAttribute).GetConstructor(new Type[] { });
            return new CustomAttributeBuilder(ctorSuppressUnmanagedCodeSecurity, new Object[] { });
        }

        public static CustomAttributeBuilder GetBuilderForSerializable()
        {
            ConstructorInfo ctorSerializable = typeof(SerializableAttribute).GetConstructor(new Type[] { });
            return new CustomAttributeBuilder(ctorSerializable, new Object[] { });
        }

        public static CustomAttributeBuilder GetBuilderForDecimalConstant(byte scale, byte sign, uint hi, uint mid, uint low)
        {
            // TlbimpV1 use the uint version, so I'll use the uint version to save some time
            ConstructorInfo ctorDecimalConstant = typeof(DecimalConstantAttribute).GetConstructor(
                new Type[] { typeof(byte), typeof(byte), typeof(uint), typeof(uint), typeof(uint) }
                );

            return new CustomAttributeBuilder(ctorDecimalConstant, new Object[] { scale, sign, hi, mid, low });
        }

        public static CustomAttributeBuilder GetBuilderForDateTimeConstant(long ticks)
        {
            ConstructorInfo ctorDateTimeConstant = typeof(DateTimeConstantAttribute).GetConstructor(
                new Type[] { typeof(long) }
                );

            return new CustomAttributeBuilder(ctorDateTimeConstant, new Object[] { ticks });
        }

        public static CustomAttributeBuilder GetBuilderForIUnknownConstant()
        {
            ConstructorInfo ctorIUnknownConstant = typeof(IUnknownConstantAttribute).GetConstructor(new Type[] {});

            return new CustomAttributeBuilder(ctorIUnknownConstant, new Object[] { });
        }

        public static CustomAttributeBuilder GetBuilderForIDispatchConstant()
        {
            ConstructorInfo ctorIDispatchConstant = typeof(IDispatchConstantAttribute).GetConstructor(new Type[] {});

            return new CustomAttributeBuilder(ctorIDispatchConstant, new Object[] { });
        }

        public static CustomAttributeBuilder GetBuilderForDefaultMember(string defaultMemberName)
        {
            ConstructorInfo ctorDefaultMember = typeof(DefaultMemberAttribute).GetConstructor(new Type[] { typeof(string) });
            return new CustomAttributeBuilder(ctorDefaultMember, new Object[] { defaultMemberName });
        }

        public static CustomAttributeBuilder GetBuilderForComAliasName(string comAliasName)
        {
            ConstructorInfo ctorComAliasName = typeof(ComAliasNameAttribute).GetConstructor(new Type[] { typeof(string) });
            return new CustomAttributeBuilder(ctorComAliasName, new Object[] { comAliasName });
        }

        public static CustomAttributeBuilder GetBuilderForPrimaryInteropAssembly(int major, int minor)
        {
            ConstructorInfo ctorPrimaryInteropAssembly = typeof(PrimaryInteropAssemblyAttribute).GetConstructor(new Type[] { typeof(int), typeof(int) });
            return new CustomAttributeBuilder(ctorPrimaryInteropAssembly, new Object[] { major, minor });
        }

    }
}