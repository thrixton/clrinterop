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

namespace tlbimp2
{
    /// <summary>
    /// Common base class of almost all ConvLocalXXX classes
    /// </summary>
    abstract class ConvLocalBase : IConvBase
    {
        protected void DefineType(ConverterInfo info, TypeInfo typeInfo, bool dealWithAlias)
        {
            m_info = info;
            m_typeInfo = typeInfo;

            if (dealWithAlias)
                m_nonAliasedTypeInfo = ConvCommon.GetAlias(typeInfo);
            else
                m_nonAliasedTypeInfo = typeInfo;

            try
            {
                OnDefineType();

                //
                // Emit SuppressUnmanagedCodeSecurityAttribute for /unsafe switch
                //
                if ((m_info.Settings.m_flags & TypeLibImporterFlags.UnsafeInterfaces) != 0)
                {
                    if (ConvType != ConvType.ClassInterface && ConvType != ConvType.EventInterface)
                        m_typeBuilder.SetCustomAttribute(CustomAttributeHelper.GetBuilderForSuppressUnmanagedCodeSecurity());
                }
            }
            catch (ReflectionTypeLoadException)
            {
                throw; // Fatal failure. Throw
            }
            catch (TlbImpResolveRefFailWrapperException)
            {
                throw; // Fatal failure. Throw
            }
            catch (TlbImpGeneralException)
            {
                throw; // Fatal failure. Throw
            }
            catch (Exception)
            {
                string name = String.Empty;
                if (m_typeInfo != null)
                {
                    try
                    {
                        name = m_typeInfo.GetDocumentation();
                    }
                    catch (Exception)
                    {
                    }
                }

                if (name != String.Empty)
                {
                    string msg = Resource.FormatString("Wrn_InvalidTypeInfo", name);
                    m_info.ReportEvent(WarningCode.Wrn_InvalidTypeInfo, msg);
                }
                else
                {
                    string msg = Resource.FormatString("Wrn_InvalidTypeInfo_Unnamed");
                    m_info.ReportEvent(WarningCode.Wrn_InvalidTypeInfo_Unnamed, msg);
                }

                // When failure, try to create the type anyway
                if (m_typeBuilder != null)
                {
                    m_type = m_typeBuilder.CreateType();
                }
            }
        }

        /// <summary>
        /// Used to reinitialize TypeInfo & NonAliasedTypeInfo in special cases. Currently it is used
        /// by ConvInterfaceLocal
        /// </summary>
        protected void ReInit(TypeInfo typeInfo, TypeInfo nonAliasedTypeInfo)
        {
            m_typeInfo = typeInfo;
            m_nonAliasedTypeInfo = nonAliasedTypeInfo;
        }

        /// <summary>
        /// Creation of managed type is split into two stages: define & create. This is the define stage.
        /// Defines the type in the assembly. This usually involves the following:
        /// 1. Defining parent types
        /// 2. Create TypeBuilder
        /// 3. Define attributes
        /// </summary>
        protected abstract void OnDefineType();

        #region IConvBase Members

        public TypeInfo RefTypeInfo
        {
            get
            {
                return m_typeInfo;
            }
        }

        public TypeInfo RefNonAliasedTypeInfo
        {
            get
            {
                return m_nonAliasedTypeInfo;
            }
        }

        public virtual Type ManagedType
        {
            get
            {
                return RealManagedType;
            }
        }

        public Type RealManagedType
        {
            get
            {
                if (m_type == null)
                    return m_typeBuilder;
                else
                    return m_type;                    
            }
        }

        public string ManagedName
        {
            get
            {
                return ManagedType.FullName;
            }
        }

        public abstract ConvType ConvType
        {
            get;
        }

        public ConvScope ConvScope
        {
            get { return ConvScope.Local; }
        }

        public void Create()
        {
            try
            {
                // Try to create if we haven't failed before
                if (!m_seenFailed && m_type == null)
                    OnCreate();
            }
            catch (ReflectionTypeLoadException)
            {
                throw; // Fatal failure. Throw
            }
            catch (TlbImpResolveRefFailWrapperException)
            {
                throw; // Fatal failure. Throw
            }
            catch (TlbImpGeneralException)
            {
                throw; // Fatal failure. Throw
            }
            catch (TypeLoadException)
            {
                throw; // TypeLoadException is critical. Throw.
            }
            catch (Exception)
            {
                string name = String.Empty;
                if (m_typeInfo != null)
                {
                    try
                    {
                        name = m_typeInfo.GetDocumentation();
                    }
                    catch (Exception)
                    {
                    }
                }

                if (name != String.Empty)
                {
                    string msg = Resource.FormatString("Wrn_InvalidTypeInfo", name);
                    m_info.ReportEvent(WarningCode.Wrn_InvalidTypeInfo, msg);
                }
                else
                {
                    string msg = Resource.FormatString("Wrn_InvalidTypeInfo_Unnamed");
                    m_info.ReportEvent(WarningCode.Wrn_InvalidTypeInfo_Unnamed, msg);
                }

                // When failure, try to create the type anyway
                if (m_type == null)
                {
                    m_seenFailed = true;
                    m_type = m_typeBuilder.CreateType();
                }
            }
        }

        public abstract void OnCreate();


        #endregion

        #region Private members

        private TypeInfo            m_nonAliasedTypeInfo;           // Non-alised type info. 
        private TypeInfo            m_typeInfo;                     // Type info for this ConvXXX class. Could be aliased
        private bool                m_seenFailed;                   // Have we failed before?
        protected TypeBuilder       m_typeBuilder;                  // TypeBuilder instance we use to create the managed type
        protected Type              m_type;                         // Created type
        protected ConverterInfo     m_info;                         // Corresponding ConverterInfo

        #endregion
    }
}