using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.ModelMap.Legacy;
using Dovetail.SDK.ModelMap.Serialization;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using DovetailGenericModelMapVisitor = Dovetail.SDK.ModelMap.Legacy.DovetailGenericModelMapVisitor;
using IModelMapVisitor = Dovetail.SDK.ModelMap.Legacy.IModelMapVisitor;

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

		        scan.AddAllTypesOf<IElementVisitor>();
		        scan.AddAllTypesOf<IMappingVariableSource>();
		    });

			For(typeof(IModelBuilder<>)).Use(typeof(ModelBuilder<>));
			For<IOutputEncoder>().Use<HtmlEncodeOutputEncoder>();
			For<IModelMapVisitor>().Use<DovetailGenericModelMapVisitor>();

		    ForSingletonOf<IModelMapCache>().Use<ModelMapCache>();
		}
	}
}