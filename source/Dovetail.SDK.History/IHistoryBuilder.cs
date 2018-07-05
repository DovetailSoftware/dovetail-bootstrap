using System;
using Dovetail.SDK.ModelMap;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.History
{
	public interface IHistoryBuilder
	{
		ModelData[] GetAll(HistoryRequest request);
		ModelData[] GetAll(HistoryRequest request, Action<ClarifyGeneric> configureActEntryGeneric);
		ModelData[] GetAll(HistoryRequest request, Action<ClarifyGeneric> configureActEntryGeneric, Action<ClarifyGeneric> configureWorkflowGeneric);
	}
}