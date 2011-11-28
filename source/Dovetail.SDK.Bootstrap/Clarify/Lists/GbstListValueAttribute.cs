using System;

namespace Dovetail.SDK.Bootstrap.Clarify.Lists
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class GbstListValueAttribute : Attribute
    {
        private readonly string _listName;

        public GbstListValueAttribute(string listName)
        {
            _listName = listName;
        }

        public virtual string GetListName()
        {
            return _listName;
        }
    }
}