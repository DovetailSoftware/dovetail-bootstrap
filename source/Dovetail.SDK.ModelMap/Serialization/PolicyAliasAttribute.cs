using System;

namespace Dovetail.SDK.ModelMap.Serialization
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PolicyAliasAttribute : Attribute
    {
        public string Alias { get; set; }

        public PolicyAliasAttribute(string alias)
        {
            Alias = alias;
        }
    }
}