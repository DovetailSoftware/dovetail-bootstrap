using System.Collections.Generic;

namespace Dovetail.SDK.History
{
	public interface IHistoryMapCache
	{
		IEnumerable<ModelMap.ModelMap> Maps();
		IEnumerable<ModelMap.ModelMap> Partials();
		void Clear();
	}
}