using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.ModelMap;

namespace Dovetail.SDK.History
{
	public class DefaultHistoryAssembler : IDefaultHistoryAssembler
	{
		private readonly IEnumerable<IActEntryResolutionPolicy> _policies;
		private readonly IDefaultActEntryResolutionPolicy _default;
		private readonly IHistoryMapRegistry _models;
		private readonly HistorySettings _settings;

		public DefaultHistoryAssembler(IEnumerable<IActEntryResolutionPolicy> policies, IDefaultActEntryResolutionPolicy @default, IHistoryMapRegistry models, HistorySettings settings)
		{
			_policies = policies;
			_default = @default;
			_models = models;
			_settings = settings;
		}

		public bool Matches(HistoryRequest request)
		{
			return true;
		}

		public HistoryResult HistoryFor(HistoryRequest request, IHistoryBuilder builder)
		{
			var activityCodes = new List<int>();
			var gatherer = new ActEntryGatherer(activityCodes, request.ShowAllActivities, _settings, request.WorkflowObject);
			var map = _models.Find(request.WorkflowObject);
			map.Accept(gatherer);

			var policy = _policies.LastOrDefault(_ => _.Matches(request)) ?? _default;
			var actEntries = policy.IdsFor(request, activityCodes.ToArray());

			var items = actEntries.Ids.Any()
				? builder.GetAll(request, generic => generic.Filter(_ => _.IsIn("objid", actEntries.Ids.ToArray())))
				: new ModelData[0];

			return new HistoryResult
			{
				HistoryItemLimit = request.HistoryItemLimit,
				Since = request.Since,
				TotalResults = actEntries.Count,
				Items = items
			};
		}
	}
}
