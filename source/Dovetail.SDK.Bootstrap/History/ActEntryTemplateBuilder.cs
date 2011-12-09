using System.Collections.Generic;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.Bootstrap.History
{
    public class ActEntryTemplateBuilder : ActEntryTemplateExpression //, IHistoryActEntryBuilder
	{
        public override IDictionary<int, ActEntryTemplate> BuildTemplates(WorkflowObject workflowObject, ClarifyGeneric actEntryGeneric)
		{
            ActEntryGeneric = actEntryGeneric;

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

            TimeAndExpenseEdittedActEntry();
            StatusChangedActEntry();
            LogResearchActEntry();
            PhoneLogActEntry();
            NoteActEntry();
            TimeAndExpenseLoggedActEntry();
            EmailOutActEntry();
            EmailInActEntry();
			
			if (workflowObject.Type == WorkflowObject.Case)
				DefineCaseSpecificActEntries();

            if (workflowObject.Type == WorkflowObject.Subcase)
				DefineSubcaseSpecificActEntries();

		    return ActEntryDefinitions;
		}

        private void DefineNestedSubcaseActivities()
        {
            TimeAndExpenseEdittedActEntry();
            StatusChangedActEntry();
            LogResearchActEntry();
            PhoneLogActEntry();
            NoteActEntry();
            TimeAndExpenseLoggedActEntry();
            EmailOutActEntry();
            EmailInActEntry();
        }

	    private void StatusChangedActEntry()
	    {
	        ActEntry(300).DisplayName("Status Changed")
	            .GetRelatedRecord("act_entry2status_chg")
	            .WithFields("notes")
	            .UpdateActivityDTOWith(statusChangeUpdater);
	    }

	    private void LogResearchActEntry()
	    {
	        ActEntry(2500).DisplayName("Research Note")
	            .GetRelatedRecord("act_entry2resrch_log")
	            .WithFields("notes", "internal")
	            .UpdateActivityDTOWith((row, dto) =>
	                                       {
	                                           dto.Detail = row["notes"].ToString();
	                                           dto.Internal = row["internal"].ToString();
	                                       });
	    }

	    private void TimeAndExpenseEdittedActEntry()
	    {
	        ActEntry(8700).DisplayName("Time and Expense Edited");
	    }

	    private void TimeAndExpenseLoggedActEntry()
	    {
	        ActEntry(1800).DisplayName("Time and Expense Logged")
	            .GetRelatedRecord("act_entry2onsite_log")
	            .WithFields("total_time", "total_exp", "notes", "internal_note")
	            .UpdateActivityDTOWith(timeAndExpensesUpdater);
	    }

	    private void EmailInActEntry()
	    {
	        ActEntry(3500).DisplayName("Email In")
	            .GetRelatedRecord("act_entry2email_log")
	            .WithFields("message", "recipient", "cc_list")
	            .UpdateActivityDTOWith(emailLogUpdater);
	    }

	    private void EmailOutActEntry()
	    {
	        ActEntry(3400).DisplayName("Email Out")
	            .GetRelatedRecord("act_entry2email_log")
	            .WithFields("message", "recipient", "cc_list")
	            .UpdateActivityDTOWith(emailLogUpdater);
	    }

	    private void PhoneLogActEntry()
	    {
	        ActEntry(500).DisplayName("Phone Log")
	            .GetRelatedRecord("act_entry2phone_log")
	            .WithFields("notes", "internal")
	            .UpdateActivityDTOWith((row, dto) =>
	                                       {
	                                           dto.Detail = row["notes"].ToString();
	                                           dto.Internal = row["internal"].ToString();
	                                       });
	    }

	    private void NoteActEntry()
	    {
	        ActEntry(1700).DisplayName("Note")
	            .GetRelatedRecord("act_entry2notes_log").WithFields("description", "internal")
	            .UpdateActivityDTOWith((record, dto) =>
	                                       {
	                                           dto.Detail = record["description"].ToString();
	                                           dto.Internal = record["internal"].ToString();
	                                       });
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
