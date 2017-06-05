using System.Collections.Generic;

namespace Dovetail.SDK.ModelMap
{
    public interface IModelMapCache
    {
        IEnumerable<ModelMap> Maps();
		IEnumerable<ModelMap> Partials();
		void Clear();
    }
}