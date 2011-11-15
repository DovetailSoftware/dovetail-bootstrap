using Bootstrap.Web.Handlers;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Spark;

namespace Bootstrap.Web
{
    public class ConfigureFubuMVC : FubuRegistry
    {
        public ConfigureFubuMVC()
        {
            // This line turns on the basic diagnostics and request tracing
            //IncludeDiagnostics(true);

            ApplyHandlerConventions<HandlerMarker>();
            
            //Output.ToJson.WhenCallMatches(a => a.OutputType().CanBeCastTo<IJsonResult>());

            this.UseSpark();

            //// All public methods from concrete classes ending in "Controller"
            //// in this assembly are assumed to be action methods
            //Actions.IncludeClassesSuffixedWithController();

            //// Policies
            //Routes
            //    .IgnoreControllerNamesEntirely()
            //    .IgnoreMethodSuffix("Html")
            //    .RootAtAssemblyNamespace();

            // Match views to action methods by matching
            // on model type, view name, and namespace
            Views.TryToAttachWithDefaultConventions();
        }
    }
}