using System;
using System.Collections.Generic;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace Dovetail.SDK.Bootstrap.History.Configuration
{
    public static class ActEntryTemplatePolicyRegistryExtension
    {
        public static void ActEntryTemplatePolicies<TRegistry>(this Registry registry)
            where TRegistry : ActEntryTemplatePolicyRegistry, new()
        {
            registry.For<IActEntryTemplatePolicyConfiguration>().Singleton().Use(ctx => new TRegistry().BuildConfiguration(ctx.GetInstance<IContainer>()));
        }
    }

    public class ActEntryTemplatePolicyRegistry
    {
        private readonly IList<Type> _policies = new List<Type>();
        private Type _defaultPolicy;

        public ActEntryTemplatePolicyRegistry Add<TPolicy>() where TPolicy : ActEntryTemplatePolicyExpression
        {
            _policies.Fill(typeof(TPolicy));

            return this;
        }

        public ActEntryTemplatePolicyRegistry Add(ActEntryTemplatePolicyExpression policy)
        {
            _policies.Fill(policy.GetType());

            return this;
        }

        public ActEntryTemplatePolicyRegistry DefaultIs<TPolicy>() where TPolicy : ActEntryTemplatePolicyExpression
        {
            _defaultPolicy = typeof(TPolicy);

            return this;
        }

        public ActEntryTemplatePolicyRegistry DefaultIs(ActEntryTemplatePolicyExpression policy)
        {
            _defaultPolicy = policy.GetType();

            return this;
        }

        public IActEntryTemplatePolicyConfiguration BuildConfiguration(IContainer container)
        {
            return new ActEntryTemplatePolicyConfiguration(_policies, _defaultPolicy, container);
        }
    }
}