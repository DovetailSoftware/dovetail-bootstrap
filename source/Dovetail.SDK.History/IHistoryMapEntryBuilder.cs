using Dovetail.SDK.ModelMap.ObjectModel;

namespace Dovetail.SDK.History
{
	public interface IHistoryMapEntryBuilder
	{
		ClarifyGenericMapEntry BuildFromModelMap(HistoryRequest request, ModelMap.ModelMap modelMap);
	}
}