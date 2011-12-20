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
		
        //public ClarifyGeneric RelatedGeneric { get; set; }
        public string RelatedGenericRelation { get; set; }
        public string[] RelatedGenericFields{ get; set; }

        public Action<HistoryItem> ActivityDTOEditor { get; set; }
		public Action<HistoryItem> HTMLizer { get; set; }
	}

	public class ActEntry
	{
	    public string Type { get; set; }
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

	public abstract class ActEntryTemplatePolicyExpression : IAfterActEntryCode, IAfterDisplayName, IHasRelatedRow, IAfterRelatedFields, IAfterHtmlizer
	{
		private readonly IDictionary<int, ActEntryTemplate> _actEntryDefinitions = new Dictionary<int, ActEntryTemplate>();
		private ActEntryTemplate _currentActEntryTemplate;
		
	    public IDictionary<int, ActEntryTemplate> ActEntryDefinitions
	    {
	        get { return _actEntryDefinitions; }
	    }

	    protected abstract void DefineTemplate(WorkflowObject workflowObject);

        public IDictionary<int, ActEntryTemplate> RenderTemplate(WorkflowObject workflowObject)
        {
            resetExpression();

            DefineTemplate(workflowObject);

            return ActEntryDefinitions;
        }

	    private void resetExpression()
	    {
	        _actEntryDefinitions.Clear();
	        _currentActEntryTemplate = null;
	    }

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
			//_currentActEntryTemplate.RelatedGeneric = ActEntryGeneric.Traverse(relationName);
		    _currentActEntryTemplate.RelatedGenericRelation = relationName;

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

			_currentActEntryTemplate.RelatedGenericFields = fieldNames;

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
			if(_currentActEntryTemplate.RelatedGenericRelation.IsEmpty())
				throw new Exception("Cannot add fields unless a record is related. First call GetRelatedRecord()");
		}

		private void addCurrentActEntryTemplate()
		{
			if (_currentActEntryTemplate == null) 
				return;
			
			ActEntryDefinitions.Add(_currentActEntryTemplate.Code, _currentActEntryTemplate);
			_currentActEntryTemplate = null;
		}
	}
}
