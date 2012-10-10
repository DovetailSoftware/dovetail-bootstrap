using System;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.History.Configuration;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History
{
    public static class CommonActEntryBuilderDSLExtensions
    {
        public static void TimeAndExpenseEdittedActEntry(this ActEntryTemplatePolicyExpression dsl)
        {
            dsl.ActEntry(8700).DisplayName("Time and expenses edited");
        }

        public static void StatusChangedActEntry(this ActEntryTemplatePolicyExpression dsl)
        {
            dsl.ActEntry(300).DisplayName("Status changed")
                .GetRelatedRecord("act_entry2status_chg")
                .WithFields("notes")
                .UpdateActivityDTOWith(statusChangeUpdater);
        }

        private static void statusChangeUpdater(ClarifyDataRow record, HistoryItem historyItem)
        {
            var notes = record["notes"].ToString();
            var notesHeader = (notes.Length > 0) ? Environment.NewLine + "Notes: " : String.Empty;
            var detail = "Status changed {0}{1}{2}".ToFormat(historyItem.Detail, notesHeader, notes);

            historyItem.Detail = detail;
        }

        public static void LogResearchActEntry(this ActEntryTemplatePolicyExpression dsl)
        {
            dsl.ActEntry(2500).DisplayName("Research note added")
                .GetRelatedRecord("act_entry2resrch_log")
                .WithFields("notes", "internal")
                .UpdateActivityDTOWith((row, dto) =>
                                           {
                                               dto.Detail = row["notes"].ToString();
                                               dto.Internal = row["internal"].ToString();
                                           });
        }

	    public static void PhoneLogActEntry(this ActEntryTemplatePolicyExpression dsl)
      {
        var noteField = "notes";
        dsl.ActEntry(500).DisplayName("Phone log added")
          .GetRelatedRecord("act_entry2phone_log")
          .WithFields(noteField, "internal", "x_is_internal")
          .UpdateActivityDTOWith((row, dto) => setInternalLog(row, dto, noteField));
      }

	    public static void NoteActEntry(this ActEntryTemplatePolicyExpression dsl)
	    {
	      var noteField = "description";
	      dsl.ActEntry(1700).DisplayName("Note logged")
			    .GetRelatedRecord("act_entry2notes_log")
          .WithFields(noteField, "internal", "x_is_internal")
			    .UpdateActivityDTOWith((row, dto) => setInternalLog(row, dto, noteField));
	    }

      private static void setInternalLog(ClarifyDataRow row, HistoryItem dto, string noteField)
	    {
			  var isNewInternalNote = row.AsInt("x_is_internal") == 1;
        var notes = row.AsString(noteField);
	      var @internal = row.AsString("internal");
			
			  dto.Detail = isNewInternalNote ? "" : notes;
		    dto.Internal = isNewInternalNote ? notes : @internal;
	    }

	    public static void TimeAndExpenseLoggedActEntry(this ActEntryTemplatePolicyExpression dsl)
        {
            dsl.ActEntry(1800).DisplayName("Time and expense logged")
                .GetRelatedRecord("act_entry2onsite_log")
                .WithFields("total_time", "total_exp", "notes", "internal_note")
                .UpdateActivityDTOWith(timeAndExpensesUpdater);
        }

        private static void timeAndExpensesUpdater(ClarifyDataRow record, HistoryItem historyItem)
        {
            var timeDescribed = TimeSpan.FromSeconds(Convert.ToInt32(record["total_time"]));
            var expense = Convert.ToDecimal(record["total_exp"]);
            var detail = "Time: {1}{0}Expense: {2}{0}{3}".ToFormat(Environment.NewLine, timeDescribed, expense.ToString("C"), record["notes"]);

            historyItem.Detail = detail;
        }

        public static void TimeAndExpenseLoggedDeletedActEntry(this ActEntryTemplatePolicyExpression dsl)
        {
            dsl.ActEntry(10600).DisplayName("Time and expense log deleted")
                .GetRelatedRecord("act_entry2onsite_log")
                .WithFields("total_time", "total_exp", "notes", "internal_note")
                .UpdateActivityDTOWith(timeAndExpensesUpdater);
        }

        public static void EmailInActEntry(this ActEntryTemplatePolicyExpression dsl)
        {
            dsl.ActEntry(3500).DisplayName("Received email")
                .GetRelatedRecord("act_entry2email_log")
                .WithFields("message", "recipient", "cc_list")
                .UpdateActivityDTOWith(emailLogUpdater);
        }

        public static void EmailOutActEntry(this ActEntryTemplatePolicyExpression dsl)
        {
            dsl.ActEntry(3400).DisplayName("Sent email")
                .GetRelatedRecord("act_entry2email_log")
                .WithFields("message", "recipient", "cc_list")
                .UpdateActivityDTOWith(emailLogUpdater);
        }

        private static void emailLogUpdater(ClarifyDataRow record, HistoryItem historyItem)
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
    }
}