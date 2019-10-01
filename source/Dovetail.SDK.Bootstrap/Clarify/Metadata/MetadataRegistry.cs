using StructureMap.Configuration.DSL;
using StructureMap.Graph;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public class MetadataRegistry : Registry
	{
		public MetadataRegistry()
		{
			Scan(_ =>
			{
				_.TheCallingAssembly();
				_.AddAllTypesOf<IXElementVisitor>();
			});

			ForSingletonOf<ISchemaMetadataCache>().Use<SchemaMetadataCache>();
		}
	}
}
