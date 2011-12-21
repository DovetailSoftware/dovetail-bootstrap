using System.Collections.Generic;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.Bootstrap.History.AssemblerPolicies
{
    public interface IHistoryAssemblerPolicy
    {
        bool Handles(WorkflowObject workflowObject);
        IEnumerable<HistoryItem> BuildHistory(WorkflowObject workflowObject, Filter actEntryFilter);
    }
}