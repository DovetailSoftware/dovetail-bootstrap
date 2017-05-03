using System.Linq;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public class ModelMapRegistry : IModelMapRegistry
    {
        private readonly IModelMapCache _cache;

        public ModelMapRegistry(IModelMapCache cache)
        {
            _cache = cache;
        }

        public ModelMap Find(string name)
        {
            return _cache.Maps().SingleOrDefault(_ => _.Name.EqualsIgnoreCase(name));
        }
    }
}