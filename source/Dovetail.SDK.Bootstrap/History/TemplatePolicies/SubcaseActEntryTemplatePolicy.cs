using Dovetail.SDK.Bootstrap.History.Configuration;

namespace Dovetail.SDK.Bootstrap.History.TemplatePolicies
{
    public class SubcaseActEntryTemplatePolicy : ActEntryTemplatePolicyExpression
    {
        private readonly HistorySettings _settings;

        public SubcaseActEntryTemplatePolicy(HistorySettings settings)
        {
            _settings = settings;
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
            ActEntry(3000).DisplayName("Subcase created")
                .GetRelatedRecord("act_entry2notes_log")
                .WithFields("description")
                .UpdateActivityDTOWith((record, dto) =>
                                           {
                                               dto.Detail = record["description"].ToString();
                                           });
            
            ActEntry(3100).DisplayName("Subcase closed")
                .GetRelatedRecord("act_entry2close_case")
                .WithFields("summary")
                .UpdateActivityDTOWith((row, dto) =>
                                           {
                                               dto.Detail = row["summary"].ToString();
                                           });

            //templates specific to subcases which are child histories
            if (!workflowObject.IsChild) return;

            //for subcase histories which are a child of another history show limited details

            this.TimeAndExpenseEdittedActEntry();
            this.StatusChangedActEntry();
            this.LogResearchActEntry();
            this.PhoneLogActEntry();
            this.NoteActEntry();
            this.TimeAndExpenseLoggedActEntry();
            this.TimeAndExpenseLoggedDeletedActEntry();
            this.EmailOutActEntry();
            this.EmailInActEntry();
        }
    }
}