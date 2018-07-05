using System;
using Dovetail.SDK.ModelMap.ObjectModel;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.History
{
	public interface IHistoryMapEntryBuilder
	{
		ClarifyGenericMapEntry BuildFromModelMap(HistoryRequest request, ModelMap.ModelMap modelMap);
		ClarifyGenericMapEntry BuildFromModelMap(HistoryRequest request, ModelMap.ModelMap modelMap, Action<ClarifyGeneric> configureWorkflowGeneric);
	}
}