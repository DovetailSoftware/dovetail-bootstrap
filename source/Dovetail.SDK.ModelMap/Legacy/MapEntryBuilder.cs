using Dovetail.SDK.ModelMap.Legacy.ObjectModel;
using Dovetail.SDK.ModelMap.Legacy.Registration;
using StructureMap;

namespace Dovetail.SDK.ModelMap.Legacy
{
    public class MapEntryBuilder : IMapEntryBuilder
    {
        private readonly IContainer _container;

        public MapEntryBuilder(IContainer container)
        {
            _container = container;
        }

        public ClarifyGenericMapEntry BuildFromModelMap<MODEL>(ModelMap<MODEL> modelMap)
        {
            var visitor = _container.GetInstance<DovetailGenericModelMapVisitor>();
            modelMap.Accept(visitor);
            return visitor.RootGenericMap;
        }
    }
}