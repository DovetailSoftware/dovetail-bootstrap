using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Util;
using StructureMap;

namespace Dovetail.SDK.Bootstrap.History.Configuration
{
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
        private readonly Cache<WorkflowObject, IDictionary<int, ActEntryTemplate>> _resultCache = new Cache<WorkflowObject, IDictionary<int, ActEntryTemplate>>();

        public ActEntryTemplatePolicyConfiguration(IEnumerable<Type> policieTypes, Type defaultPolicyType, IContainer container)
        {
            _policieTypes = policieTypes;
            _defaultPolicyType = defaultPolicyType;
            _container = container;
            _resultCache.OnMissing = buildResultsFor;
        }

        private IDictionary<int, ActEntryTemplate> buildResultsFor(WorkflowObject workflowObject)
        {
            var results = new Dictionary<int, ActEntryTemplate>();

            DefaultPolicy.RenderTemplate(workflowObject, results);

            Policies.Each(p => p.RenderTemplate(workflowObject, results));

            return results;
        }

        public IEnumerable<ActEntryTemplatePolicyExpression> Policies
        {
            get { return _policieTypes.Select(policyType => _container.GetInstance(policyType)).Cast<ActEntryTemplatePolicyExpression>(); }
        }

        public ActEntryTemplatePolicyExpression DefaultPolicy
        {
            get { return _container.GetInstance(_defaultPolicyType) as ActEntryTemplatePolicyExpression; }
        }

        public IDictionary<int, ActEntryTemplate> RenderPolicies(WorkflowObject workflowObject)
        {
            return _resultCache[workflowObject];
        }
    }
}