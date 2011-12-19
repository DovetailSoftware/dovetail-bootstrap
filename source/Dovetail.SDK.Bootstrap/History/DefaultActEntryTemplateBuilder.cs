using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.Bootstrap.History
{
    public class ChildSubcaseActEntryTemplateBuilder : ActEntryTemplateExpression 
    {
        protected override void DefineTemplate(WorkflowObject workflowObject)
        {
            this.TimeAndExpenseEdittedActEntry();
            this.StatusChangedActEntry();
            this.LogResearchActEntry();
            this.PhoneLogActEntry();
            this.NoteActEntry();
            this.TimeAndExpenseLoggedActEntry();
            this.EmailOutActEntry();
            this.EmailInActEntry();
        }
    }

    public class ARCActEntryTemplateBuilder : ActEntryTemplateExpression
    {
        protected override void DefineTemplate(WorkflowObject workflowObject)
        {
            ActEntry(10500).DisplayName("Foo")
                .EditActivityDTO(dto => { dto.Detail = "Foo " + dto.Detail; });
        }
    }

    public class DefaultActEntryTemplateBuilder : ActEntryTemplateExpression 
	{
        protected override void DefineTemplate(WorkflowObject workflowObject)
		{
			ActEntry(10500).DisplayName("Assigned")
				.EditActivityDTO(dto => { dto.Detail = "Assigned " + dto.Detail; });
			ActEntry(100).DisplayName("Accepted")
				.EditActivityDTO(dto => { dto.Detail = "Accepted " + dto.Detail; });
			ActEntry(200).DisplayName("Closed")
				.GetRelatedRecord("act_entry2close_case")
				.WithFields("summary")
				.UpdateActivityDTOWith((row, dto) =>
				{
					dto.Detail = row["summary"].ToString();
				});
			ActEntry(400).DisplayName("Committment Created");
            ActEntry(600).DisplayName("Created");
			ActEntry(900).DisplayName("Dispatched")
				.EditActivityDTO(dto => { dto.Detail = "Dispatched " + dto.Detail; });
			ActEntry(1100).DisplayName("Forwarded");
			ActEntry(1600).DisplayName("Committment Modified");
            ActEntry(2400).DisplayName("Reopened")
				.EditActivityDTO(dto => { dto.Detail = "Reopened " + dto.Detail; });
			ActEntry(2600).DisplayName("Return To Sender");
            ActEntry(4100).DisplayName("Yanked");
			ActEntry(4200).DisplayName("Subcase Reopened");
			ActEntry(7200).DisplayName("Administrative Subcase Created");

            //TODO add policy for attachment adds for: Seeker attachment downloads, url rewriting, plain
            ActEntry(8900).DisplayName("Attachment Added"); //.HtmlizeWith(item => { }).UpdateActivityDTOWith((row, item) => _attachmentPathHistoryItemUpdater.Update(row["addnl_info"].ToString(), item));

            ActEntry(9100).DisplayName("Attachment Deleted");
            ActEntry(9800).DisplayName("Contact Changed");
            ActEntry(1400).DisplayName("Linked");
            ActEntry(4000).DisplayName("Unlinked");
            ActEntry(9200).DisplayName("Initial Response");

            this.TimeAndExpenseEdittedActEntry();
            this.StatusChangedActEntry();
            this.LogResearchActEntry();
            this.PhoneLogActEntry();
            this.NoteActEntry();
            this.TimeAndExpenseLoggedActEntry();
            this.EmailOutActEntry();
            this.EmailInActEntry();
			
			if (workflowObject.Type == WorkflowObject.Case)
				DefineCaseSpecificActEntries();

            if (workflowObject.Type == WorkflowObject.Subcase)
				DefineSubcaseSpecificActEntries();
		}

        private void DefineCaseSpecificActEntries()
		{
			ActEntry(3000).DisplayName("Subcase Created");
			ActEntry(3100).DisplayName("Subcase Closed");
		}

		private void DefineSubcaseSpecificActEntries()
		{
			ActEntry(3000).DisplayName("Subcase Created")
				.GetRelatedRecord("act_entry2notes_log")
				.WithFields("description")
				.UpdateActivityDTOWith((record, dto) =>
				{
					dto.Detail = record["description"].ToString();
				});
			ActEntry(3100).DisplayName("Subcase Closed")
				.GetRelatedRecord("act_entry2close_case")
				.WithFields("summary")
				.UpdateActivityDTOWith((row, dto) =>
				{
					dto.Detail = row["summary"].ToString();
				});
		}
	}
}
