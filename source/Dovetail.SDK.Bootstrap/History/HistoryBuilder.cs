using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.History.Configuration;
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
        private readonly IActEntryTemplatePolicyConfiguration _templatePolicyConfiguration;

        public HistoryBuilder(IClarifySessionCache sessionCache, ISchemaCache schemaCache, IActEntryTemplatePolicyConfiguration templatePolicyConfiguration)
        {
            _sessionCache = sessionCache;
            _schemaCache = schemaCache;
            _templatePolicyConfiguration = templatePolicyConfiguration;
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
            
            var templateDictionary = _templatePolicyConfiguration.RenderPolicies(workflowObject);
            
            //query generic hierarchy and while using act entry templates transform the results into HistoryItems
            return new HistoryItemAssembler(templateDictionary, workflowObject).Assemble(actEntryGeneric);
        }
    }
}