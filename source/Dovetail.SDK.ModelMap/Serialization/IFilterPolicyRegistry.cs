using System;

namespace Dovetail.SDK.ModelMap.Serialization
{
    public interface IFilterPolicyRegistry
    {
        bool HasPolicy(string name);
        Type FindPolicy(string name);
    }
}