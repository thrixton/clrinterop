using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRuleEngine
{
    /// <summary>
    /// A MatchTarget is an object that the Conditions try to match with.
    /// It is passed to ICondition.Match method to test if the MatchTarget satisified the Condition.
    /// </summary>
    public interface IMatchTarget
    {
        ICategory GetCategory();
    }

}
