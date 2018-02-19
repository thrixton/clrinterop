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
    /// All global entities that will be created by TlbImp
    /// </summary>
    enum ConvType
    { 
        Struct, 
        Interface, 
        Enum, 
        Union, 
        CoClass,
        ClassInterface,
        EventInterface,
        Module
    }

    /// <summary>
    /// Exernal type or local type? See comments for IConvBase for details
    /// </summary>
    enum ConvScope
    {
        Local,              // Local => meaning that the corresponding managed type is the active type library that we are converting
        External            // External => meaning that this managed type is already available in a different DLL
    }

    /// <summary>
    /// Represents native ITypeInfo => managed type conversion
    /// 
    /// All the native TypeInfo will have corresponding IConvBase instances, either local/external
    /// 
    /// The creation of IConvBase instance is usually done in two steps:
    /// ===Step 1 (in constructor) ===
    /// Determine the name and create type builder
    /// 
    /// ===Step 2 (in create function) ===
    /// Use the type builder to add members and create actual type
    /// 
    /// In order to abstract away the difference between local types & external types, I use the following approach:
    /// Use a interface to derive from IConvBase, and have two different classes represent local/external type respectively
    /// implementing this interface. We only need to access using this interface instead of using the class directly, thus
    /// avoid relying on the fact that the IConvBase instance is external or local. 
    /// 
    /// IConvInterface : IConvBase
    /// ConvInterfaceLocal : IConvInterface    => Local version
    /// ConvInterfaceExternal : IConvInterface => External version
    /// 
    /// These derived classes (ConvInterfaceLocal, ConvInterfaceExternal) represents 
    /// actual conversions (ConvCoClassLocal/External, ConvStructLocal/External, 
    /// ConvInterfaceLocal/External, ConvEnumLocal/External, ConvUnionLocal/External,
    /// ConvClassInterfaceLocal/External, etc.). IConvBase is used for references and
    /// definitions. TypeBuilder is derived from Type and can be used to generate 
    /// references to the type being constructed. During the conversion process, there
    /// can be an ITypeInfo encountered for a given type that hasn't been converted yet.
    /// This is typically when the type is used as a parameter or field type definition.
    /// In this case, ConverterInfo::GetTypeRef is called. The algorithm looks like this:
    /// 
    ///  1) Is the type already present in the symbol table? Return if present.
    ///  2) Is the type defined in another type library?
    ///    (Yes) Is type library missing from our internal mapping table?
    ///       (Yes) Resolve type library to assembly through callback and add to table.
    ///    *** - Load type in other assembly
    ///    *** - Create ConvGeneric item to house type and add to symbol table
    ///    *** - Return item
    ///  3) Create an appropriate IConvBase derived class for the given ITypeInfo. Just
    ///     do enough work to create a TypeBuilder and add the item to the symbol table.
    ///     When the time is right to actually do the conversion, pull this in progress
    ///     item from the symbol table and complete the type definition process via
    ///     the GetXXX methods.
    /// 
    /// *** Regarding External Types***
    /// There are many questions that left to be answered for external types
    /// 1. Say TypeLib_1 ref TypeLib_2, TypeLib_2 is imported and we'll have local types for them. Shall we re-use those local types?
    /// My suggestion: not for now. This will break many assumptions within the code.
    /// 
    /// 2. This is sort of related to 1. Shall we maintain multiple ConverterInfo instances for multiple libs?
    /// My suggestion: not for now. This will break many assumptions within the code.
    /// 
    /// 3. We don't have a problem for external types that has corresponding type info. But what do we do for coclass/class interface/event interface? 
    /// We shouldn't have reference for external coclass/class interface. But it is possible for event interfaces.
    /// For example, TypeLib_1::CoClass has source TypeLib_2::IA, in this case the class interface & coclass will have to process
    /// external event interface TypeLib_2::IA_Event. Question is shall we create a new one or reuse the old one in TypeLib_2.DLL?
    /// My suggestion: We create a new one. Old version of TlbImp already does that. 
    /// 
    /// </summary>
    interface IConvBase
    {
        /// <summary>
        /// The TypeInfo. The reason I name it Ref is because this property returns a existing TypeInfo instead of  a new one
        /// </summary>
        TypeInfo RefTypeInfo
        {
            get;
        }

        /// <summary>
        /// The always non-aliased TypeInfo
        /// </summary>
        TypeInfo RefNonAliasedTypeInfo
        {
            get;
        }

        /// <summary>
        /// ManagedType after conversion
        /// In some cases this is not the real managed type this ITypeInfo represents.
        /// One example of this is the DefaultInterface => ClassInterface mapping
        /// </summary>
        Type ManagedType
        {
            get;
        }

        /// <summary>
        /// Real ManagedType after conversion. This is always the true managed type converted from ITypeInfo
        /// </summary>
        Type RealManagedType
        {
            get;
        }

        /// <summary>
        /// Managed name. Must be unique. Usually we decide the name at the constructor and just use the type builder's name here
        /// </summary>
        string ManagedName
        {
            get;
        }

        /// <summary>
        /// Get the ConvType for this IConvBase
        /// </summary>
        ConvType ConvType
        {
            get;
        }

        /// <summary>
        /// Scope => Local / External
        /// </summary>
        ConvScope ConvScope
        {
            get;
        }

        /// <summary>
        /// Create the type
        /// </summary>
        void Create();
    }

}