using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.ModelMap;
using FChoice.Common.Data;
using FubuCore;

namespace Dovetail.SDK.History
{
	public class CaseHistoryAssemblyPolicy : IHistoryAssemblyPolicy
	{
		private readonly IHistoryMapRegistry _models;
		private readonly HistorySettings _settings;
		private readonly IServiceLocator _services;
		private readonly ICurrentSDKUser _user;

		public CaseHistoryAssemblyPolicy(IHistoryMapRegistry models, HistorySettings settings, IServiceLocator services, ICurrentSDKUser user)
		{
			_models = models;
			_settings = settings;
			_services = services;
			_user = user;
		}

		public bool Matches(HistoryRequest request)
		{
			return request.WorkflowObject.Type.EqualsIgnoreCase("case")
				   && _settings.MergeCaseHistoryChildSubcases;
		}

		public HistoryResult HistoryFor(HistoryRequest request, IHistoryBuilder builder)
		{
			var caseActivityCodes = determineActCodes(new WorkflowObject { Type = "case" }, request.ShowAllActivities);
			var subcaseActivityCodes = determineActCodes(new WorkflowObject { Type = "subcase", IsChild = true }, request.ShowAllActivities);

			var actEntries = resolveEntries(request, caseActivityCodes.ToArray(), subcaseActivityCodes.ToArray());
			var caseHistoryItems = actEntries.CaseEntries.Any()
				? builder.GetAll(request, generic => generic.Filter(_ => _.IsIn("objid", actEntries.CaseEntries.ToArray())), workflowGeneric => workflowGeneric.Filter(_ => _.Equals("id_number", request.WorkflowObject.Id)))
				: new ModelData[0];

			var childRequest = new HistoryRequest
			{
				WorkflowObject = new WorkflowObject { Type = WorkflowObject.Subcase, IsChild = true },
				ShowAllActivities = request.ShowAllActivities
			};
			var subcaseHistoryItems = actEntries.SubcaseEntries.Any() && actEntries.Subcases.Any()
				? builder.GetAll(childRequest,
					actEntryGeneric => actEntryGeneric.Filter(_ => _.IsIn("objid", actEntries.SubcaseEntries.ToArray())),
					workflowGeneric => workflowGeneric.Filter(_ => _.IsIn("objid", actEntries.Subcases.ToArray())))
				: new ModelData[0];

			var items = new List<ModelData>();
			items.AddRange(caseHistoryItems);
			items.AddRange(subcaseHistoryItems);

			var combinedItems = request.ReverseOrder
				? items.OrderBy(_ => _.Get<DateTime>("timestamp")).ThenBy(_ => _.Get<int>("id")).ToArray()
				: items.OrderByDescending(_ => _.Get<DateTime>("timestamp")).ThenByDescending(_ => _.Get<int>("id")).ToArray();

			return new HistoryResult
			{
				HistoryItemLimit = request.HistoryItemLimit,
				Since = request.Since,
				TotalResults = actEntries.Count,
				Items = combinedItems,
				NextTimestamp = request.HistoryItemLimit > (actEntries.CaseEntries.Length + actEntries.SubcaseEntries.Length)
					? null
					: actEntries.LastTimestamp
			};
		}

		private IEnumerable<int> determineActCodes(WorkflowObject workflowObject, bool showAllActivities)
		{
			var activityCodes = new List<int>();
			var gatherer = new ActEntryGatherer(activityCodes, showAllActivities, workflowObject, _services, _user);
			var map = _models.Find(workflowObject);
			map.Accept(gatherer);

			return activityCodes;
		}

		private ActEntryResult resolveEntries(HistoryRequest request, int[] caseActCodes, int[] subcaseActCodes)
		{
			var actCodes = new List<int>(caseActCodes);
			actCodes.AddRange(subcaseActCodes);

			if (!actCodes.Any())
			{
				return new ActEntryResult
				{
					Count = 0,
					Subcases = new int[0],
					CaseEntries = new int[0],
					SubcaseEntries = new int[0]
				};
			}

			var codeArg = actCodes.Select(_ => _.ToString()).Join(",");
			var entryTimeArg = request.EntryTimeArg();
			var order = request.SortOrder();

			var objId = (int)new SqlHelper("SELECT objid FROM table_case WHERE id_number = '{0}'".ToFormat(request.WorkflowObject.Id)).ExecuteScalar();
			var command = "SELECT COUNT(1) FROM table_act_entry WHERE act_code IN ({0}){1} AND (act_entry2case = {2} OR act_entry2subcase IN (SELECT objid FROM table_subcase WHERE subcase2case = {2}))".ToFormat(codeArg, entryTimeArg, objId);
			var helper = new SqlHelper(command);
			var count = (int)helper.ExecuteScalar();

			if (count == 0)
			{
				return new ActEntryResult
				{
					Count = 0,
					Subcases = new int[0],
					CaseEntries	= new int[0],
					SubcaseEntries = new int[0]
				};
			}

			command = "SELECT objid FROM table_subcase WHERE subcase2case = {0}".ToFormat(objId);
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

			command = new StringBuilder("SELECT TOP ")
				.Append(request.SqlLimit())
				.Append(" objid, act_entry2case, act_entry2subcase, entry_time FROM table_act_entry WHERE ")
				.AppendFormat("((act_code IN ({0}) AND act_entry2case = {1})", caseActCodes.Select(_ => _.ToString()).Join(","), objId)
				.Append(" OR ")
				.AppendFormat("(act_code IN ({0}) AND act_entry2subcase IN (SELECT objid FROM table_subcase WHERE subcase2case = {1})))", subcaseActCodes.Select(_ => _.ToString()).Join(","), objId)
				.Append(entryTimeArg)
				.AppendFormat(" ORDER BY entry_time {0}, objid {0}", order)
				.ToString();

			helper = new SqlHelper(command);

			var caseIds = new List<int>();
			var subcaseIds = new List<int>();
			DateTime? lastTimestamp = null;
			using (var reader = helper.ExecuteReader())
			{
				while (reader.Read())
				{
					var objid = reader.GetInt32(0);
					if(!reader.IsDBNull(reader.GetOrdinal("act_entry2subcase")))
						subcaseIds.Add(objid);
					else
						caseIds.Add(objid);

					lastTimestamp = reader.GetDateTime(reader.GetOrdinal("entry_time"));
				}
			}

			return new ActEntryResult
			{
				Count = count,
				Subcases = ids.ToArray(),
				CaseEntries = caseIds.ToArray(),
				SubcaseEntries = subcaseIds.ToArray(),
				LastTimestamp = lastTimestamp
			};
		}

		private class ActEntryResult
		{
			public int Count { get; set; }
			public int[] Subcases { get; set; }
			public int[] CaseEntries { get; set; }
			public int[] SubcaseEntries { get; set; }
			public DateTime? LastTimestamp { get; set; }
		}
	}
}
