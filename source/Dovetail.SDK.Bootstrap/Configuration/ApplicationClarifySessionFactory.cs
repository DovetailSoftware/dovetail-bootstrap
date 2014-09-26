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

		public DefaultApplicationClarifySessionFactory(IClarifySessionCache sessionCache)
		{
			_sessionCache = sessionCache;
		}

		public IApplicationClarifySession Create()
		{
			var session = (ClarifySessionWrapper) _sessionCache.GetApplicationSession();

			return session;
		}
	}
}