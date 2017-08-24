using StructureMap;

namespace Dovetail.SDK.ModelMap
{
	public static class ModelMapManager
	{
		public static void Initialize(IContainer container)
		{
			var settings = container.GetInstance<ModelMapSettings>();
			if (!settings.EnableCache)
				return;

			container.Model.EjectAndRemove<IModelMapCache>();
			container.Configure(_ => _.ForSingletonOf<IModelMapCache>().Use<ModelMapCache>());
		}
	}
}