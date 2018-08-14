using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dovetail.SDK.ModelMap;

namespace Dovetail.SDK.History
{
	public class HistoryProvider : IHistoryProvider
	{
		private readonly IDefaultHistoryAssembler _default;
		private readonly IEnumerable<IHistoryAssemblyPolicy> _policies;
		private readonly IHistoryBuilder _builder;

		public HistoryProvider(IDefaultHistoryAssembler @default, IEnumerable<IHistoryAssemblyPolicy> policies, IHistoryBuilder builder)
		{
			_default = @default;
			_policies = policies;
			_builder = builder;
		}

		public HistoryResult HistoryFor(HistoryRequest request)
		{
			var policy = _policies.LastOrDefault(_ => _.Matches(request)) ?? _default;
			var result = determineHistory(request, policy);

			return result;
		}

		private HistoryResult determineHistory(HistoryRequest request, IHistoryAssemblyPolicy policy)
		{
			var result = policy.HistoryFor(request, _builder);
			if (!result.Items.Any())
				return result;

			var localTimestamp = result.NextTimestamp;
			result.NextTimestamp = normalizeNextTimestamp(result);

			var sameTimestamp = result.NextTimestamp.HasValue &&
			                    result.Items.All(_ => _.Get<DateTime>("timestamp") == result.NextTimestamp.Value);

			if (!sameTimestamp)
				return result;


			var combinedItems = new List<ModelData>();
			combinedItems.AddRange(result.Items);

			request.Since = localTimestamp.Value;
			request.FindRepeatingTimestamp = true;

			var repeatingResult = policy.HistoryFor(request, _builder);
			foreach (var item in repeatingResult.Items)
			{
				if (combinedItems.Any(_ => _.Get<int>("id") == item.Get<int>("id")))
					continue;

				combinedItems.Add(item);
			}

			request.FindRepeatingTimestamp = false;
			request.EntryTimeExclusive = true;
			request.HistoryItemLimit = 1;

			var nextResult = policy.HistoryFor(request, _builder);
			result.NextTimestamp = normalizeNextTimestamp(nextResult);
			result.Items = combinedItems.ToArray();

			return result;
		}

		private static DateTime? normalizeNextTimestamp(HistoryResult result)
		{
			if (result.NextTimestamp.HasValue && result.NextTimestamp.Value.Kind != DateTimeKind.Utc)
			{
				var dateTime = result.NextTimestamp.Value;
				result.NextTimestamp = dateTime.ToUniversalTime();
			}

			return result.NextTimestamp;
		}
	}
}
