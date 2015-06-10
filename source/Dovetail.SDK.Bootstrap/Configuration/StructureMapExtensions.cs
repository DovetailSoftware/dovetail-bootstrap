using StructureMap.Configuration.DSL.Expressions;
using StructureMap.Web;

namespace Dovetail.SDK.Bootstrap.Configuration
{
	public static class StructureMapExtensions
	{
		public static CreatePluginFamilyExpression<TPlugin> TransientOrHybridHttpScoped<TPlugin>(this CreatePluginFamilyExpression<TPlugin> expression)
		{
			if (BootstrapRegistry.MaintainAspNetCompatibility)
				return expression.HybridHttpOrThreadLocalScoped();

			return expression;
		}
	}
}
