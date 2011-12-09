using System;
using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Extensions;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History
{
    public class ActEntryTemplate
	{
		public ActEntryTemplate()
		{
            //TODO make this come from an injectable type? - kjm 10/2011
		    HTMLizer = item =>
		                   {
		                       item.Detail = item.Detail.HtmlEncode().ToHtml();
                               item.Internal = item.Internal.HtmlEncode().ToHtml();
		                   };
		}

		public int Code { get; set; }
		public string DisplayName { get; set; }
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

	public abstract class ActEntryTemplateExpression : IAfterActEntryCode, IAfterDisplayName, IHasRelatedRow, IAfterRelatedFields, IAfterHtmlizer
	{
		private readonly IDictionary<int, ActEntryTemplate> _actEntryDefinitions = new Dictionary<int, ActEntryTemplate>();
		private ActEntryTemplate _currentActEntryTemplate;
		public ClarifyGeneric ActEntryGeneric { get; set; }

	    public IDictionary<int, ActEntryTemplate> ActEntryDefinitions
	    {
	        get { return _actEntryDefinitions; }
	    }

	    public abstract IDictionary<int, ActEntryTemplate> BuildTemplates(WorkflowObject workflowObject, ClarifyGeneric actEntryGeneric);

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

		private void validateThereIsARelatedRecord()
		{
			if(_currentActEntryTemplate.RelatedGeneric == null)
				throw new Exception("Cannot add fields unless a record is related. First call GetRelatedRecord()");
		}

		private void addCurrentActEntryTemplate()
		{
			if (_currentActEntryTemplate == null) 
				return;
			
			ActEntryDefinitions.Add(_currentActEntryTemplate.Code, _currentActEntryTemplate);
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
