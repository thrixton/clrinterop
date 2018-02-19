using System;
using System.Collections.Generic;
using System.Text;
using CoreRuleEngine;

namespace TlbImpRuleEngine
{
    public class TlbImpCategoryManager : AbstractCategoryManager
    {
        public TlbImpCategoryManager()
        {
            ICategory typeCategory = TypeCategory.GetInstance();
            RegisterCategory(typeCategory.GetCategoryName(), typeCategory);
            ICategory functionCategory = FunctionCategory.GetInstance();
            RegisterCategory(functionCategory.GetCategoryName(), functionCategory);
            ICategory signatureCategory = SignatureCategory.GetInstance();
            RegisterCategory(signatureCategory.GetCategoryName(), signatureCategory);
            ICategory fieldCategory = FieldCategory.GetInstance();
            RegisterCategory(fieldCategory.GetCategoryName(), fieldCategory);
        }
    }

    public class FunctionCategory : ICategory
    {
        private const string CategoryName = "Function";

        private static FunctionCategory s_category = new FunctionCategory();

        private FunctionCategory() { }

        public static FunctionCategory GetInstance()
        {
            return s_category;
        }

        #region ICategory Members

        public string GetCategoryName()
        {
            return CategoryName;
        }

        #endregion
    }

    public class FieldCategory : ICategory
    {
        private const string CategoryName = "Field";

        private static FieldCategory s_category = new FieldCategory();

        private FieldCategory() { }

        public static FieldCategory GetInstance()
        {
            return s_category;
        }

        #region ICategory Members

        public string GetCategoryName()
        {
            return CategoryName;
        }

        #endregion
    }

    public class TypeCategory : ICategory
    {
        private const string s_categoryName = "Type";

        private static TypeCategory s_category = new TypeCategory();

        private TypeCategory() { }

        public static TypeCategory GetInstance()
        {
            return s_category;
        }

        #region ICategory Members

        public string GetCategoryName()
        {
            return s_categoryName;
        }

        #endregion
    }

    public class SignatureCategory : ICategory
    {
        private const string s_categoryName = "Signature";

        private static SignatureCategory s_category = new SignatureCategory();

        private SignatureCategory() { }

        public static SignatureCategory GetInstance()
        {
            return s_category;
        }

        #region ICategory Members

        public string GetCategoryName()
        {
            return s_categoryName;
        }

        #endregion
    }
}
