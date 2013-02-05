using Dovetail.SDK.Bootstrap.Configuration;
using StructureMap.Configuration.DSL;

namespace Dovetail.SDK.ModelMap.Configuration
{
	public class ModelMapperRegistry : Registry
	{
		public ModelMapperRegistry()
		{
		    Scan(scan =>
		             {
                         scan.TheCallingAssembly();
                         scan.WithDefaultConventions();
		                 
                         //TODO move settings configuration into its own registry
                         scan.Convention<SettingsScanner>();
		             });

			For(typeof(IModelBuilder<>)).Use(typeof(ModelBuilder<>));
			For<IOutputEncoder>().Use<HtmlEncodeOutputEncoder>();
			For<IModelMapVisitor>().Use<DovetailGenericModelMapVisitor>();
		}
	}
}