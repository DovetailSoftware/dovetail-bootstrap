using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.ModelMap;
using FChoice.Common.Data;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.History
{
	public class NonWorkflowHistoryAssembler : IHistoryAssemblyPolicy
	{
		private readonly IHistoryMapRegistry _models;
		private readonly IServiceLocator _services;
		private readonly ICurrentSDKUser _user;
		private readonly ISchemaCache _schema;

		public NonWorkflowHistoryAssembler(IHistoryMapRegistry models, IServiceLocator services, ICurrentSDKUser user, ISchemaCache schema)
		{
			_models = models;
			_services = services;
			_user = user;
			_schema = schema;
		}

		public bool Matches(HistoryRequest request)
		{
			var workflowObjectInfo = WorkflowObjectInfo.GetObjectInfo(request.WorkflowObject.Type);
			return workflowObjectInfo.ActivityRelation.IsEmpty();
		}

		public HistoryResult HistoryFor(HistoryRequest request, IHistoryBuilder builder)
		{
			var activityCodes = new List<int>();
			var gatherer = new ActEntryGatherer(activityCodes, request.ShowAllActivities, request.WorkflowObject, _services, _user);
			var map = _models.Find(request.WorkflowObject);
			map.Accept(gatherer);

			var options = new ActEntryOptions
			{
				FocusType = _schema.GetTableNumber(request.WorkflowObject.Type),
				FocusId = int.Parse(request.WorkflowObject.Id)
			};
			var actEntries = IdsFor(request, options, activityCodes.ToArray());


			var items = actEntries.Ids.Any()
				? builder.GetAll(request, options, generic => generic.Filter(_ => _.IsIn("objid", actEntries.Ids.ToArray())))
				: new ModelData[0];

			return new HistoryResult
			{
				HistoryItemLimit = request.HistoryItemLimit,
				Since = request.Since,
				TotalResults = actEntries.Count,
				Items = items
			};
		}

		public ActEntryResolution IdsFor(HistoryRequest request, ActEntryOptions options, int[] actCodes)
		{
			var codeArg = actCodes.Select(_ => _.ToString()).Join(",");
			var entryTimeArg = request.Since.HasValue
				? " AND entry_time {0} '{1}'".ToFormat(request.ReverseOrder ? ">=" : "<=", request.Since.ToString())
				: "";

			var findByFocusTypeAndId = "focus_type = {0} AND focus_lowid = {1}".ToFormat(options.FocusType, options.FocusId);
			var focusArg = findByFocusTypeAndId;
			var workflowObjectInfo = WorkflowObjectInfo.GetObjectInfo(request.WorkflowObject.Type);
			if (workflowObjectInfo.UseParticipantActEntryModel)
			{
				focusArg = "(({0}) OR objid IN (SELECT participant2act_entry FROM table_participant WHERE {0}))".ToFormat(findByFocusTypeAndId);
			}

			var command = "SELECT COUNT(1) FROM table_act_entry WHERE act_code IN ({0}){1} AND {2}".ToFormat(codeArg, entryTimeArg, focusArg);
			var helper = new SqlHelper(command);
			var count = (int)helper.ExecuteScalar();

			if (count == 0)
			{
				return new ActEntryResolution
				{
					Count = 0,
					Ids = new int[0]
				};
			}

			var orderDirection = request.ReverseOrder ? "ASC" : "DESC";
			command = "SELECT TOP {0} objid FROM table_act_entry WHERE act_code IN ({1}){2} AND {3} ORDER BY entry_time {4}, objid {4}".ToFormat(request.HistoryItemLimit, codeArg, entryTimeArg, focusArg, orderDirection);
			helper = new SqlHelper(command);

			var ids = new List<int>();
			using (var reader = helper.ExecuteReader())
			{
				while (reader.Read())
				{
					var objid = reader.GetInt32(0);
					ids.Add(objid);
				}
			}

			return new ActEntryResolution
			{
				Count = count,
				Ids = ids
			};
		}
	}
}
