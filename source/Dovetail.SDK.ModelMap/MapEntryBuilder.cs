using Dovetail.SDK.ModelMap.ObjectModel;
using StructureMap;

namespace Dovetail.SDK.ModelMap
{
    public class MapEntryBuilder : IMapEntryBuilder
    {
        private readonly IContainer _container;

        public MapEntryBuilder(IContainer container)
        {
            _container = container;
        }
        
        public ClarifyGenericMapEntry BuildFromModelMap(ModelMap modelMap)
        {
            var visitor = _container.GetInstance<DovetailGenericModelMapVisitor>();
            modelMap.Accept(visitor);
            var generic = visitor.RootGenericMap;
	        generic.Entity = modelMap.Entity;

	        return generic;
        }
    }
}