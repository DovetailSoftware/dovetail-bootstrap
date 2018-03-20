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

		public IEnumerable<HistoryItem> Build(HistoryRequest request)
		{
			return Build(request, (generic, workflowObjectInfo) =>
			{
				if (workflowObjectInfo.IDFieldName.IsEmpty() || workflowObjectInfo.IDFieldName == "objid")
				{
					generic.Filter(f => f.Equals("objid", Convert.ToInt32(request.WorkflowObject.Id)));
				}
				else
				{
					generic.Filter(f => f.Equals(workflowObjectInfo.IDFieldName, request.WorkflowObject.Id));
				}
			});
		}

		public IEnumerable<HistoryItem> Build(HistoryRequest request, string[] ids)
		{
			return Build(request, (generic,workflowObjectInfo) => generic.Filter(f => f.IsIn(workflowObjectInfo.IDFieldName, ids)));
		}

		private IEnumerable<HistoryItem> Build(HistoryRequest request, Action<ClarifyGeneric, WorkflowObjectInfo> genericAction)
		{
			var clarifyDataSet = _session.CreateDataSet();

			var workflowObjectInfo = WorkflowObjectInfo.GetObjectInfo(request.WorkflowObject.Type);
			var workflowGeneric = clarifyDataSet.CreateGenericWithFields(workflowObjectInfo.ObjectName);
			if (workflowObjectInfo.HasIDFieldName)
			{
				workflowGeneric.DataFields.Add(workflowObjectInfo.IDFieldName);
			}
			
			genericAction(workflowGeneric, workflowObjectInfo);

			var inverseActivityRelation = workflowObjectInfo.ActivityRelation;
			var activityRelation = _schemaCache.GetRelation("act_entry", inverseActivityRelation).InverseRelation;

			var actEntryGeneric = workflowGeneric.Traverse(activityRelation.Name);
			actEntryGeneric.AppendSort("entry_time", false);
			actEntryGeneric.AppendSort("objid", false);

			if (request.Since.HasValue)
			{
				var filter = new FilterExpression().MoreThan("entry_time", request.Since.Value);
				actEntryGeneric.Filter.AddFilter(filter);
			}

			var templateDictionary = _templatePolicyConfiguration.RenderPolicies(request.WorkflowObject);

			//query generic hierarchy and while using act entry templates transform the results into HistoryItems
			return _historyItemAssembler.Assemble(actEntryGeneric, templateDictionary, request);						
		}
	}
}