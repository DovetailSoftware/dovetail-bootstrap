using System;
using System.Collections.Generic;
using System.Linq;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace Dovetail.SDK.Bootstrap.History
{
    public static class ActEntryTemplatePolicyRegistryExtension
    {
        public static void ActEntryTemplatePolicies<TRegistry>(this Registry registry)
            where TRegistry : ActEntryTemplatePolicyRegistry, new()
        {
            registry.For<IActEntryTemplatePolicyConfiguration>().Use(ctx => new TRegistry().BuildConfiguration(ctx.GetInstance<IContainer>()));
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

    public interface IActEntryTemplatePolicyConfiguration
	{
		IEnumerable<ActEntryTemplatePolicyExpression> Policies { get; }
		ActEntryTemplatePolicyExpression DefaultPolicy { get; }
        IDictionary<int, ActEntryTemplate> RenderPolicies(WorkflowObject workflowObject);
	}

    public class ActEntryTemplatePolicyConfiguration : IActEntryTemplatePolicyConfiguration
    {
        private readonly IEnumerable<Type> _policieTypes;
        private readonly Type _defaultPolicyType;
        private readonly IContainer _container;

        public IEnumerable<ActEntryTemplatePolicyExpression> Policies
        {
            get { return _policieTypes.Select(policyType => _container.GetInstance(policyType)).Cast<ActEntryTemplatePolicyExpression>(); }
        }

        public ActEntryTemplatePolicyExpression DefaultPolicy
        {
            get { return _container.GetInstance(_defaultPolicyType) as ActEntryTemplatePolicyExpression; }
        }

        public ActEntryTemplatePolicyConfiguration(IEnumerable<Type> policieTypes, Type defaultPolicyType, IContainer container)
        {
            _policieTypes = policieTypes;
            _defaultPolicyType = defaultPolicyType;
            _container = container;
        }

        //TODO add method which gets back all the templates. 

        public IDictionary<int, ActEntryTemplate> RenderPolicies(WorkflowObject workflowObject)
        {
            var results = new Dictionary<int, ActEntryTemplate>();

            DefaultPolicy.RenderTemplate(workflowObject, results);

            Policies.Each(p => p.RenderTemplate(workflowObject, results));

            //TODO think about caching the result by workflow object.Type

            return results;
        }
    }
}