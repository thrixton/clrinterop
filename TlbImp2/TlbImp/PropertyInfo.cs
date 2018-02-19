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
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using TlbImpCode;
using TypeLibTypes.Interop;

namespace tlbimp2
{
    /// <summary>
    /// Represent a property which has get/put/putref methods
    /// Collect the MethodBuilder for get/put/putref and generate a property
    /// </summary>
    class ConvProperty
    {
        public ConvProperty(InterfacePropertyInfo propertyInfo)
        {
            m_propertyInfo = propertyInfo;
        }

        public void SetGetMethod(MethodBuilder method)
        {
            m_methodGet = method;
        }

        public void SetPutMethod(MethodBuilder method)
        {
            m_methodPut = method;
        }

        public void SetPutRefMethod(MethodBuilder method)
        {
            m_methodPutRef = method;
        }

        /// <summary>
        /// Generate properties for Functions
        /// </summary>
        public void GenerateProperty(InterfaceInfo info, TypeBuilder typebuilder)
        {
            // Generate property using unique name
            string uniqueName = info.GenerateUniqueMemberName(
                m_propertyInfo.RecommendedName,
                null, 
                MemberTypes.Property);
            
            //
            // Convert the signature
            // 
            Type[] paramTypes = null;
            Type retType = null;
            
            if (m_propertyInfo.Kind == PropertyKind.VarProperty)
            {
                // Converting variable to property. There are no parameters at all
                TypeConverter typeConverter = m_propertyInfo.GetPropertyTypeConverter(info.ConverterInfo, info.RefTypeInfo);
                info.IsConversionLoss |= typeConverter.IsConversionLoss;
                retType = typeConverter.ConvertedType;
            }
            else
            {
                // If /preservesig is specified, do not generate property, and only
                // the getter and setter functions are enough.
                // Property requires that the property getter must return the real property value, and the first parameter of
                // the setter must be the property value. While, the /preservesig switch will change the prototype of the setters and
                // getters, which is different from what the compiler expected.
                // So we do not support the Property if the /preservesig is specified.
                if (info.ConverterInfo.Settings.m_isPreserveSig &&
                    (m_propertyInfo.BestFuncDesc != null && !m_propertyInfo.BestFuncDesc.IsDispatch))
                {
                    if (TlbImpCode.TlbImpCode.s_Options.m_bVerboseMode)
                        FormattedOutput.Output.WriteInfo(Resource.FormatString("Msg_PropertyIsOmitted",
                                                                               m_propertyInfo.RecommendedName,
                                                                               m_propertyInfo.RefTypeInfo.GetDocumentation()),
                                                         MessageCode.Msg_PropertyIsOmitted);
                    return;
                }

                // Converting propget/propput/propputref functions to property.
                TypeConverter typeConverter = m_propertyInfo.GetPropertyTypeConverter(info.ConverterInfo, info.RefTypeInfo);
                retType = typeConverter.ConvertedType;
                info.IsConversionLoss |= typeConverter.IsConversionLoss;

                FuncDesc bestFuncDesc = m_propertyInfo.BestFuncDesc;

                // if we have a [vararg]
                int varArg, firstOptArg, lastOptArg;

                if (bestFuncDesc.cParamsOpt == -1)
                    ConvCommon.CheckForOptionalArguments(info.ConverterInfo, bestFuncDesc, out varArg, out firstOptArg, out lastOptArg);
                else
                    varArg = -1;

                List<Type> paramTypeList = new List<Type>();

                // Find the index part of the property's signature
                bool skipLastRetVal = (bestFuncDesc.IsPropertyPut || bestFuncDesc.IsPropertyPutRef);
                for (int i = 0; i < bestFuncDesc.cParams; ++i)
                {
                    ElemDesc elemDesc = bestFuncDesc.GetElemDesc(i);
                    ParamDesc paramDesc = elemDesc.paramdesc;

                    // Skip LCID/RetVal
                    if (paramDesc.IsLCID || paramDesc.IsRetval)
                        continue;

                    // Skip the "new value" parameter for putters
                    if (skipLastRetVal)
                    {
                        skipLastRetVal = false;
                        continue;
                    }

                    ConversionType conversionType;
                    if (i == varArg)
                        conversionType = ConversionType.VarArgParameter;
                    else
                        conversionType = ConversionType.Parameter;

                    TypeConverter paramTypeConverter = new TypeConverter(info.ConverterInfo, info.RefTypeInfo, elemDesc.tdesc, conversionType);
                    info.IsConversionLoss |= paramTypeConverter.IsConversionLoss;
                    paramTypeList.Add(paramTypeConverter.ConvertedType);
                }

                paramTypes = paramTypeList.ToArray();
            }

            // Define the property
            PropertyBuilder propertyBuilder = typebuilder.DefineProperty(uniqueName, PropertyAttributes.HasDefault, retType, paramTypes);

            if (info.IsCoClass && !info.IsDefaultInterface)
            {
                // Skip non-default interfaces / implemented interfaces (when we are creating coclass)
            }
            else
            {
                // Emit dispatch id attribute
                propertyBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForDispId(m_propertyInfo.DispId));
            }

