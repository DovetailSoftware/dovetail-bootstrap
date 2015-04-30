using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Fubu.TokenAuthentication.Token.Extensions;
using FubuMVC.Core.Registration;

namespace Dovetail.SDK.Fubu.TokenAuthentication.Token
{
	public class AuthenticationTokenConvention : IConfigurationAction
	{
		public void Configure(BehaviorGraph graph)
		{
			graph
				.Actions()
				.Where(action => action.InputType().IsAuthenticatedAPIRequest())
				.Each(action => action.ParentChain().Authorization.AddPolicy(new AuthenticationTokenAuthorizationPolicy()));
		}
	}
}