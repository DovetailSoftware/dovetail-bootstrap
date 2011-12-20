using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Filters;
using FChoice.Foundation.Schema;

namespace Dovetail.SDK.Bootstrap.History
{
    public class HistoryBuilder
    {
        private readonly IClarifySessionCache _sessionCache;
        private readonly ISchemaCache _schemaCache;
        private readonly DefaultActEntryTemplatePolicy _templatePolicy;

        public HistoryBuilder(IClarifySessionCache sessionCache, ISchemaCache schemaCache, DefaultActEntryTemplatePolicy templatePolicy)
        {
            _sessionCache = sessionCache;
            _schemaCache = schemaCache;
            _templatePolicy = templatePolicy;
        }

        public IEnumerable<HistoryItem> Build(WorkflowObject workflowObject, Filter actEntryFilter)
        {
            var clarifyDataSet = _sessionCache.GetUserSession().CreateDataSet();

            var workflowObjectInfo = WorkflowObjectInfo.GetObjectInfo(workflowObject.Type);
            var workflowGeneric = clarifyDataSet.CreateGenericWithFields(workflowObjectInfo.ObjectName);
            workflowGeneric.AppendFilter(workflowObjectInfo.IDFieldName, StringOps.Equals, workflowObject.Id);

            var inverseActivityRelation = workflowObjectInfo.ActivityRelation;
            var activityRelation = _schemaCache.GetRelation("act_entry", inverseActivityRelation).InverseRelation;

            var actEntryGeneric = workflowGeneric.Traverse(activityRelation.Name);
            actEntryGeneric.AppendSort("entry_time", false);

            if (actEntryFilter != null)
            {
                actEntryGeneric.Filter.AddFilter(actEntryFilter);
            }
            
            //TODO think about caching the resulting templateDictionary, generic hierarchy per workflow object.Type (would need to set the rootGeneric filter to the current object Id)

            //build act entry templates and simultaneously populate actEntryGeneric hierarchy with needed related generics
            var templateDictionary = _templatePolicy.RenderTemplate(workflowObject);

            //query generic hierarchy and while using act entry templates transform the results into HistoryItems
            return new HistoryItemAssembler(templateDictionary).Assemble(actEntryGeneric);
        }
    }
}