using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.Extensions;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History
{
	public interface IHistoryActEntryBuilder
	{
        IEnumerable<HistoryItem> Query(WorkflowObject workflowObject, ClarifyGeneric actEntryGeneric);
	}

	public class ActEntryTemplate
	{
		public ActEntryTemplate()
		{
            //TODO make this come from an injectable type? - kjm 10/2011
		    HTMLizer = item =>
		                   {
		                       item.Kind = item.Kind;
		                       item.Detail = item.Detail.HtmlEncode().ToHtml();
                               item.Internal = item.Internal.HtmlEncode().ToHtml();
		                       item.Who.Name = item.Who.Name;
                               item.Who.Login = item.Who.Login;
		                   };
		}

		public int Code { get; set; }
		public string DisplayName { get; set; }
		public bool IsWorkflow;
		public Action<ClarifyDataRow, HistoryItem> ActivityDTOUpdater;
		public ClarifyGeneric RelatedGeneric { get; set; }
		public Action<HistoryItem> ActivityDTOEditor { get; set; }
		public Action<HistoryItem> HTMLizer { get; set; }
	}

	public class ActEntry
	{
		public ClarifyDataRow ActEntryRecord { get; set; }
		public ActEntryTemplate Template { get; set; }
		public HistoryItemEmployee Who { get; set; }
		public DateTime When { get; set; }
		public string AdditionalInfo { get; set; }
	}

	public interface IAfterActEntryCode
	{
		IAfterDisplayName DisplayName(string displayName);
	}

	public interface IAfterDisplayName
	{
		IHasRelatedRow GetRelatedRecord(string relationName);
		IAfterHtmlizer HtmlizeWith(Action<HistoryItem> htmlizer);
		void UpdateActivityDTOWith(Action<ClarifyDataRow, HistoryItem> mapper);
		void EditActivityDTO(Action<HistoryItem> action);
	}

	public interface IAfterHtmlizer
	{
		IHasRelatedRow GetRelatedRecord(string relationName);
		void UpdateActivityDTOWith(Action<ClarifyDataRow, HistoryItem> mapper);
		void EditActivityDTO(Action<HistoryItem> action);
	}

	public interface IHasRelatedRow
	{
		IAfterRelatedFields WithFields(params string[] fieldNames);
	}

	public interface IAfterRelatedFields
	{
		void UpdateActivityDTOWith(Action<ClarifyDataRow, HistoryItem> mapper);
	}

	public abstract class HistoryMap : IAfterActEntryCode, IAfterDisplayName, IHasRelatedRow, IAfterRelatedFields, IAfterHtmlizer
	{
		private readonly IDictionary<int, ActEntryTemplate> _actEntryDefinitions = new Dictionary<int, ActEntryTemplate>();
		private ActEntryTemplate _currentActEntryTemplate;
		public ClarifyGeneric ActEntryGeneric { get; private set; }

		protected abstract void DefineActEntries(WorkflowObject workflowObject);

		public IAfterActEntryCode ActEntry(int code)
		{
			addCurrentActEntryTemplate();

			_currentActEntryTemplate = new ActEntryTemplate {Code = code};

			return this;
		}

		public IAfterDisplayName DisplayName(string displayName)
		{
			_currentActEntryTemplate.DisplayName = displayName;
				
			return this;
		}

		public IHasRelatedRow GetRelatedRecord(string relationName)
		{
			_currentActEntryTemplate.RelatedGeneric = ActEntryGeneric.Traverse(relationName);

			return this;
		}

		public IAfterHtmlizer HtmlizeWith(Action<HistoryItem> htmlizer)
		{
			_currentActEntryTemplate.HTMLizer = htmlizer;

			return this;
		}

		public IAfterRelatedFields WithFields(params string[] fieldNames)
		{
			validateThereIsARelatedRecord();

			_currentActEntryTemplate.RelatedGeneric.DataFields.AddRange(fieldNames);

			return this;
		}

		public void UpdateActivityDTOWith(Action<ClarifyDataRow, HistoryItem> mapper)
		{
			_currentActEntryTemplate.ActivityDTOUpdater = mapper;
		}

		public void EditActivityDTO(Action<HistoryItem> action)
		{
			_currentActEntryTemplate.ActivityDTOEditor = action;
		}

		public IEnumerable<HistoryItem> Query(WorkflowObject workflowObjectType, ClarifyGeneric actEntryGeneric)
		{
			ActEntryGeneric = actEntryGeneric;

			DefineActEntries(workflowObjectType);

			addCurrentActEntryTemplate();

			var codes = _actEntryDefinitions.Values.Select(d => d.Code);
			actEntryGeneric.DataFields.AddRange("act_code", "entry_time", "addnl_info");
			actEntryGeneric.AppendFilterInList("act_code", true, codes.ToArray());

			var actEntryUserGeneric = actEntryGeneric.Traverse("act_entry2user");
			actEntryUserGeneric.DataFields.AddRange("objid", "login_name");

			var actEntryEmployeeGeneric = actEntryUserGeneric.Traverse("user2employee");
			actEntryEmployeeGeneric.DataFields.AddRange("first_name", "last_name");

			actEntryGeneric.Query();
			
			Func<ClarifyDataRow, HistoryItemEmployee> employeeAssembler = actEntryRecord =>
			{
				var userRows = actEntryRecord.RelatedRows(actEntryUserGeneric);
				if (userRows.Length == 0)
					return new HistoryItemEmployee();

				var userRecord = userRows[0];
				var login = userRecord["login_name"].ToString();
				var employeeRows = userRecord.RelatedRows(actEntryEmployeeGeneric);
				if (employeeRows.Length == 0)
					return new HistoryItemEmployee {Login = login};

				var employeeRecord = employeeRows[0];
				var name = "{0} {1}".ToFormat(employeeRecord["first_name"], employeeRecord["last_name"]);
				var id = Convert.ToInt32(employeeRecord.UniqueID);

			    return new HistoryItemEmployee {Name = name, Id = id, Login = login};
			};

			var actEntryDTOS = assembleActEntryDTOs(actEntryGeneric, employeeAssembler);

		    return actEntryDTOS.Select(createActivityDTOFromMapper).ToArray();
		}

		private void validateThereIsARelatedRecord()
		{
			if(_currentActEntryTemplate.RelatedGeneric == null)
				throw new Exception("Cannot add fields unless a record is related. First call GetRelatedRecord()");
		}

		private HistoryItem createActivityDTOFromMapper(ActEntry actEntry)
		{
			var dto = defaultActivityDTOAssembler(actEntry);

			if (isActivityDTOUpdaterPresent(actEntry))
			{
				var relatedRow = actEntry.ActEntryRecord;

				if (actEntry.Template.RelatedGeneric != null)
				{
					var relatedRows = actEntry.ActEntryRecord.RelatedRows(actEntry.Template.RelatedGeneric);

					relatedRow = relatedRows.Length > 0 ? relatedRows[0] : null;
				}

				if(relatedRow != null)
					actEntry.Template.ActivityDTOUpdater(relatedRow, dto);
			}

			if (isActivityDTOEditorPresent(actEntry))
			{
				actEntry.Template.ActivityDTOEditor(dto);
			}

			actEntry.Template.HTMLizer(dto);

			return dto;
		}

		private static bool isActivityDTOUpdaterPresent(ActEntry actEntry)
		{
			return actEntry.Template.ActivityDTOUpdater != null;
		}

		private static bool isActivityDTOEditorPresent(ActEntry actEntry)
		{
			return actEntry.Template.ActivityDTOEditor != null;
		}

		private static HistoryItem defaultActivityDTOAssembler(ActEntry actEntry)
		{
			return new HistoryItem
				{
                    Id = actEntry.ActEntryRecord.DatabaseIdentifier(),
					Kind = actEntry.Template.DisplayName,
					Who = actEntry.Who,
					When = actEntry.When,
					Detail = actEntry.AdditionalInfo
				};
		}

		private IEnumerable<ActEntry> assembleActEntryDTOs(ClarifyGeneric actEntryGeneric, Func<ClarifyDataRow, HistoryItemEmployee> employeeAssembler)
		{
			foreach (ClarifyDataRow actEntryRecord in actEntryGeneric.Rows)
			{
				var code = Convert.ToInt32(actEntryRecord["act_code"]);
				var template = _actEntryDefinitions[code];

				var when = Convert.ToDateTime(actEntryRecord["entry_time"]);
				var detail = actEntryRecord["addnl_info"].ToString();
				var who = employeeAssembler(actEntryRecord);

				yield return new ActEntry { Template = template, When = when, Who = who, AdditionalInfo = detail, ActEntryRecord = actEntryRecord };
			}
		}

		private void addCurrentActEntryTemplate()
		{
			if (_currentActEntryTemplate == null) 
				return;
			
			_actEntryDefinitions.Add(_currentActEntryTemplate.Code, _currentActEntryTemplate);
			_currentActEntryTemplate = null;
		}

		protected static void emailLogUpdater(ClarifyDataRow record, HistoryItem historyItem)
		{
			var detail = "Send to: {1}{0}".ToFormat(Environment.NewLine, record["recipient"]);

			var cclist = record["cc_list"].ToString();
			if (cclist.IsNotEmpty())
			{
				detail += "CC: {1}{0}".ToFormat(Environment.NewLine, cclist);
			}
			detail += record["message"].ToString();

			historyItem.Detail = detail;
		}

		protected static void timeAndExpensesUpdater(ClarifyDataRow record, HistoryItem historyItem)
		{
			var timeDescribed = TimeSpan.FromSeconds(Convert.ToInt32(record["total_time"]));
			var expense = Convert.ToDecimal(record["total_exp"]);
			var detail = "Time: {1}{0}Expense: {2}{0}{3}".ToFormat(Environment.NewLine, timeDescribed, expense.ToString("C"), record["notes"]);

			historyItem.Detail = detail;
		}

		protected static void statusChangeUpdater(ClarifyDataRow record, HistoryItem historyItem)
		{
			var notes = record["notes"].ToString();
			var notesHeader = (notes.Length > 0) ? Environment.NewLine + "Notes: " : string.Empty;
			var detail = "Status changed {0}{1}{2}".ToFormat(historyItem.Detail, notesHeader, notes);

			historyItem.Detail = detail;
		}
	}
}
