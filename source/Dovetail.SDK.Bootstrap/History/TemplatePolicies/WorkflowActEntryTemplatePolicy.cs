using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History.TemplatePolicies
{
	public class WorkflowActEntryTemplatePolicy : ActEntryTemplatePolicyExpression
	{
		private readonly ISchemaCache _schemaCache;
		private readonly IHistoryOutputParser _parser;

		public WorkflowActEntryTemplatePolicy(ISchemaCache schemaCache, IHistoryOutputParser parser) : base(parser)
		{
			_schemaCache = schemaCache;
			_parser = parser;
		}

		protected override void DefineTemplate(WorkflowObject workflowObject)
		{
			//child object histories are not a concern of this policy
			if (workflowObject.IsChild) return;

			//typical workflow object policies

			ActEntry(10500).DisplayName(HistoryBuilderTokens.ASSIGNED)
				.EditActivityDTO(dto => { dto.Detail = "{0} {1}".ToFormat(HistoryBuilderTokens.ASSIGNED, dto.Detail); });
			ActEntry(100).DisplayName(HistoryBuilderTokens.ACCEPTED)
				.EditActivityDTO(dto => { dto.Detail = "{0} {1}".ToFormat(HistoryBuilderTokens.ACCEPTED, dto.Detail); });
			ActEntry(200).DisplayName(HistoryBuilderTokens.CLOSED)
				.GetRelatedRecord("act_entry2close_case")
				.WithFields("summary")
				.UpdateActivityDTOWith((row, dto) => { dto.Detail = row["summary"].ToString(); });
			ActEntry(400).DisplayName(HistoryBuilderTokens.COMMITTMENT_CREATED);
			ActEntry(600).DisplayName(HistoryBuilderTokens.CREATED);
			ActEntry(900).DisplayName(HistoryBuilderTokens.DISPATCHED)
				.EditActivityDTO(dto => { dto.Detail = "{0} {1}".ToFormat(HistoryBuilderTokens.DISPATCHED, dto.Detail); });
			ActEntry(1600).DisplayName(HistoryBuilderTokens.COMMITTMENT_MODIFED);
			ActEntry(2400).DisplayName(HistoryBuilderTokens.REOPENED)
				.EditActivityDTO(dto => { dto.Detail = "{0} {1}".ToFormat(HistoryBuilderTokens.REOPENED, dto.Detail); });
			ActEntry(4100).DisplayName(HistoryBuilderTokens.YANKED);
			ActEntry(4200).DisplayName(HistoryBuilderTokens.SUBCASE_REOPENED);
			ActEntry(7200).DisplayName(HistoryBuilderTokens.SUBCASE_CREATED_ADMINISTRATIVE);

			//TODO add policy for attachment adds for: Seeker attachment downloads, url rewriting, plain
			ActEntry(8900).DisplayName(HistoryBuilderTokens.ATTACHMENT_ADDED); //.HtmlizeWith(item => { }).UpdateActivityDTOWith((row, item) => _attachmentPathHistoryItemUpdater.Update(row["addnl_info"].ToString(), item));

			ActEntry(9100).DisplayName(HistoryBuilderTokens.ATTACHMENT_DELETED);
			ActEntry(9800).DisplayName(HistoryBuilderTokens.CONTACT_CHANGED);
			ActEntry(1400).DisplayName(HistoryBuilderTokens.SOLUTION_LINKED);
			ActEntry(4000).DisplayName(HistoryBuilderTokens.SOLUTION_UNLINKED);
			ActEntry(9200).DisplayName(HistoryBuilderTokens.INITIAL_RESPONSE);

			ActEntry(3000).DisplayName(HistoryBuilderTokens.SUBCASE_CREATED);
			ActEntry(3100).DisplayName(HistoryBuilderTokens.SUBCASE_CLOSED);

			this.TimeAndExpenseEdittedActEntry();
			this.StatusChangedActEntry();
			this.LogResearchActEntry();
			this.PhoneLogActEntry(_schemaCache);
			this.NoteActEntry(_schemaCache);
			this.TimeAndExpenseLoggedActEntry();
			this.TimeAndExpenseLoggedDeletedActEntry();
			this.EmailOutActEntry(_schemaCache, _parser);
			this.EmailInActEntry(_schemaCache, _parser);
			this.ForwardActEntry();
			this.RejectActEntry();
		}
	}
}