using System.Reflection;
using Dovetail.SDK.ModelMap.Registration;
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
						 //scan.AssembliesFromApplicationBaseDirectory(a => a.FullName.Contains("Dovetail.Agent."));
		                 //assemblies.Each(scan.Assembly);

		                 
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