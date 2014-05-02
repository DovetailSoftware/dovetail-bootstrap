using System;
using System.Collections.Generic;
using System.Linq;
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
	    public const int DefaultActEntryTemplateMagicCode = -999;

        public ActEntryTemplatePolicyConfiguration(IEnumerable<Type> policieTypes, Type defaultPolicyType, IContainer container)
        {
            _policieTypes = policieTypes;
            _defaultPolicyType = defaultPolicyType;
            _container = container;
        }

        private IDictionary<int, ActEntryTemplate> buildTemplatesFor(WorkflowObject workflowObject)
        {
            var results = new Dictionary<int, ActEntryTemplate>();

			//Setup default template to be used when act_entries with no template are found in the results
			//TODO allow better customization of how the default template is defined
	        var defaultTemplate = _container.GetInstance<ActEntryTemplate>();
	        defaultTemplate.Code = DefaultActEntryTemplateMagicCode;
			results.Add(DefaultActEntryTemplateMagicCode, defaultTemplate);

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
	        var templates = buildTemplatesFor(workflowObject);

	        return templates;
        }
    }
}