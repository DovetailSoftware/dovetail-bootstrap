using Dovetail.SDK.ModelMap.NewStuff.ObjectModel;
using StructureMap;

namespace Dovetail.SDK.ModelMap.NewStuff
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