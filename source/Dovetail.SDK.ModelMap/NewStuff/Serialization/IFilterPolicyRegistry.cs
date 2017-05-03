using System;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization
{
    public interface IFilterPolicyRegistry
    {
        bool HasPolicy(string name);
        Type FindPolicy(string name);
    }
}