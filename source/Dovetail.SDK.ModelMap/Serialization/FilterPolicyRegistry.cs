using System;
using System.Reflection;
using FubuCore.Reflection;
using FubuCore.Util;

namespace Dovetail.SDK.ModelMap.Serialization
{
    public class FilterPolicyRegistry : IFilterPolicyRegistry
    {
        private static readonly Cache<string, Type> Types;

        static FilterPolicyRegistry()
        {
            Types = new Cache<string, Type>(_ => null);
            Reset();
        }

        public bool HasPolicy(string name)
        {
            return Types.Has(name.ToLower());
        }

        public Type FindPolicy(string name)
        {
            return Types[name.ToLower()];
        }

        public static void WithPolicy<TPolicy>(Action action) where TPolicy : IFilterPolicy
        {
            Types.ClearAll();
            try
            {
                fillType(typeof(TPolicy));
                action();
            }
            finally
            {
                Reset();
            }
        }

        public static void Reset()
        {
            Types.ClearAll();

            foreach (var type in TypeScanner.ConcreteImplementationsOf<IFilterPolicy>())
            {
                //if (type.HasAttribute<ExcludeFromRegistryAttribute>()) continue;
                fillType(type);
            }
        }

        private static void fillType(Type type)
        {
            var name = type.Name;
            if (type.HasAttribute<PolicyAliasAttribute>())
                name = type.GetCustomAttribute<PolicyAliasAttribute>().Alias;

            Types.Fill(name.ToLower().Replace("policy", ""), type);
        }
    }
}