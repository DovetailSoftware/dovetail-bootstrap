using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.ModelMap;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.History
{
	public class DefaultHistoryAssembler : IDefaultHistoryAssembler
	{
		private readonly IEnumerable<IActEntryResolutionPolicy> _policies;
		private readonly IDefaultActEntryResolutionPolicy _default;
		private readonly IHistoryMapRegistry _models;
		private readonly IServiceLocator _services;
		private readonly ICurrentSDKUser _user;

		public DefaultHistoryAssembler(IEnumerable<IActEntryResolutionPolicy> policies, IDefaultActEntryResolutionPolicy @default, IHistoryMapRegistry models, IServiceLocator services, ICurrentSDKUser user)
		{
			_policies = policies;
			_default = @default;
			_models = models;
			_services = services;
			_user = user;
		}

		public bool Matches(HistoryRequest request)
		{
			return true;
		}

		public HistoryResult HistoryFor(HistoryRequest request, IHistoryBuilder builder)
		{
			var activityCodes = new List<int>();
			var gatherer = new ActEntryGatherer(activityCodes, request.ShowAllActivities, request.WorkflowObject, _services, _user);
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
				Items = request.ReverseOrder
					? items.OrderBy(_ => _.Get<DateTime>("timestamp")).ToArray()
					: items.OrderByDescending(_ => _.Get<DateTime>("timestamp")).ToArray(),
				NextTimestamp = HistoryResult.DetermineNextTimestamp(request, actEntries)
			};
		}
	}
}
