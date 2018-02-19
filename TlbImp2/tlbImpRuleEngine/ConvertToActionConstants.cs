using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TlbImpRuleEngine
{
    public enum ParameterDirection
    {
        IN = 0,
        OUT,
        INOUT,
        NONE,
    }

    public enum ManagedTypeConvertTo
    {
        LPARRAY = 0,
        INT,
        STRING,
        STRINGBUILDER,
        DECIMAL,
        OBJECT,
        NONE,
    }

    public class ConvertToActionConstants
    {
        public const string ParameterDirectionIn = "[In]";

        public const string ParameterDirectionOut = "[Out]";

        public const string ParameterDirectionInOut = "[In, Out]";

        public static ParameterDirection GetParameterDirection(string directionStr)
        {
            if (directionStr.Equals(ParameterDirectionIn))
            {
                return ParameterDirection.IN;
            }
            else if (directionStr.Equals(ParameterDirectionOut))
            {
                return ParameterDirection.OUT;
            }
            else if (directionStr.Equals(ParameterDirectionInOut))
            {
                return ParameterDirection.INOUT;
            }
            return ParameterDirection.NONE;
        }

        public static readonly string[] ManagedTypes = {"LPArray", "int", "string", "StringBuilder",
            "Decimal", "object"};

        private static Dictionary<string, List<string>> s_typeConvertToMapping;

        public static List<string> GetMarshalAsTypes(string managedType)
        {
            if (s_typeConvertToMapping == null)
            {
                s_typeConvertToMapping = new Dictionary<string, List<string>>();
                List<string> temp = new List<string>();
                temp.Add(UnmanagedType.Error.ToString());
                s_typeConvertToMapping.Add("int", temp);
                temp = new List<string>();
                temp.Add(UnmanagedType.LPStr.ToString());
                temp.Add(UnmanagedType.LPWStr.ToString());
                s_typeConvertToMapping.Add("string",temp);
                s_typeConvertToMapping.Add("StringBuilder", temp);
                temp = new List<string>();
                temp.Add(UnmanagedType.Currency.ToString());
                s_typeConvertToMapping.Add("Decimal", temp);
                temp = new List<string>();
                temp.Add(UnmanagedType.IUnknown.ToString());
                temp.Add(UnmanagedType.IDispatch.ToString());
                s_typeConvertToMapping.Add("object", temp);
            }
            if (s_typeConvertToMapping.ContainsKey(managedType))
            {
                return s_typeConvertToMapping[managedType];
            }
            else
            {
                return null;
            }
        }

        public static ManagedTypeConvertTo GetManagedTypeConvertTo(string typeStr)
        {
            int i;
            for (i = 0; i < ManagedTypes.Length; i++)
            {
                if (ManagedTypes[i].Equals(typeStr))
                    break;
            }
            if (i == ManagedTypes.Length)
                return ManagedTypeConvertTo.NONE;
            else
                return (ManagedTypeConvertTo)i;
        }

        public static string GetTypeConvertedToString(ManagedTypeConvertTo typeConvertedTo)
        {
            if (typeConvertedTo != ManagedTypeConvertTo.NONE)
                return ManagedTypes[(int)typeConvertedTo];
            else
                return null;
        }

        public static UnmanagedType GetMarshalAs(string typeString)
        {
            if (UnmanagedType.Error.ToString().Equals(typeString))
            {
                return UnmanagedType.Error;
            }
            else if (UnmanagedType.LPStr.ToString().Equals(typeString))
            {
                return UnmanagedType.LPStr;
            }
            else if (UnmanagedType.LPWStr.ToString().Equals(typeString))
            {
                return UnmanagedType.LPWStr;
            }
            else if (UnmanagedType.Currency.ToString().Equals(typeString))
            {
                return UnmanagedType.Currency;
            }
            else if (UnmanagedType.IUnknown.ToString().Equals(typeString))
            {
                return UnmanagedType.IUnknown;
            }
            else if (UnmanagedType.IDispatch.ToString().Equals(typeString))
            {
                return UnmanagedType.IDispatch;
            }
            else
            {
                return (UnmanagedType)(-1);
            }
        }
    }

}
