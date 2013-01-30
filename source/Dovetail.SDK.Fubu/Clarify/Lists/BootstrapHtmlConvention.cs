using FubuMVC.Core.UI;

namespace Dovetail.SDK.Fubu.Clarify.Lists
{
    public class BootstrapHtmlConvention : HtmlConventionRegistry
    {
        public BootstrapHtmlConvention()
        {
            Editors.BuilderPolicy<GbstListValueDropdownBuilder>();
        }
    }
}