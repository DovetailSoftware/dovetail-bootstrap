using System;
using System.Collections.Generic;
using System.Linq;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.Bootstrap.History
{
	public interface IHistoryAssembler
	{
		HistoryViewModel GetHistory(WorkflowObject workflowObject);
		HistoryViewModel GetHistoryTop(WorkflowObject workflowObject, int numberOfMostRecentEntries);
	    HistoryViewModel GetHistorySince(WorkflowObject workflowObject, DateTime sinceDate);
	}

    public class HistoryAssembler : IHistoryAssembler
	{
	    private readonly IEnumerable<IWorkflowHistoryBuilder> _entityHistoryBuilders;

	    public HistoryAssembler(IEnumerable<IWorkflowHistoryBuilder> entityHistoryBuilders)
		{
		    _entityHistoryBuilders = entityHistoryBuilders;
		}

	    public HistoryViewModel GetHistory(WorkflowObject workflowObject)
		{
			return getHistoryWithConstraint(workflowObject, null);
		}

        public HistoryViewModel GetHistorySince(WorkflowObject workflowObject, DateTime sinceDate)
        {
            var filter = new FilterExpression().MoreThan("entry_time", sinceDate);

            return getHistoryWithConstraint(workflowObject, filter);
        }

		public HistoryViewModel GetHistoryTop(WorkflowObject workflowObject, int numberOfMostRecentEntries)
		{
            return getHistoryForFirst(workflowObject, null, numberOfMostRecentEntries);
		}

		private HistoryViewModel getHistoryWithConstraint(WorkflowObject workflowObject, Filter actEntryFilter)
		{
            var historyItems = getHistoryItems(workflowObject, actEntryFilter);

            return new HistoryViewModel { WorkflowObject= workflowObject, HistoryItems = historyItems };
		}

	    private IEnumerable<HistoryItem> getHistoryItems(WorkflowObject workflowObject, Filter actEntryFilter)
	    {
	        var historyItems = new List<HistoryItem>();

	        foreach (var builder in _entityHistoryBuilders.Where(builder => builder.Handles(workflowObject)))
	        {
	            historyItems.AddRange(builder.BuildHistory(workflowObject, actEntryFilter));
	        }

	        return historyItems.OrderByDescending(h => h.When);
	    }

        private HistoryViewModel getHistoryForFirst(WorkflowObject workflowObject, Filter actEntryFilter, int numberOfEntries)
		{
            var historyItems = getHistoryItems(workflowObject, actEntryFilter).Take(numberOfEntries);

            return new HistoryViewModel { WorkflowObject= workflowObject, HistoryItems = historyItems };
		}
	}
}
