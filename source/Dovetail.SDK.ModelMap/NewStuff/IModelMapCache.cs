using System.Collections.Generic;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public interface IModelMapCache
    {
        IEnumerable<ModelMap> Maps();
        void Clear();
    }
}