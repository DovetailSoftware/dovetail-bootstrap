using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Authentication
{
	public interface IUserAuthenticator
	{
		bool Authenticate(string username, string password);
	}

	public class UserAuthenticator : IUserAuthenticator
	{
		private readonly ILogger _logger;
		private readonly IClarifyApplication _clarifyApplication;

		public UserAuthenticator(ILogger logger, IClarifyApplication clarifyApplication)
		{
			_logger = logger;
			//HACK to make sure SDK is spun up. ICK
			_clarifyApplication = clarifyApplication;
		}

		public bool Authenticate(string username, string password)
		{
			var clarifyAuthenticationService = new ClarifyAuthenticationService();

			ClarifyAuthenticationResult result = clarifyAuthenticationService.AuthenticateUser(username, password);

			_logger.LogDebug("Authentication for user {0} was {1}successful.".ToFormat(username, result.Success ? "" : "not "));

			return result.Success;
		}
	}
}