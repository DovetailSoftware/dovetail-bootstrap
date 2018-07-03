using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;

namespace Dovetail.SDK.History
{
	public interface IDefaultHistoryAssembler : IHistoryAssemblyPolicy
	{
	}

	public class DefaultHistoryAssembler : IDefaultHistoryAssembler
	{
		private readonly IEnumerable<IActEntryResolutionPolicy> _policies;
		private readonly IDefaultActEntryResolutionPolicy _default;
		private readonly IHistoryMapRegistry _models;

		public DefaultHistoryAssembler(IEnumerable<IActEntryResolutionPolicy> policies, IDefaultActEntryResolutionPolicy @default, IHistoryMapRegistry models)
		{
			_policies = policies;
			_default = @default;
			_models = models;
		}

		public bool Matches(HistoryRequest request)
		{
			return true;
		}

		public HistoryResult HistoryFor(HistoryRequest request, IHistoryBuilder builder)
		{
			var activityCodes = new List<int>();
			var gatherer = new ActEntryGatherer(activityCodes, request.ShowAllActivities);
			var map = _models.Find(request.WorkflowObject);
			map.Accept(gatherer);

			var policy = _policies.LastOrDefault(_ => _.Matches(request)) ?? _default;
			var actEntries = policy.IdsFor(request, activityCodes.ToArray());

			return new HistoryResult
			{
				HistoryItemLimit = request.HistoryItemLimit,
				Since = request.Since,
				TotalResults = actEntries.Count,
				Items = builder.GetAll(request, generic => generic.Filter(_ => _.IsIn("objid", actEntries.Ids.ToArray())))
			};
		}
	}
}