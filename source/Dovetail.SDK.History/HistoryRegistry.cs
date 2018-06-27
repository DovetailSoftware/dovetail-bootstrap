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

				_.AddAllTypesOf<IElementVisitor>();
			});
		}
	}
}