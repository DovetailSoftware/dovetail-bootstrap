using System.Collections.Generic;

namespace Dovetail.SDK.Bootstrap.History.AssemblerPolicies
{
	public class DefaultHistoryAssemblerPolicy : IHistoryAssemblerPolicy
	{
		private readonly HistoryBuilder _historyBuilder;

		public DefaultHistoryAssemblerPolicy(HistoryBuilder historyBuilder)
		{
			_historyBuilder = historyBuilder;
		}

		public bool Handles(WorkflowObject workflowObject)
		{
			return workflowObject.Type != WorkflowObject.Case;
		}

		public IEnumerable<HistoryItem> BuildHistory(HistoryRequest request)
		{
			return _historyBuilder.Build(request);
		}
	}
}