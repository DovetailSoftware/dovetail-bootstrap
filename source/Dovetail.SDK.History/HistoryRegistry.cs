using Dovetail.SDK.ModelMap.Serialization;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;

namespace Dovetail.SDK.History
{
	public class HistoryRegistry : Registry
	{
		public HistoryRegistry()
		{
			Scan(_ =>
			{
				_.TheCallingAssembly();
				_.WithDefaultConventions();
			});

			Scan(_ =>
			{
				_.TheCallingAssembly();

				_.ExcludeType<DefaultHistoryAssembler>();
				_.ExcludeType<DefaultActEntryResolutionPolicy>();

				_.AddAllTypesOf<IElementVisitor>();
				_.AddAllTypesOf<IHistoryAssemblyPolicy>();
				_.AddAllTypesOf<IActEntryResolutionPolicy>();
			});
		}
	}
}