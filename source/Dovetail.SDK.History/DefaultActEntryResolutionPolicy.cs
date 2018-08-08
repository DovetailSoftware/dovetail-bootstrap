using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.ModelMap;
using FChoice.Common.Data;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.History
{
	public class DefaultActEntryResolutionPolicy : IDefaultActEntryResolutionPolicy
	{
		private readonly ISchemaCache _schema;

		public DefaultActEntryResolutionPolicy(ISchemaCache schema)
		{
			_schema = schema;
		}

		public bool Matches(HistoryRequest request)
		{
			return true;
		}

		public ActEntryResolution IdsFor(HistoryRequest request, int[] actCodes)
		{
			var codeArg = actCodes.Select(_ => _.ToString()).Join(",");
			var entryTimeArg = request.Since.HasValue
				? " AND entry_time {0} '{1}'".ToFormat(request.ReverseOrder ? ">=" : "<=", request.Since.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"))
				: "";

			var idArg = "";
			var workflowObjectInfo = WorkflowObjectInfo.GetObjectInfo(request.WorkflowObject.Type);
			var inverseActivityRelation = workflowObjectInfo.ActivityRelation;

			if (inverseActivityRelation.IsEmpty())
				throw new InvalidOperationException("Cannot traverse from {0} to act_entry".ToFormat(request.WorkflowObject.Type));

			var activityRelation = _schema.GetRelation("act_entry", inverseActivityRelation).Name;
			if (workflowObjectInfo.IDFieldName.IsEmpty() || workflowObjectInfo.IDFieldName == "objid")
			{
				idArg = "{0} = {1}".ToFormat(activityRelation, request.WorkflowObject.Id);
			}
			else
			{
				var objId = (int)new SqlHelper("SELECT objid FROM table_{0} WHERE {1} = '{2}'".ToFormat(workflowObjectInfo.DatabaseTable, workflowObjectInfo.IDFieldName, request.WorkflowObject.Id)).ExecuteScalar();
				idArg = "{0} = {1}".ToFormat(activityRelation, objId);
			}

			var command = "SELECT COUNT(1) FROM table_act_entry WHERE act_code IN ({0}){1} AND {2}".ToFormat(codeArg, entryTimeArg, idArg);
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
			command = "SELECT TOP {0} objid, entry_time FROM table_act_entry WHERE act_code IN ({1}){2} AND {3} ORDER BY entry_time {4}, objid {4}".ToFormat(request.HistoryItemLimit, codeArg, entryTimeArg, idArg, orderDirection);
			helper = new SqlHelper(command);

			DateTime? last = null;
			var ids = new List<int>();
			using (var reader = helper.ExecuteReader())
			{
				while (reader.Read())
				{
					var objid = reader.GetInt32(0);
					ids.Add(objid);

					last = reader.GetDateTime(reader.GetOrdinal("entry_time"));
				}
			}

			return new ActEntryResolution
			{
				Count = count,
				Ids = ids,
				LastTimestamp = last
			};
		}
	}
}
