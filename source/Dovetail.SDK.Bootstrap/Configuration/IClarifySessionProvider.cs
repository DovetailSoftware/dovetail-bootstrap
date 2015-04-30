using Dovetail.SDK.Bootstrap.Clarify;

namespace Dovetail.SDK.Bootstrap.Configuration
{
	public interface IClarifySessionProvider
	{
		IClarifySession CreateSession();
	}

	public class ClarifySessionProvider : IClarifySessionProvider
	{
		private readonly IClarifySessionCache _sessions;
		private readonly ICurrentSDKUser _sdkUser;

		public ClarifySessionProvider(IClarifySessionCache sessions, ICurrentSDKUser sdkUser)
		{
			_sessions = sessions;
			_sdkUser = sdkUser;
		}

		public IClarifySession CreateSession()
		{
			return _sessions.GetSession(_sdkUser.Username);
		}
	}
}