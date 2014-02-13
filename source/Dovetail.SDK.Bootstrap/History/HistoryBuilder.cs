using System;
using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.History.Configuration;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Filters;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History
{
	public class HistoryBuilder
	{
		private readonly IClarifySession _session;
		private readonly ISchemaCache _schemaCache;
		private readonly IActEntryTemplatePolicyConfiguration _templatePolicyConfiguration;
		private readonly IHistoryItemAssembler _historyItemAssembler;

		public HistoryBuilder(IClarifySession session, ISchemaCache schemaCache, IActEntryTemplatePolicyConfiguration templatePolicyConfiguration, IHistoryItemAssembler historyItemAssembler)
		{
			_session = session;
			_schemaCache = schemaCache;
			_templatePolicyConfiguration = templatePolicyConfiguration;
			_historyItemAssembler = historyItemAssembler;
		}

		public IEnumerable<HistoryItem> Build(WorkflowObject workflowObject, Filter actEntryFilter)
		{
			var clarifyDataSet = _session.CreateDataSet();

			var workflowObjectInfo = WorkflowObjectInfo.GetObjectInfo(workflowObject.Type);
			var workflowGeneric = clarifyDataSet.CreateGenericWithFields(workflowObjectInfo.ObjectName);

			if (workflowObjectInfo.IDFieldName.IsEmpty() || workflowObjectInfo.IDFieldName == "objid")
			{
				workflowGeneric.Filter(f => f.Equals("objid", Convert.ToInt32(workflowObject.Id)));
			}
			else
			{
				workflowGeneric.Filter(f => f.Equals(workflowObjectInfo.IDFieldName, workflowObject.Id));
			}

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
			return _historyItemAssembler.Assemble(actEntryGeneric, templateDictionary, workflowObject);
		}
	}
}