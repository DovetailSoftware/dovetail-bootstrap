using Bootstrap.Web.Handlers;
using Bootstrap.Web.Handlers.error.http500;
using Bootstrap.Web.Handlers.home;
using Bootstrap.Web.Security;
using Dovetail.SDK.Fubu.Clarify.Lists;
using Dovetail.SDK.Fubu.TokenAuthentication.Token;
using FubuMVC.Core;
using FubuMVC.Core.Registration.Conventions;
using FubuMVC.Core.Security;
using FubuMVC.Swagger;

namespace Bootstrap.Web
{
    public class ConfigureFubuMVC : FubuRegistry
    {
        public ConfigureFubuMVC()
        {
            Import<HandlerConvention>(x => x.MarkerType<HandlerMarker>());
            
			// Match views to action methods by matching
            // on model type, view name, and namespace
            Views.TryToAttachWithDefaultConventions();

            Routes.HomeIs<HomeRequest>();

            ApplyConvention<AuthenticationTokenConvention>();
            
            //convention to transfer exceptions to the view for an input model given via generic argument
            ApplyConvention<APIExceptionConvention<Error500Request>>();

			Import<BootstrapHtmlConvention>();

            //TODO replace this with Swagger Bottle
            ApplyConvention<SwaggerConvention>();

            Services(s=>
                         {
                             s.ReplaceService<IAuthorizationFailureHandler, BootstrapAuthorizationFailureHandler>();

                             //TODO replace this with Swagger Bottle
                             s.AddService<IActionGrouper, APIRouteGrouper>();
                         });
        }
    }
}