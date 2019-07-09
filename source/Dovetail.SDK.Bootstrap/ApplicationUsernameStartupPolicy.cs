using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.Bootstrap
{
	public class ApplicationUsernameStartupPolicy : IStartupPolicy
	{
		private readonly DovetailDatabaseSettings _settings;
		private readonly ILogger _logger;

		public ApplicationUsernameStartupPolicy(DovetailDatabaseSettings settings, ILogger logger)
		{
			_settings = settings;
			_logger = logger;
		}

		public void Execute()
		{
			if (!FCApplication.IsInitialized)
			{
				_logger.LogWarn("FCApplication not initialized");
				return;
			}

			_logger.LogDebug("Setting ClarifyApplication username to: " + _settings.ApplicationUsername);
			ClarifyApplication.Instance.Username = _settings.ApplicationUsername;
		}
	}
}
