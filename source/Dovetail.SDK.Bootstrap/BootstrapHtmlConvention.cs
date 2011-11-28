using Dovetail.SDK.Bootstrap.Clarify.Lists;
using FubuMVC.Core.UI;

namespace Dovetail.SDK.Bootstrap
{
    public class BootstrapHtmlConvention : HtmlConventionRegistry
    {
        public BootstrapHtmlConvention()
        {
            Editors.Builder<GbstListValueDropdownBuilder>();
        }
    }

}