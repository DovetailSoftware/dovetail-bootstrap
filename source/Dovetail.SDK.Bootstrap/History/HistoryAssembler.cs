using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.History.AssemblerPolicies;

namespace Dovetail.SDK.Bootstrap.History
{
	public interface IHistoryAssembler
	{
		HistoryViewModel GetHistory(HistoryRequest request);
	}

	public class HistoryAssembler : IHistoryAssembler
	{
		private readonly IEnumerable<IHistoryAssemblerPolicy> _entityHistoryBuilders;

		public HistoryAssembler(IEnumerable<IHistoryAssemblerPolicy> entityHistoryBuilders)
		{
			_entityHistoryBuilders = entityHistoryBuilders;
		}

		public HistoryViewModel GetHistory(HistoryRequest request)
		{
			return getHistoryWithConstraint(request);
		}

		private HistoryViewModel getHistoryWithConstraint(HistoryRequest request)
		{
			var historyItems = getHistoryItems(request);

			return CreateHistoryModel(request, historyItems);
		}

		private IEnumerable<HistoryItem> getHistoryItems(HistoryRequest request)
		{
			var historyBuilderPolicy = _entityHistoryBuilders.First(policy => policy.Handles(request.WorkflowObject));

			return historyBuilderPolicy.BuildHistory(request);
		}

		private static HistoryViewModel CreateHistoryModel(HistoryRequest request, IEnumerable<HistoryItem> historyItems)
		{
			return new HistoryViewModel {WorkflowObject = request.WorkflowObject, AllActivitiesShown = request.ShowAllActivities, HistoryItems = historyItems.ToArray()};
		}
	}
}