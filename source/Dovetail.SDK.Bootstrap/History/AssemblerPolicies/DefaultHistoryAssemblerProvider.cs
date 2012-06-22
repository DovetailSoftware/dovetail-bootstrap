using System.Collections.Generic;
using System.Linq;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.Bootstrap.History.AssemblerPolicies
{
    public class DefaultHistoryAssemblerPolicy : IHistoryAssemblerPolicy
    {   
        private readonly HistoryBuilder _historyBuilder;
        
        public DefaultHistoryAssemblerPolicy(HistoryBuilder historyBuilder)
        {
            _historyBuilder = historyBuilder;
        }

        public bool Handles(WorkflowObject workflowObject)
        {
            return workflowObject.Type != WorkflowObject.Case;
        }

        public IEnumerable<HistoryItem> BuildHistory(WorkflowObject workflowObject, Filter actEntryFilter)
        {
            return _historyBuilder.Build(workflowObject, actEntryFilter);
        }

		public IEnumerable<HistoryItem> BuildHistories(string type, string[] ids, Filter actEntryFilter)
		{
			return ids.SelectMany(id =>
			{
				var workflowObject = WorkflowObject.Create(type, id);
				return BuildHistory(workflowObject, actEntryFilter);
			});
		}

    }
}