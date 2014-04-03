using System.Collections.Generic;

namespace Dovetail.SDK.Bootstrap.History.AssemblerPolicies
{
    public interface IHistoryAssemblerPolicy
    {
        bool Handles(WorkflowObject workflowObject);
        IEnumerable<HistoryItem> BuildHistory(HistoryRequest request);
    }
}