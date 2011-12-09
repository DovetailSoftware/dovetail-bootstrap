using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Filters;
using FChoice.Foundation.Schema;

namespace Dovetail.SDK.Bootstrap.History
{
    public interface IWorkflowHistoryBuilder
    {
        bool Handles(WorkflowObject workflowObject);
        IEnumerable<HistoryItem> BuildHistory(WorkflowObject workflowObject, Filter actEntryFilter);
    }

    public class WorkflowHistoryBuilder : IWorkflowHistoryBuilder
    {   
        private readonly IClarifySessionCache _sessionCache;
        private readonly ISchemaCache _schemaCache;
		
        public WorkflowHistoryBuilder(IClarifySessionCache sessionCache, ISchemaCache schemaCache)
        {
            _sessionCache = sessionCache;
            _schemaCache = schemaCache;
        }

        public bool Handles(WorkflowObject workflowObject)
        {
            return true;
        }

        public IEnumerable<HistoryItem> BuildHistory(WorkflowObject workflowObject, Filter actEntryFilter)
        {
            var workflowObjectInfo = WorkflowObjectInfo.GetObjectInfo(workflowObject.Type);

            var clarifyDataSet = _sessionCache.GetUserSession().CreateDataSet();
            var workflowGeneric = clarifyDataSet.CreateGeneric(workflowObjectInfo.ObjectName);
            workflowGeneric.AppendFilter(workflowObjectInfo.IDFieldName, StringOps.Equals, workflowObject.Id);
            workflowGeneric.DataFields.Add("title");

            var conditionGeneric = workflowGeneric.Traverse(workflowObjectInfo.ConditionRelation);
            conditionGeneric.DataFields.Add("title");

            var inverseActivityRelation = workflowObjectInfo.ActivityRelation;
            var activityRelation = _schemaCache.GetRelation("act_entry", inverseActivityRelation).InverseRelation;

            var actEntryGeneric = workflowGeneric.Traverse(activityRelation.Name);

            if (actEntryFilter != null)
            {
                actEntryGeneric.Filter.AddFilter(actEntryFilter);
            }

            //build act entry templates and simultaneously populate actEntryGeneric hierarchy with needed related generics
            var templateDictionary = new ActEntryTemplateBuilder().BuildTemplates(workflowObject, actEntryGeneric);

            //query generic hierarchy and while using act entry templates transform the results into HistoryItems
            return new HistoryItemAssembler(templateDictionary).Assemble(actEntryGeneric);
        }
    }
}