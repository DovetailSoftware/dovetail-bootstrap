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
		HistoryViewModel GetHistory(WorkflowObject workflowObject);
		HistoryViewModel GetHistoryTop(WorkflowObject workflowObject, int numberOfMostRecentEntries);
	    HistoryViewModel GetHistorySince(WorkflowObject workflowObject, DateTime sinceDate);
	}

	public class HistoryBuilder : IHistoryBuilder
	{
		private readonly IHistoryActEntryBuilder _historyActEntryBuilder;
        private readonly IClarifySessionCache _sessionCache;
		private readonly ISchemaCache _schemaCache;
		private WorkflowObjectInfo _workflowObjectInfo;

		public HistoryBuilder(IHistoryActEntryBuilder historyActEntryBuilder, IClarifySessionCache sessionCache, ISchemaCache schemaCache)
		{
			_historyActEntryBuilder = historyActEntryBuilder;
			_sessionCache = sessionCache;
			_schemaCache = schemaCache;
		}

		public HistoryViewModel GetHistory(WorkflowObject workflowObject)
		{
			_workflowObjectInfo = WorkflowObjectInfo.GetObjectInfo(workflowObject.Type);

            return getHistoryWithConstraint(workflowObject, null);
		}

        public HistoryViewModel GetHistorySince(WorkflowObject workflowObject, DateTime sinceDate)
        {
            _workflowObjectInfo = WorkflowObjectInfo.GetObjectInfo(workflowObject.Type);

            var filter = new FilterExpression().MoreThan("entry_time", sinceDate);

            return getHistoryWithConstraint(workflowObject, filter);
        }

		public HistoryViewModel GetHistoryTop(WorkflowObject workflowObject, int numberOfMostRecentEntries)
		{
            _workflowObjectInfo = WorkflowObjectInfo.GetObjectInfo(workflowObject.Type);

            return getHistoryForFirst(workflowObject, null, numberOfMostRecentEntries);
		}

		private HistoryViewModel getHistoryWithConstraint(WorkflowObject workflowObject, Filter actEntryFilter)
		{
            var historyItems = getHistoryItems(workflowObject, actEntryFilter);

            return new HistoryViewModel { WorkflowObject= workflowObject, HistoryItems = historyItems };
		}

	    private IEnumerable<HistoryItem> getHistoryItems(WorkflowObject workflowObject, Filter actEntryFilter)
	    {
	        var clarifyDataSet = _sessionCache.GetUserSession().CreateDataSet();
	        var workflowGeneric = clarifyDataSet.CreateGeneric(_workflowObjectInfo.ObjectName);
            workflowGeneric.AppendFilter(_workflowObjectInfo.IDFieldName, StringOps.Equals, workflowObject.Id);
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

            var activityDTOS = _historyActEntryBuilder.Query(workflowObject, actEntryGeneric).OrderByDescending(a => a.When);

            return activityDTOS;
	    }

        private HistoryViewModel getHistoryForFirst(WorkflowObject workflowObject, Filter actEntryFilter, int numberOfEntries)
		{
            var historyItems = getHistoryItems(workflowObject, actEntryFilter).Take(numberOfEntries);

            return new HistoryViewModel { WorkflowObject= workflowObject, HistoryItems = historyItems };
		}
	}
}
