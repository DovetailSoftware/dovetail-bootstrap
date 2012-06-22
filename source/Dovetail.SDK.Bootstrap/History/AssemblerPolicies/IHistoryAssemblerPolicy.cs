using System.Collections.Generic;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.Bootstrap.History.AssemblerPolicies
{
    public interface IHistoryAssemblerPolicy
    {
        bool Handles(WorkflowObject workflowObject);
        IEnumerable<HistoryItem> BuildHistory(WorkflowObject workflowObject, Filter actEntryFilter);
		IEnumerable<HistoryItem> BuildHistories(string type, string[] ids, Filter actEntryFilter);
    }
}