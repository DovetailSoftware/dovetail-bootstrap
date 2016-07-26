using System;
using System.Linq;
using System.Security.Authentication;
using System.Web;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Authentication.Principal
{
	public class WindowsAuthenticationPrincipalValidator : IPrincipalValidator
	{
		private readonly ILogger _logger;
		private readonly IApplicationClarifySession _session;

		public WindowsAuthenticationPrincipalValidator(ILogger logger,
			IApplicationClarifySession session)
		{
			_logger = logger;
			_session = session;
		}

		public void FailureHandler(Exception ex)
		{
			_logger.LogError("Windows authentication failed. Returning empty 401.1 response.", ex);

			//output response code 401.1 and end response.
			var context = HttpContext.Current;
			if (context != null)
			{
				context.Response.StatusCode = 401;
				context.Response.SubStatusCode = 1;
				context.Response.End();
			}
			else
			{
				_logger.LogDebug("Current HttpContext is null so it looks like we are not in a web request. Doing nothing.");
			}
		}

		public string UserValidator(string username)
		{
			var dataset = _session.CreateDataSet();
			var emplViewGeneric = dataset.CreateGenericWithFields("empl_user", "login_name");
			emplViewGeneric.Filter(f => f.And(f.Equals("status", 1), f.Equals("windows_login", username)));
			emplViewGeneric.Query();

			if (emplViewGeneric.Count < 1)
			{
				throw new AuthenticationException("There is no employee whose Windows login name maps to {0}.".ToFormat(username));
			}

			if (emplViewGeneric.Count > 1)
			{
				var names = emplViewGeneric.DataRows().Select(r => r.AsString("login_name")).ToArray();
				_logger.LogError("There is more than one employee whose Windows login name maps to the user {0}. Using the first one of : {1}".ToFormat(username, String.Join(",", names)));
			}

			var clarifyUserName = emplViewGeneric.Rows[0].AsString("login_name");
			_logger.LogDebug("Windows user {0} mapped to Clarify user {1}.", username, clarifyUserName);
			return clarifyUserName;
		}
	}
}
