using System;
using Dovetail.SDK.Bootstrap.Clarify;

namespace Dovetail.SDK.Bootstrap.Authentication.Principal
{
	public class NoAuthPrincipalValidator: IPrincipalValidator
	{
		private readonly DovetailDatabaseSettings _settings;

		public NoAuthPrincipalValidator(DovetailDatabaseSettings settings)
		{
			_settings = settings;
		}

		public void FailureHandler(Exception ex)
		{
			
		}

		public string UserValidator(string username)
		{
			return _settings.ApplicationUsername;
		}
	}
}