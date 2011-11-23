namespace Dovetail.SDK.Bootstrap.Clarify
{
    public interface IClarifySessionProvider
    {
        IClarifySession GetHttpRequestSession();
    }

    public class ClarifySessionProvider : IClarifySessionProvider
    {
        private readonly IClarifySessionCache _sessionCache;
        private readonly ICurrentSDKUser _user;
		private readonly DovetailDatabaseSettings _settings;

		public ClarifySessionProvider(IClarifySessionCache sessionCache, ICurrentSDKUser user, DovetailDatabaseSettings settings)
        {
            _sessionCache = sessionCache;
            _user = user;
        	_settings = settings;
        }

        public IClarifySession GetHttpRequestSession()
        {
        	var username = _user.Username ?? _settings.ApplicationUsername;

            return  _sessionCache.GetUserSession(username);
        }
    }
}