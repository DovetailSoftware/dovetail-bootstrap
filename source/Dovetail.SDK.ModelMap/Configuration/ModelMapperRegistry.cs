using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.ModelMap.NextGen;
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
			For<IModelBuilderResultEncoder>().Use<HttpAssemblerResultEncoder>();
			For<IModelMapVisitor>().Use<DovetailGenericModelMapVisitor>();

			For(typeof(IModelMapConfigFactory<,>)).Use(typeof(ModelMapConfigFactory<,>));
			For(typeof(IMapQueryFactory<,>)).Use(typeof(MapQueryFactory<,>));
			For(typeof(IModelBuilder<,>)).Use(typeof(ModelBuilder<,>));
		}
	}
}