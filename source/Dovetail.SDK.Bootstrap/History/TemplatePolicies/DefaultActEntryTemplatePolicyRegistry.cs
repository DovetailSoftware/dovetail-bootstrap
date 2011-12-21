using Dovetail.SDK.Bootstrap.History.Configuration;

namespace Dovetail.SDK.Bootstrap.History.TemplatePolicies
{
    public class DefaultActEntryTemplatePolicyRegistry : ActEntryTemplatePolicyRegistry
    {
        public DefaultActEntryTemplatePolicyRegistry()
        {
            DefaultIs<WorkflowActEntryTemplatePolicy>();
            Add<SubcaseActEntryTemplatePolicy>();
            //Add<SamplePolicy>();
        }
    }
}