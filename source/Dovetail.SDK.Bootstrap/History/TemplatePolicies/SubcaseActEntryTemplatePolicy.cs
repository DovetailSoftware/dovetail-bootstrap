using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;
using FChoice.Foundation.Schema;

namespace Dovetail.SDK.Bootstrap.History.TemplatePolicies
{
	public class SubcaseActEntryTemplatePolicy : ActEntryTemplatePolicyExpression
	{
		private readonly HistorySettings _settings;
		private readonly ISchemaCache _schemaCache;
		private readonly IHistoryOutputParser _parser;

		public SubcaseActEntryTemplatePolicy(HistorySettings settings, ISchemaCache schemaCache, IHistoryOutputParser parser) : base(parser)
		{
			_settings = settings;
			_schemaCache = schemaCache;
			_parser = parser;
		}

		protected override void DefineTemplate(WorkflowObject workflowObject)
		{
			//kill subcase create and close for case objects when merging subcase history
			if (_settings.MergeCaseHistoryChildSubcases && workflowObject.Type == WorkflowObject.Case)
			{
				ActEntry(3000).Remove();
				ActEntry(3100).Remove();
			}

			//the rest of this policy is subcase specific 
			if (workflowObject.Type != WorkflowObject.Subcase) return;

			//make subcase creation and close more detailed.
			ActEntry(3000).DisplayName(HistoryBuilderTokens.SUBCASE_CREATED)
				.GetRelatedRecord("act_entry2notes_log")
				.WithFields("description")
				.UpdateActivityDTOWith((record, dto) => { dto.Detail = record.AsString("description"); });

			ActEntry(3100).DisplayName(HistoryBuilderTokens.SUBCASE_CLOSED)
				.GetRelatedRecord("act_entry2close_case")
				.WithFields("summary")
				.UpdateActivityDTOWith((record, dto) => { dto.Detail = record.AsString("summary"); });

			//templates specific to subcases which are child histories
			if (!workflowObject.IsChild) return;

			//for subcase histories which are a child of another history show limited details

			this.TimeAndExpenseEdittedActEntry();
			this.StatusChangedActEntry();
			this.LogResearchActEntry();
			this.PhoneLogActEntry(_schemaCache);
			this.NoteActEntry(_schemaCache);
			this.TimeAndExpenseLoggedActEntry();
			this.TimeAndExpenseLoggedDeletedActEntry();
			this.EmailOutActEntry(_schemaCache, _parser);
			this.EmailInActEntry(_schemaCache, _parser);
		}
	}
}