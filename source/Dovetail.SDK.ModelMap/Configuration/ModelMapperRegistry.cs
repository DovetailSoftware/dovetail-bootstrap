using FubuMVC.StructureMap;
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
		                 scan.Convention<SettingsScanner>();
		             });

			For(typeof(IModelBuilder<>)).Use(typeof(ModelBuilder<>));
			For<IModelBuilderResultEncoder>().Use<HttpAssemblerResultEncoder>();
			For<IModelMapVisitor>().Use<DovetailGenericModelMapVisitor>();
		}
	}
}