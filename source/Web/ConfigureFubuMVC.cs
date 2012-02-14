using Bootstrap.Web.Handlers;
using Bootstrap.Web.Handlers.home;
using Bootstrap.Web.Security;
using Dovetail.SDK.Fubu.Authentication.Token;
using Dovetail.SDK.Fubu.Clarify.Lists;
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

            ApplyConvention<SwaggerConvention<IApi>>();

            Services(s=> s.ReplaceService<IAuthorizationFailureHandler, BootstrapAuthorizationFailureHandler>());
        }
    }
}