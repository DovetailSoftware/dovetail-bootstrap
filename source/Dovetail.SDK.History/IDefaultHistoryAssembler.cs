using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.ModelMap;
using FChoice.Common.Data;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.History
{
	public interface IDefaultHistoryAssembler : IHistoryAssemblyPolicy
	{
	}

	public class DefaultHistoryAssembler : IDefaultHistoryAssembler
	{
		private readonly IHistoryMapRegistry _models;
		private readonly ISchemaCache _schema;

		public DefaultHistoryAssembler(IHistoryMapRegistry models, ISchemaCache schema)
		{
			_models = models;
			_schema = schema;
		}

		public bool Matches(HistoryRequest request)
		{
			return true;
		}

		public HistoryResult HistoryFor(HistoryRequest request, IHistoryBuilder builder)
		{
			var activityCodes = new List<int>();
			var gatherer = new ActEntryGatherer(activityCodes, request.ShowAllActivities);
			var map = _models.Find(request.WorkflowObject);
			map.Accept(gatherer);

			var codeArg = activityCodes.Select(_ => _.ToString()).Join(",");
			var entryTimeArg = request.Since.HasValue
				? " AND entry_time {0} '{1}'".ToFormat(request.ReverseOrder ? ">" : "<", request.Since.ToString())
				: "";

			var idArg = "";
			var workflowObjectInfo = WorkflowObjectInfo.GetObjectInfo(request.WorkflowObject.Type);
			var inverseActivityRelation = workflowObjectInfo.ActivityRelation;
			var activityRelation = _schema.GetRelation("act_entry", inverseActivityRelation).Name;

			if (workflowObjectInfo.IDFieldName.IsEmpty() || workflowObjectInfo.IDFieldName == "objid")
			{
				idArg = "{0} = {1}".ToFormat(activityRelation, request.WorkflowObject.Id);
			}
			else
			{
				var objId = (int) new SqlHelper("SELECT objid FROM table_{0} WHERE {1} = '{2}'".ToFormat(workflowObjectInfo.DatabaseTable, workflowObjectInfo.IDFieldName, request.WorkflowObject.Id)).ExecuteScalar();
				idArg = "{0} = {1}".ToFormat(activityRelation, objId);
			}

			var command = "SELECT COUNT(1) FROM table_act_entry WHERE act_code IN ({0}){1} AND {2}".ToFormat(codeArg, entryTimeArg, idArg);
			var helper = new SqlHelper(command);
			var count = (int) helper.ExecuteScalar();

			var result = new HistoryResult
			{
				HistoryItemLimit = request.HistoryItemLimit,
				Since = request.Since,
				TotalResults = count,
				Items = new ModelData[0]
			};

			if (count == 0)
			{
				return result;
			}

			command = "SELECT TOP {0} objid FROM table_act_entry WHERE act_code IN ({1}){2} AND {3} ORDER BY entry_time DESC, objid DESC".ToFormat(request.HistoryItemLimit, codeArg, entryTimeArg, idArg);
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

			result.Items = builder.GetAll(request, generic => generic.Filter(_ => _.IsIn("objid", ids.ToArray())));
			return result;
		}
	}
}