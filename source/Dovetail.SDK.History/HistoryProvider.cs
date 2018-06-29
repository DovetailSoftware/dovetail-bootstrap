using System.Collections.Generic;
using System.Linq;

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
			return policy.HistoryFor(request, _builder);
		}
	}
}