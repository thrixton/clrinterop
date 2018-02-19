using System;
using System.Collections.Generic;
using System.Text;
using CoreRuleEngine;
using System.Runtime.InteropServices;

namespace TlbImpRuleEngine
{
    public class ConvertToActionDef : IActionDef
    {
        private const string ActionName = "ConvertTo";

        private ConvertToActionDef() { }

        private static ConvertToActionDef s_convertToActionDef =
            new ConvertToActionDef();

        public static ConvertToActionDef GetInstance()
        {
            return s_convertToActionDef;
        }

        public const string ParameterDirection = "Direction";
        public const string ParameterByRef = "ByRef";
        public const string ParameterManagedType = "ManagedType";
        public const string ParameterMarshalAs = "MarshalAs";
        public const string ParameterAttributes = "Attributes";

        private static readonly List<string> parameterNames = new List<string> { ParameterDirection,
            ParameterByRef, ParameterManagedType, ParameterMarshalAs, ParameterAttributes };

        private static readonly List<ICategory> possibleCategoryList = new List<ICategory>
        {
            SignatureCategory.GetInstance(),
        };

        public const string SizeConst = "SizeConst";
        public const string SizeParamIndex = "SizeParamIndex";
        public const string SizeParamIndexOffset = "SizeParamIndexOffset";

        #region IActionDef Members


        public IAction Create()
        {
            ConvertToAction convertToAction = new ConvertToAction();
            return convertToAction;
        }

        public IAction Create(Dictionary<string, string> parameters)
        {
            ConvertToAction convertToAction = new ConvertToAction();
            if (parameters.ContainsKey(ParameterDirection))
            {
                convertToAction.Direction = parameters[ParameterDirection];
            }
            else
            {
                throw new NoActionParameterException(ActionName, ParameterDirection);
            }
            if (parameters.ContainsKey(ParameterByRef))
            {
                convertToAction.ByRef = Boolean.Parse(parameters[ParameterByRef]);
            }
            else
            {
                throw new NoActionParameterException(ActionName, ParameterByRef);
            }
            if (parameters.ContainsKey(ParameterManagedType))
            {
                convertToAction.ManagedTypeConvertTo = parameters[ParameterManagedType];
            }
            else
            {
                throw new NoActionParameterException(ActionName, ParameterManagedType);
            }
            if (parameters.ContainsKey(ParameterMarshalAs))
            {
                convertToAction.UnmanagedTypeMarshalAs = parameters[ParameterMarshalAs];
            }
            else
            {
                throw new NoActionParameterException(ActionName, ParameterMarshalAs);
            }
            if (parameters.ContainsKey(ParameterAttributes))
            {
                convertToAction.Attributes = parameters[ParameterAttributes];
            }
            else
            {
                throw new NoActionParameterException(ActionName, ParameterAttributes);
            }
            return convertToAction;
        }

        public string GetActionName()
        {
            return ActionName;
        }

        public List<string> GetParameterNames()
        {
            return parameterNames;
        }

        public bool CanApplyToCategory(ICategory category)
        {
            return possibleCategoryList.Contains(category);
        }

        #endregion

    }

    public class ConvertToAction : IAction
    {
        private bool m_isInitialized;

        private string m_direction;

        private bool m_byref;

        private string m_managedTypeConvertTo;

        private string m_unmanagedTypeMarshalAs;

        private string m_attributes;

        public string Direction
        {
            get
            {
                return m_direction;
            }
            set
            {
                m_direction = value;
            }
        }

        public bool ByRef
        {
            get
            {
                return m_byref;
            }
            set
            {
                m_byref = value;
            }
        }

        public string ManagedTypeConvertTo
        {
            get
            {
                return m_managedTypeConvertTo;
            }
            set
            {
                m_managedTypeConvertTo = value;
            }
        }

        public string UnmanagedTypeMarshalAs
        {
            get
            {
                return m_unmanagedTypeMarshalAs;
            }
            set
            {
                m_unmanagedTypeMarshalAs = value;
            }
        }

        public string Attributes
        {
            get
            {
                return m_attributes;
            }
            set
            {
                m_attributes = value;
            }
        }

        public ConvertToAction()
        {
        }
        
        #region IAction Members

        public string GetParameterValue(string parameterName)
        {
            if (parameterName.Equals(ConvertToActionDef.ParameterDirection))
            {
                return m_direction;
            }
            else if (parameterName.Equals(ConvertToActionDef.ParameterByRef))
            {
                return m_byref.ToString();
            }
            else if (parameterName.Equals(ConvertToActionDef.ParameterManagedType))
            {
                return m_managedTypeConvertTo;
            }
            else if (parameterName.Equals(ConvertToActionDef.ParameterMarshalAs))
            {
                return m_unmanagedTypeMarshalAs;
            }
            else if (parameterName.Equals(ConvertToActionDef.ParameterAttributes))
            {
                return m_attributes;
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

            if (parameterName.Equals(ConvertToActionDef.ParameterDirection))
            {
                m_direction = value;
                return true;
            }
            else if (parameterName.Equals(ConvertToActionDef.ParameterByRef))
            {
                m_byref = Boolean.Parse(value);
                return true;
            }
            else if (parameterName.Equals(ConvertToActionDef.ParameterManagedType))
            {
                m_managedTypeConvertTo = value;
                return true;
            }
            else if (parameterName.Equals(ConvertToActionDef.ParameterMarshalAs))
            {
                m_unmanagedTypeMarshalAs = value;
                return true;
            }
            else if (parameterName.Equals(ConvertToActionDef.ParameterAttributes))
            {
                m_attributes = value;
                return true;
            }

            return false;
        }

        public IActionDef GetActionDef()
        {
            return ConvertToActionDef.GetInstance();
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

        public static Dictionary<string, string> GetConvertToAttributeDictionary(
            string convertToAttributes)
        {
            string attributes = convertToAttributes.Substring(1, convertToAttributes.Length - 2);
            string[] attributePairs = attributes.Split(';');
            Dictionary<string, string> attributePairDictionary = new Dictionary<string, string>();
            foreach (string attributePair in attributePairs)
            {
                string[] pair = attributePair.Split('=');
                if (pair.Length == 2)
                {
                    attributePairDictionary.Add(pair[0], pair[1]);
                }
            }
            return attributePairDictionary;
        }

    }
}
