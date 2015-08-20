using System;
using Bootstrap.Web.Handlers;
using Bootstrap.Web.Handlers.error.http500;
using Bootstrap.Web.Security;
using Dovetail.SDK.Fubu.Clarify.Lists;
using Dovetail.SDK.Fubu.TokenAuthentication.Token;
using FubuCore;
using FubuLocalization;
using FubuMVC.Core;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Security;
using FubuMVC.Localization;
//using FubuMVC.Swagger;
//using FubuMVC.Swagger.Configuration;
//using IApi = Dovetail.SDK.Bootstrap.IApi;

namespace Bootstrap.Web
{
	public class ConfigureFubuMVC : FubuRegistry
	{
		public ConfigureFubuMVC()
		{
			//Import<HandlerConvention>(x => x.MarkerType<HandlerMarker>());

			Policies.Global.Add<AuthenticationTokenConvention>();

			//convention to transfer exceptions to the view for an input model given via generic argument
			Policies.Global.Add<APIExceptionConvention<Error500Request>>();

			Import<BootstrapHtmlConvention>();

			//TODO replace this with Swagger Bottle
			//Policies.Global.Add<SwaggerConvention>();

			Services(s =>
			{
				s.ReplaceService<IAuthorizationFailureHandler, BootstrapAuthorizationFailureHandler>();

//				s.AddService<IActionGrouper, APIRouteGrouper>();
//				s.AddService<IActionFinder, BootstrapActionFinder>();
				s.AddService<ICurrentCultureContext, CurrentCultureContext>();
			});

			Import<BasicLocalizationSupport>();
			Policies.Global.Add(x => x.Wrap.WithBehavior<BasicLocalizationBehavior>());
		}
	}

	//public class BootstrapActionFinder : IActionFinder
	//{
	//	public Func<ActionCall, bool> Matches
	//	{
	//		get { return IsApiCall; }
	//	}

	//	private static bool IsApiCall(ActionCall actionCall)
	//	{
	//		return actionCall.ParentChain().InputType().CanBeCastTo<IApi>();
	//	}
	//}
}
