using Dovetail.SDK.Bootstrap.Http;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public class CleanUpInvalidSessions : IHttpIntervalService
	{
		private readonly DovetailDatabaseSettings _settings;
		private readonly IClarifySessionCache _cache;

		public CleanUpInvalidSessions(DovetailDatabaseSettings settings, IClarifySessionCache cache)
		{
			_settings = settings;
			_cache = cache;
		}

		public int Interval
		{
			get { return _settings.SessionCleanupInMilliseconds; }
		}

		public void Execute()
		{
			_cache.CleanUpInvalidSessions();
		}

		public void CleanUp()
		{
			// no-op
		}
	}
}
