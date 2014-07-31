using System;

namespace Dovetail.SDK.Bootstrap.Authentication.Principal
{
	public interface IPrincipalValidator
	{
		void FailureHandler(Exception ex);
		string UserValidator(string username);
	}
}