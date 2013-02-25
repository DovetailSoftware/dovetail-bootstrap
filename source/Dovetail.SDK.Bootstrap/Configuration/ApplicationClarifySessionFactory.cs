using Dovetail.SDK.Bootstrap.Clarify;

namespace Dovetail.SDK.Bootstrap.Configuration
{
	public interface IApplicationClarifySessionFactory
	{
		IApplicationClarifySession Create();
	}

	public class DefaultApplicationClarifySessionFactory : IApplicationClarifySessionFactory
	{
		private readonly IClarifySessionCache _sessionCache;
		private readonly UTCTimezoneUserClarifySessionConfigurator _sessionConfigurator;

		public DefaultApplicationClarifySessionFactory(IClarifySessionCache sessionCache, UTCTimezoneUserClarifySessionConfigurator sessionConfigurator)
		{
			_sessionCache = sessionCache;
			_sessionConfigurator = sessionConfigurator;
		}

		public IApplicationClarifySession Create()
		{
			var session = (ClarifySessionWrapper) _sessionCache.GetApplicationSession();

			//_sessionConfigurator.Configure(session.ClarifySession);

			return session;
		}
	}
}