using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.History.Configuration;
using FChoice.Foundation;

namespace Dovetail.SDK.Bootstrap.History.AssemblerPolicies
{
	public class CaseHistoryAssemblerPolicy : IHistoryAssemblerPolicy
	{
		private readonly IClarifySession _session;
		private readonly HistoryBuilder _historyBuilder;
		private readonly HistorySettings _historySettings;
		private readonly ILogger _logger;

		public CaseHistoryAssemblerPolicy(IClarifySession session, HistoryBuilder historyBuilder,
			HistorySettings historySettings, ILogger logger)
		{
			_session = session;
			_historyBuilder = historyBuilder;
			_historySettings = historySettings;
			_logger = logger;
		}

		public bool Handles(WorkflowObject workflowObject)
		{
			return workflowObject.Type == WorkflowObject.Case;
		}

		public IEnumerable<HistoryItem> BuildHistory(HistoryRequest request)
		{
			if (!_historySettings.MergeCaseHistoryChildSubcases)
			{
				return _historyBuilder.Build(request);
			}

			_logger.LogDebug("Build merged history for case {0}", request.WorkflowObject.Id);

			var subcaseIds = GetSubcaseIds(request.WorkflowObject).ToArray();

			var caseHistory = _historyBuilder.Build(request);

			if (!subcaseIds.Any())
			{
				return caseHistory;
			}
			var subcaseWorkflowObject = new WorkflowObject {Type = WorkflowObject.Subcase, Id = subcaseIds.First(), IsChild = true};
			var subcaseHistoryRequest = new HistoryRequest
			{
				HistoryItemLimit = request.HistoryItemLimit,
				WorkflowObject = subcaseWorkflowObject,
				Since = request.Since,
				ShowAllActivities = request.ShowAllActivities
			};

			_logger.LogDebug("Build and merge subcase history for case {0} subcases: {1}", request.WorkflowObject.Id, subcaseIds.Join(","));

			var subcaseHistories = _historyBuilder.Build(subcaseHistoryRequest, subcaseIds);

			var results = subcaseHistories.Concat(caseHistory);

			return results.OrderByDescending(r => r.When).ThenByDescending(r => r.DatabaseIdentifier);
		}

		private IEnumerable<string> GetSubcaseIds(WorkflowObject workflowObject)
		{
			var clarifyDataSet = _session.CreateDataSet();
			var caseGeneric = clarifyDataSet.CreateGenericWithFields("case");
			caseGeneric.AppendFilter("id_number", StringOps.Equals, workflowObject.Id);

			var subcaseGeneric = caseGeneric.TraverseWithFields("case2subcase", "id_number");

			caseGeneric.Query();

			return subcaseGeneric.Count > 0 ? subcaseGeneric.DataRows().Select(s => s.AsString("id_number")) : new string[0];
		}
	}
}