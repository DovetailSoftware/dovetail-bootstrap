using System.Collections.Generic;

namespace Dovetail.SDK.Clarify
{
	public class Bootstrapper : IBootstrapper
	{
		private readonly IEnumerable<IStartupPolicy> _policies;
		private readonly ILogger _logger;

		public Bootstrapper(IEnumerable<IStartupPolicy> policies, ILogger logger)
		{
			_policies = policies;
			_logger = logger;
		}

		public void Bootstrap()
		{
			_policies.Each(_ =>
			{
				_logger.LogInfo("Executing startup policy: " + _.GetType().FullName);
				_.Execute();
			});
		}
	}
}