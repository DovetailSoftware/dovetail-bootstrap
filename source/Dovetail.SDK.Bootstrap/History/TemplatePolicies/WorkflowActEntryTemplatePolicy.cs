using Dovetail.SDK.Bootstrap.History.Configuration;

namespace Dovetail.SDK.Bootstrap.History.TemplatePolicies
{
    public class WorkflowActEntryTemplatePolicy : ActEntryTemplatePolicyExpression 
	{
        protected override void DefineTemplate(WorkflowObject workflowObject)
		{
            //child object histories are not a concern of this policy
            if (workflowObject.IsChild) return;

            //typical workflow object policies

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
			ActEntry(400).DisplayName("Committment created");
            ActEntry(600).DisplayName("Created");
			ActEntry(900).DisplayName("Dispatched")
				.EditActivityDTO(dto => { dto.Detail = "Dispatched " + dto.Detail; });
			ActEntry(1100).DisplayName("Forwarded");
			ActEntry(1600).DisplayName("Committment modified");
            ActEntry(2400).DisplayName("Reopened")
				.EditActivityDTO(dto => { dto.Detail = "Reopened " + dto.Detail; });
			ActEntry(2600).DisplayName("Returned to sender");
            ActEntry(4100).DisplayName("Yanked");
			ActEntry(4200).DisplayName("Subcase reopened");
			ActEntry(7200).DisplayName("Administrative subcase created");

            //TODO add policy for attachment adds for: Seeker attachment downloads, url rewriting, plain
            ActEntry(8900).DisplayName("Attachment added"); //.HtmlizeWith(item => { }).UpdateActivityDTOWith((row, item) => _attachmentPathHistoryItemUpdater.Update(row["addnl_info"].ToString(), item));

            ActEntry(9100).DisplayName("Attachment deleted");
            ActEntry(9800).DisplayName("Contact changed");
            ActEntry(1400).DisplayName("Linked to a solution");
            ActEntry(4000).DisplayName("Unlinked from the solution");
            ActEntry(9200).DisplayName("Initial response");

            ActEntry(3000).DisplayName("Subcase created");
            ActEntry(3100).DisplayName("Subcase closed");
                
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
}
