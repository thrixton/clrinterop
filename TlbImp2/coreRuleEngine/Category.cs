using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRuleEngine
{
    /// <summary>
    /// The manager of all Categories.
    /// All Categories are registered in this manager. CategoryManager managers them using the category
    /// name. So, we can get the Category instance by its name.
    /// </summary>
    public abstract class AbstractCategoryManager
    {
        private Dictionary<string, ICategory> m_registeredCategory;

        public AbstractCategoryManager()
        {
            m_registeredCategory = new Dictionary<string, ICategory>();
        }

        protected void RegisterCategory(string categoryName, ICategory category)
        {
            m_registeredCategory.Add(categoryName, category);
        }

        public ICategory GetCategory(string categoryString)
        {
            if (m_registeredCategory.ContainsKey(categoryString))
            {
                return m_registeredCategory[categoryString];
            }
            return null;
        }

        public List<ICategory> GetAllCategory()
        {
            List<ICategory> categoryList = new List<ICategory>();
            Dictionary<string, ICategory>.Enumerator enumerator =
                m_registeredCategory.GetEnumerator();
            while (enumerator.MoveNext())
            {
                categoryList.Add(enumerator.Current.Value);
            }
            return categoryList;
        }
    }

    /// <summary>
    /// Category is the classification of MatchTarget.
    /// A category's name property is used as identification.
    /// </summary>
    public interface ICategory
    {
        string GetCategoryName();
    }

}
