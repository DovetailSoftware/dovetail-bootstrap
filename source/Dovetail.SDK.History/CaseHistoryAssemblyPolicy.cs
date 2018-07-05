using System;
using System.Collections.Generic;
using System.Linq;
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

		public CaseHistoryAssemblyPolicy(IHistoryMapRegistry models, HistorySettings settings)
		{
			_models = models;
			_settings = settings;
		}

		public bool Matches(HistoryRequest request)
		{
			return request.WorkflowObject.Type.EqualsIgnoreCase("case")
				   && _settings.MergeCaseHistoryChildSubcases;
		}

		public HistoryResult HistoryFor(HistoryRequest request, IHistoryBuilder builder)
		{
			var activityCodes = determineActCodes(request.ShowAllActivities);

			var actEntries = resolveEntries(request, activityCodes.ToArray());
			var caseHistoryItems = actEntries.CaseEntries.Any()
				? builder.GetAll(request, generic => generic.Filter(_ => _.IsIn("objid", actEntries.CaseEntries.ToArray())))
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
				? items.OrderBy(_ => _.Get<DateTime>("timestamp")).ToArray()
				: items.OrderByDescending(_ => _.Get<DateTime>("timestamp")).ToArray();

			return new HistoryResult
			{
				HistoryItemLimit = request.HistoryItemLimit,
				Since = request.Since,
				TotalResults = actEntries.Count,
				Items = combinedItems
			};
		}

		private int[] determineActCodes(bool showAllActivities)
		{
			var actCodes = new List<int>();
			actCodes.AddRange(determineActCodes(new WorkflowObject {Type = "case"}, showAllActivities));
			actCodes.AddRange(determineActCodes(new WorkflowObject { Type = "subcase" }, showAllActivities));

			return actCodes.ToArray();
		}

		private IEnumerable<int> determineActCodes(WorkflowObject workflowObject, bool showAllActivities)
		{
			var activityCodes = new List<int>();
			var gatherer = new ActEntryGatherer(activityCodes, showAllActivities);
			var map = _models.Find(workflowObject);
			map.Accept(gatherer);

			return activityCodes;
		}

		private ActEntryResult resolveEntries(HistoryRequest request, int[] actCodes)
		{
			var codeArg = actCodes.Select(_ => _.ToString()).Join(",");
			var entryTimeArg = request.Since.HasValue
				? " AND entry_time {0} '{1}'".ToFormat(request.ReverseOrder ? ">" : "<", request.Since.ToString())
				: "";

			var objId = (int)new SqlHelper("SELECT objid FROM table_case WHERE id_number = '{0}'".ToFormat(request.WorkflowObject.Id)).ExecuteScalar();
			var command = "SELECT COUNT(1) FROM table_act_entry WHERE act_code IN ({0}){1} AND (act_entry2case = {2} OR act_entry2subcase IN (SELECT objid FROM table_subcase WHERE subcase2case = {2}))".ToFormat(codeArg, entryTimeArg, objId);
			var helper = new SqlHelper(command);
			var count = (int)helper.ExecuteScalar();

			if (count == 0)
			{
				return new ActEntryResult
				{
					Count = 0,
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

			command = "SELECT TOP {0} objid, act_entry2case, act_entry2subcase FROM table_act_entry WHERE act_code IN ({1}){2} AND (act_entry2case = {3} OR act_entry2subcase IN (SELECT objid FROM table_subcase WHERE subcase2case = {3})) ORDER BY entry_time DESC, objid DESC".ToFormat(request.HistoryItemLimit, codeArg, entryTimeArg, objId);
			helper = new SqlHelper(command);

			var caseIds = new List<int>();
			var subcaseIds = new List<int>();
			using (var reader = helper.ExecuteReader())
			{
				while (reader.Read())
				{
					var objid = reader.GetInt32(0);
					if(!reader.IsDBNull(reader.GetOrdinal("act_entry2subcase")))
						subcaseIds.Add(objid);
					else
						caseIds.Add(objid);
				}
			}

			return new ActEntryResult
			{
				Count = count,
				Subcases = ids.ToArray(),
				CaseEntries = caseIds.ToArray(),
				SubcaseEntries = subcaseIds.ToArray()
			};
		}

		private class ActEntryResult
		{
			public int Count { get; set; }
			public int[] Subcases { get; set; }
			public int[] CaseEntries { get; set; }
			public int[] SubcaseEntries { get; set; }
		}
	}
}