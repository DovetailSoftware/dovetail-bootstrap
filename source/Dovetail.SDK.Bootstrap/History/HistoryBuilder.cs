using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Filters;
using FChoice.Foundation.Schema;

namespace Dovetail.SDK.Bootstrap.History
{
	public interface IHistoryBuilder
	{
		HistoryViewModel GetHistory(WorkflowObject workflowObject, string id);
		HistoryViewModel GetHistoryTop(WorkflowObject workflowObject, string id, int numberOfMostRecentEntries);
	    HistoryViewModel GetHistorySince(WorkflowObject workflowObject, string id, DateTime sinceDate);
	}

	public class HistoryBuilder : IHistoryBuilder
	{
		private readonly IHistoryActEntryBuilder _historyActEntryBuilder;
		private readonly IClarifySession _session;
		private readonly ISchemaCache _schemaCache;
		private WorkflowObjectInfo _workflowObjectInfo;

		public HistoryBuilder(IHistoryActEntryBuilder historyActEntryBuilder, IClarifySession session, ISchemaCache schemaCache)
		{
			_historyActEntryBuilder = historyActEntryBuilder;
			_session = session;
			_schemaCache = schemaCache;
		}

		public HistoryViewModel GetHistory(WorkflowObject workflowObject, string id)
		{
			_workflowObjectInfo = WorkflowObjectInfo.GetObjectInfo(workflowObject.ToString());

			return getHistoryWithConstraint(id, null);
		}

        public HistoryViewModel GetHistorySince(WorkflowObject workflowObject, string id, DateTime sinceDate)
        {
            _workflowObjectInfo = WorkflowObjectInfo.GetObjectInfo(workflowObject.ToString());

            var filter = new FilterExpression().MoreThan("entry_time", sinceDate);

            return getHistoryWithConstraint(id, filter);
        }

		public HistoryViewModel GetHistoryTop(WorkflowObject workflowObject, string id, int numberOfMostRecentEntries)
		{
			_workflowObjectInfo = WorkflowObjectInfo.GetObjectInfo(workflowObject.ToString());

			return getHistoryForFirst(id, null ,numberOfMostRecentEntries);
		}

		private HistoryViewModel getHistoryWithConstraint(string id, Filter actEntryFilter)
		{
		    var historyItems = getHistoryItems(id, actEntryFilter);

		    return new HistoryViewModel {Id = id, HistoryItems = historyItems};
		}

	    private IEnumerable<HistoryItem> getHistoryItems(string id, Filter actEntryFilter)
	    {
	        var clarifyDataSet = _session.CreateDataSet();
	        var workflowGeneric = clarifyDataSet.CreateGeneric(_workflowObjectInfo.ObjectName);
	        workflowGeneric.AppendFilter(_workflowObjectInfo.IDFieldName, StringOps.Equals, id);
	        workflowGeneric.DataFields.Add("title");

	        var conditionGeneric = workflowGeneric.Traverse(_workflowObjectInfo.ConditionRelation);
	        conditionGeneric.DataFields.Add("title");

	        var inverseActivityRelation = _workflowObjectInfo.ActivityRelation;
	        var activityRelation = _schemaCache.GetRelation("act_entry", inverseActivityRelation).InverseRelation;

	        var actEntryGeneric = workflowGeneric.Traverse(activityRelation.Name);

	        if (actEntryFilter != null)
	        {
	            actEntryGeneric.Filter.AddFilter(actEntryFilter);
	        }

            var activityDTOS = _historyActEntryBuilder.Query(actEntryGeneric).OrderByDescending(a => a.When);

            return activityDTOS;
	    }

	    private HistoryViewModel getHistoryForFirst(string id, Filter actEntryFilter, int numberOfEntries)
		{
            var historyItems = getHistoryItems(id, actEntryFilter).Take(numberOfEntries);

            return new HistoryViewModel { Id = id, HistoryItems = historyItems };
		}
	}
}
