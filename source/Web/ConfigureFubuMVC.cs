using Bootstrap.Web.Handlers;
using Bootstrap.Web.Handlers.home;
using Bootstrap.Web.Security;
using Dovetail.SDK.Fubu.Clarify.Lists;
using Dovetail.SDK.Fubu.TokenAuthentication.Token;
using FubuMVC.Core;
using FubuMVC.Core.Security;
using FubuMVC.Spark;
using FubuMVC.Swagger;

namespace Bootstrap.Web
{
    public class ConfigureFubuMVC : FubuRegistry
    {
        public ConfigureFubuMVC()
        {
#if DEBUG
            IncludeDiagnostics(true);
#endif
            ApplyHandlerConventions<HandlerMarker>();
            
            this.UseSpark();

            // Match views to action methods by matching
            // on model type, view name, and namespace
            Views.TryToAttachWithDefaultConventions();

            Routes.HomeIs<HomeRequest>();

            ApplyConvention<AuthenticationTokenConvention>();

            HtmlConvention<BootstrapHtmlConvention>();

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