using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;

namespace Dovetail.SDK.Bootstrap.History.TemplatePolicies
{
    public class SamplePolicy : ActEntryTemplatePolicyExpression
    {
		public SamplePolicy(IHistoryOutputParser historyOutputParser) : base(historyOutputParser)
	    {
	    }

	    protected override void DefineTemplate(WorkflowObject workflowObject)
        {
            //you can remove templates created by other policies
            ActEntry(3000).Remove();

            //you can redefine existing policies
            ActEntry(900).DisplayName("Dys-patched")
                .EditActivityDTO(dto => { dto.Detail = "Dys-patched to the deep six. " + dto.Detail; });
        }
    }
}