using System;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.ModelMap.ObjectModel;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Filters;
using FChoice.Foundation.Schema;
using FubuCore;
using StructureMap;

namespace Dovetail.SDK.History
{
	public class HistoryMapEntryBuilder : IHistoryMapEntryBuilder
	{
		private readonly IContainer _container;

		public HistoryMapEntryBuilder(IContainer container)
		{
			_container = container;
		}

		public ClarifyGenericMapEntry BuildFromModelMap(HistoryRequest request, ModelMap.ModelMap modelMap)
		{
			var session = _container.GetInstance<IClarifySession>();
			var schemaCache = _container.GetInstance<ISchemaCache>();
			var clarifyDataSet = session.CreateDataSet();

			var workflowObjectInfo = WorkflowObjectInfo.GetObjectInfo(request.WorkflowObject.Type);
			var workflowGeneric = clarifyDataSet.CreateGenericWithFields(workflowObjectInfo.ObjectName);
			if (workflowObjectInfo.HasIDFieldName)
			{
				workflowGeneric.DataFields.Add(workflowObjectInfo.IDFieldName);
			}

			if (workflowObjectInfo.IDFieldName.IsEmpty() || workflowObjectInfo.IDFieldName == "objid")
			{
				workflowGeneric.Filter(f => f.Equals("objid", Convert.ToInt32(request.WorkflowObject.Id)));
			}
			else
			{
				workflowGeneric.Filter(f => f.Equals(workflowObjectInfo.IDFieldName, request.WorkflowObject.Id));
			}

			var inverseActivityRelation = workflowObjectInfo.ActivityRelation;
			var activityRelation = schemaCache.GetRelation("act_entry", inverseActivityRelation).InverseRelation;

			var actEntryGeneric = workflowGeneric.Traverse(activityRelation.Name);
			actEntryGeneric.AppendSort("entry_time", false);
			actEntryGeneric.AppendSort("objid", false);

			if (request.Since.HasValue)
			{
				var filter = new FilterExpression().MoreThan("entry_time", request.Since.Value);
				actEntryGeneric.Filter.AddFilter(filter);
			}

			var visitor = _container
				.With(clarifyDataSet)
				.With(actEntryGeneric)
				.With(request.WorkflowObject)
				.GetInstance<HistoryModelMapVisitor>();

			modelMap.Accept(visitor);
			var generic = visitor.RootGenericMap;
			generic.Entity = modelMap.Entity;

			if (!request.ShowAllActivities)
			{
				var activeCodes = visitor.ActEntries.Where(t => !t.IsVerbose).Select(d => d.Code).ToArray();
				actEntryGeneric.Filter(f => f.IsIn("act_code", activeCodes));
			}

			return generic;
		}
	}
}