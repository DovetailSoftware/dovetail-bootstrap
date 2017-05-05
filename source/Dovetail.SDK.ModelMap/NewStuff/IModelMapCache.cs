using System.Collections.Generic;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public interface IModelMapCache
    {
        IEnumerable<ModelMap> Maps();
		IEnumerable<ModelMap> Partials();
		void Clear();
    }
}