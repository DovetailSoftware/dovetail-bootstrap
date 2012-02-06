using Bootstrap.Web.Handlers;
using Bootstrap.Web.Handlers.home;
using Bootstrap.Web.Security;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Fubu.Authentication.Token;
using Dovetail.SDK.Fubu.Clarify.Lists;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Security;
using FubuMVC.Spark;

namespace Bootstrap.Web
{
    public class ConfigureFubuMVC : FubuRegistry
    {
        public ConfigureFubuMVC()
        {
#if DEBUG
            this.IncludeDiagnostics(true);
#endif
            ApplyHandlerConventions<HandlerMarker>();
            
            this.UseSpark();

            // Match views to action methods by matching
            // on model type, view name, and namespace
            Views.TryToAttachWithDefaultConventions();

            Routes.HomeIs<HomeRequest>();

            Media.ApplyContentNegotiationTo(x => x.InputType().CanBeCastTo<IApi>() || x.InputType().CanBeCastTo<IUnauthenticatedApi>());

            ApplyConvention<AuthenticationTokenConvention>();

            HtmlConvention<BootstrapHtmlConvention>();

            Services(s=>s.ReplaceService<IAuthorizationFailureHandler, BootstrapAuthorizationFailureHandler>());
        }
    }
}