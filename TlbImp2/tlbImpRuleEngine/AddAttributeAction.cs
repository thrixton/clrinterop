using System;
using System.Collections.Generic;
using System.Text;
using CoreRuleEngine;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Globalization;

namespace TlbImpRuleEngine
{
    public class AddAttributeActionDef : IActionDef
    {
        private const string ActionName = "AddAttribute";

        private AddAttributeActionDef() { }

        private static AddAttributeActionDef s_addAttributeActionDef =
            new AddAttributeActionDef();

        public static AddAttributeActionDef GetInstance()
        {
            return s_addAttributeActionDef;
        }

        public const string ParameterAssemblyName = "AssemblyName";

        public const string ParameterTypeName = "TypeName";

        public const string ParameterConstructor = "Constructor";

        public const string ParameterData = "Data";

        private static readonly List<string> s_parameterNames = new List<string> {
            ParameterAssemblyName, ParameterTypeName, ParameterConstructor, ParameterData
        };

        private static readonly List<ICategory> s_possibleCategoryList = new List<ICategory>
        {
            TypeCategory.GetInstance(),
        };

        #region IActionCreator Members

        public IAction Create()
        {
            AddAttributeAction addAttributeAction = new AddAttributeAction();
            return addAttributeAction;
        }

        public IAction Create(Dictionary<string, string> parameters)
        {
            AddAttributeAction addAttributeAction = new AddAttributeAction();
            if (parameters.ContainsKey(ParameterAssemblyName))
            {
                addAttributeAction.AssemblyName = parameters[ParameterAssemblyName];
            }
            else
            {
                throw new NoActionParameterException(ActionName, ParameterAssemblyName);
            }
            if (parameters.ContainsKey(ParameterTypeName))
            {
                addAttributeAction.TypeName = parameters[ParameterTypeName];
            }
            else
            {
                throw new NoActionParameterException(ActionName, ParameterTypeName);
            }
            if (parameters.ContainsKey(ParameterConstructor))
            {
                addAttributeAction.Constructor = parameters[ParameterConstructor];
            }
            else
            {
                throw new NoActionParameterException(ActionName, ParameterConstructor);
            }
            if (parameters.ContainsKey(ParameterData))
            {
                addAttributeAction.Data = parameters[ParameterData];
            }
            else
            {
                throw new NoActionParameterException(ActionName, ParameterData);
            }
            return addAttributeAction;
        }

        public string GetActionName()
        {
            return ActionName;
        }

        public List<string> GetParameterNames()
        {
            return s_parameterNames;
        }

        public bool CanApplyToCategory(ICategory category)
        {
            return s_possibleCategoryList.Contains(category);
        }

        #endregion
    }

    public class AddAttributeAction : IAction
    {
        private bool m_isInitialized;

        private string m_assemblyName;
        private string m_typeName;
        private string m_constructor;
        private string m_data;

        public AddAttributeAction()
        {
        }

        public string AssemblyName
        {
            get
            {
                return m_assemblyName;
            }
            set
            {
                m_assemblyName = value;
            }
        }

        public string TypeName
        {
            get
            {
                return m_typeName;
            }
            set
            {
                m_typeName = value;
            }
        }

        public string Constructor
        {
            get
            {
                return m_constructor;
            }
            set
            {
                m_constructor = value;
            }
        }

        public string Data
        {
            get
            {
                return m_data;
            }
            set
            {
                m_data = value;
            }
        }
        
        #region IAction Members

        public string GetParameterValue(string parameterName)
        {
            if (parameterName.Equals(AddAttributeActionDef.ParameterAssemblyName))
            {
                return m_assemblyName;
            }
            else if (parameterName.Equals(AddAttributeActionDef.ParameterTypeName))
            {
                return m_typeName;
            }
            else if (parameterName.Equals(AddAttributeActionDef.ParameterConstructor))
            {
                return m_constructor;
            }
            else if (parameterName.Equals(AddAttributeActionDef.ParameterData))
            {
                return m_data;
            }
            else
            {
                return null;
            }
        }

        public bool SetParameterValue(string parameterName, string value)
        {
            if (value == null)
                return false;

            if (parameterName.Equals(AddAttributeActionDef.ParameterAssemblyName))
            {
                m_assemblyName = value;
                return true;
            }
            else if (parameterName.Equals(AddAttributeActionDef.ParameterTypeName))
            {
                m_typeName = value;
                return true;
            }
            else if (parameterName.Equals(AddAttributeActionDef.ParameterConstructor))
            {
                m_constructor = value;
                return true;
            }
            else if (parameterName.Equals(AddAttributeActionDef.ParameterData))
            {
                m_data = value;
                return true;
            }
            
            return false;
        }

        public IActionDef GetActionDef()
        {
            return AddAttributeActionDef.GetInstance();
        }

        public bool IsInitialized
        {
            get
            {
                return m_isInitialized;
            }
            set
            {
                m_isInitialized = value;
            }
        }

        #endregion


        public bool GetCustomAttribute(out System.Reflection.ConstructorInfo attributeCtor, out byte[] blob)
        {
            attributeCtor = null;
            blob = null;
            try
            {
                // Get ctor
                Assembly assembly = Assembly.Load(m_assemblyName);
                Type type = assembly.GetType(m_typeName);
                ConstructorInfo[] ctors = type.GetConstructors();
                foreach (ConstructorInfo ctor in ctors)
                {
                    if (ctor.ToString().Equals(m_constructor))
                    {
                        attributeCtor = ctor;
                        break;
                    }
                }
                if (attributeCtor == null)
                    return false;
                return GetBlobByString(m_data, out blob);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool GetBlobByString(string data, out byte[] blob)
        {
            blob = null;
            try
            {
                // Get byte[]
                string[] byteStrings = data.Split(new char[] { ' ' });
                blob = new byte[byteStrings.Length];
                for (int i = 0; i < byteStrings.Length; i++)
                {
                    blob[i] = Byte.Parse(byteStrings[i], NumberStyles.AllowHexSpecifier,
                        CultureInfo.InvariantCulture);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static string GetStringByBlob(byte[] blob)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < blob.Length; i++)
            {
                if (sb.Length != 0)
                    sb.Append(' ');
                sb.Append(blob[i].ToString("X2", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }
    }
}