            // We don't need to emit MarshalAs for properties because the get/set functions should already have them
            // Emitting MarshalAs for property will hang up CLR!!
            if (m_methodGet != null)
            {
                propertyBuilder.SetGetMethod(m_methodGet);
            }

            // Has both propPut & propPutRef?
            if (m_methodPut != null && m_methodPutRef != null)
            {
                propertyBuilder.SetSetMethod(m_methodPutRef);
                propertyBuilder.AddOtherMethod(m_methodPut);
            }
            else
            {
                if (m_methodPut != null)
                {
                    propertyBuilder.SetSetMethod(m_methodPut);
                }
                else if (m_methodPutRef != null)
                {
                    propertyBuilder.SetSetMethod(m_methodPutRef);
                }              
            }
            
            //
            // Handle DefaultMemberAttribute
            //
            if (m_propertyInfo.DispId == WellKnownDispId.DISPID_VALUE)
            {
                // DIFF: TlbImpv1 use the type library name while we use the unique name
                info.ConverterInfo.SetDefaultMember(info.TypeBuilder, uniqueName);
            }
            
            // Handle alias information
            ConvCommon.HandleAlias(info.ConverterInfo, info.RefTypeInfo, m_propertyInfo.PropertyTypeDesc, propertyBuilder);
        }

        private MethodBuilder m_methodGet;              // Method body for propget
        private MethodBuilder m_methodPut;              // Method body for propput
        private MethodBuilder m_methodPutRef;           // Method body for propputref
        private InterfacePropertyInfo m_propertyInfo;   // Information for the property generated by the IConvInterface
    }

    /// <summary>
    /// Represent all the properties for the interface used to create the managed properties
    /// </summary>
    class PropertyInfo
    {
        public PropertyInfo(InterfaceInfo info)
        {
            m_info = info;
            Debug.Assert(info != null);
        }

        /// <summary>
        /// Remember the InterfaceMemberInfo/MethodBuilder information for creating properties later
        /// </summary>
        public void SetPropertyInfo(InterfaceMemberInfo memberInfo, MethodBuilder method)
        {
            int dispId = memberInfo.MemId;
            ConvProperty property = null;
            if (!m_properties.TryGetValue(dispId, out property))
            {
                property = new ConvProperty(memberInfo.PropertyInfo);
                m_properties.Add(dispId, property);
            }
            if (memberInfo.IsPropertyGet)
            {
                property.SetGetMethod(method);
            }
            if (memberInfo.IsPropertyPut)
            {
                property.SetPutMethod(method);
            }
            if (memberInfo.IsPropertyPutRef)
            {
                property.SetPutRefMethod(method);
            }
        }

        /// <summary>
        /// Generate the actual property (not the property accessors)
        /// </summary>
        public void GenerateProperties()
        {
            foreach(KeyValuePair<int, ConvProperty> pair in m_properties)
            {
                ConvProperty property = pair.Value;
                property.GenerateProperty(m_info, m_info.TypeBuilder);
            }

            // Clear all properties so that we can re-use the same interface info again
            m_properties.Clear();
        }

        private InterfaceInfo m_info;  
        private Dictionary<int, ConvProperty> m_properties = new Dictionary<int, ConvProperty>();
    }
}
