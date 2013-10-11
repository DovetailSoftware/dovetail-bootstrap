using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History
{
	public static class CommonActEntryBuilderDSLExtensions
	{
		public static void TimeAndExpenseEdittedActEntry(this ActEntryTemplatePolicyExpression dsl)
		{
			dsl.ActEntry(8700).DisplayName(HistoryBuilderTokens.LOG_EXPENSES_EDITTED);
		}

		public static void StatusChangedActEntry(this ActEntryTemplatePolicyExpression dsl)
		{
			dsl.ActEntry(300).DisplayName(HistoryBuilderTokens.STATUS_CHANGE)
				.GetRelatedRecord("act_entry2status_chg")
				.WithFields("notes")
				.UpdateActivityDTOWith(statusChangeUpdater);
		}

		private static void statusChangeUpdater(ClarifyDataRow record, HistoryItem historyItem)
		{
			var notes = record["notes"].ToString();
			var notesHeader = (notes.Length > 0) ? Environment.NewLine + "Notes: " : String.Empty;
			var detail = "{0} {1}{2}{3}".ToFormat(HistoryBuilderTokens.STATUS_CHANGE, historyItem.Detail, notesHeader, notes);

			historyItem.Detail = detail;
		}

		public static void LogResearchActEntry(this ActEntryTemplatePolicyExpression dsl)
		{
			dsl.ActEntry(2500).DisplayName(HistoryBuilderTokens.LOG_RESEARCH)
				.GetRelatedRecord("act_entry2resrch_log")
				.WithFields("notes", "internal")
				.UpdateActivityDTOWith((row, dto) =>
				{
					dto.Detail = row["notes"].ToString();
					dto.Internal = row["internal"].ToString();
				});
		}

		public static void PhoneLogActEntry(this ActEntryTemplatePolicyExpression dsl, ISchemaCache schemaCache)
		{
			const string noteField = "notes";

			if (schemaCache.IsValidField("phone_log", "x_is_internal"))
			{
				dsl.ActEntry(500).DisplayName(HistoryBuilderTokens.LOG_PHONE)
					.GetRelatedRecord("act_entry2phone_log")
					.WithFields(noteField, "internal", "x_is_internal")
					.UpdateActivityDTOWith((row, dto) => setInternalLog(row, dto, noteField));
				return;
			}

			dsl.ActEntry(500).DisplayName(HistoryBuilderTokens.LOG_PHONE)
				.GetRelatedRecord("act_entry2phone_log")
				.WithFields(noteField, "internal")
				.UpdateActivityDTOWith((row, dto) =>
				{
					dto.Detail = row["notes"].ToString();
					dto.Internal = row["internal"].ToString();
				});
		}

		public static void NoteActEntry(this ActEntryTemplatePolicyExpression dsl, ISchemaCache schemaCache)
		{
			const string noteField = "description";

			if (schemaCache.IsValidField("notes_log", "x_is_internal"))
			{
				dsl.ActEntry(1700).DisplayName(HistoryBuilderTokens.LOG_NOTE)
					.GetRelatedRecord("act_entry2notes_log")
					.WithFields(noteField, "internal", "x_is_internal")
					.UpdateActivityDTOWith((row, dto) => setInternalLog(row, dto, noteField));
				return;
			}

			dsl.ActEntry(1700).DisplayName(HistoryBuilderTokens.LOG_NOTE)
				.GetRelatedRecord("act_entry2notes_log")
				.WithFields(noteField, "internal")
				.UpdateActivityDTOWith((row, dto) =>
				{
					dto.Detail = row[noteField].ToString();
					dto.Internal = row["internal"].ToString();
				});
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
			dsl.ActEntry(1800).DisplayName(HistoryBuilderTokens.LOG_EXPENSES)
				.GetRelatedRecord("act_entry2onsite_log")
				.WithFields("total_time", "total_exp", "notes", "internal_note")
				.UpdateActivityDTOWith(timeAndExpensesUpdater);
		}

		private static void timeAndExpensesUpdater(ClarifyDataRow record, HistoryItem historyItem)
		{
			var timeDescribed = TimeSpan.FromSeconds(record.AsInt("total_time"));
			var expense = Convert.ToDecimal(record["total_exp"]).ToString("C");
			var notes = record.AsString("notes");
			var detail = HistoryBuilderTokens.LOG_EXPENSES_DETAIL.ToFormat(Environment.NewLine, timeDescribed, expense, notes);
			historyItem.Detail = detail;
		}

		public static void TimeAndExpenseLoggedDeletedActEntry(this ActEntryTemplatePolicyExpression dsl)
		{
			dsl.ActEntry(10600).DisplayName(HistoryBuilderTokens.LOG_EXPENSES_DELETED)
				.GetRelatedRecord("act_entry2onsite_log")
				.WithFields("total_time", "total_exp", "notes", "internal_note")
				.UpdateActivityDTOWith(timeAndExpensesUpdater);
		}


		public static void EmailInActEntry(this ActEntryTemplatePolicyExpression dsl, ISchemaCache schemaCache, IHistoryOutputParser historyOutputParser)
		{
			dsl.ActEntry(3500).DisplayName(HistoryBuilderTokens.LOG_EMAIL_IN)
				.HtmlizeWith(item => EncodeEmail(historyOutputParser, item))
				.GetRelatedRecord("act_entry2email_log")
				.WithFields(getEmailLogFields(schemaCache))
				.UpdateActivityDTOWith((record, historyItem) => emailLogUpdater(record, historyItem, schemaCache));
		}

		private static void EncodeEmail(IHistoryOutputParser historyOutputParser, HistoryItem item)
		{
			item.Detail = historyOutputParser.EncodeEmailLog(item.Detail);
			item.Internal = historyOutputParser.Encode(item.Internal);
		}

		public static void EmailOutActEntry(this ActEntryTemplatePolicyExpression dsl, ISchemaCache schemaCache, IHistoryOutputParser historyOutputParser)
		{
			dsl.ActEntry(3400).DisplayName(HistoryBuilderTokens.LOG_EMAIL_OUT)
				.HtmlizeWith(item => EncodeEmail(historyOutputParser, item))
				.GetRelatedRecord("act_entry2email_log")
				.WithFields(getEmailLogFields(schemaCache))
				.UpdateActivityDTOWith((record, historyItem) => emailLogUpdater(record, historyItem, schemaCache));
		}

		private static string[] getEmailLogFields(ISchemaCache schemaCache)
		{
			string[] emailLogFields = {"message", "sender", "recipient", "cc_list", "creation_time"};
			
			if (doesEmailLogSubjectExist(schemaCache))
			{
				emailLogFields = emailLogFields.Concat(new [] {"x_subject"}).ToArray();
			}

			return emailLogFields;
		}

		private static bool doesEmailLogSubjectExist(ISchemaCache schemaCache)
		{
			return schemaCache.IsValidField("email_log", "x_subject");
		}

		private static void emailLogUpdater(ClarifyDataRow record, HistoryItem historyItem, ISchemaCache schemaCache)
		{
			var log = new StringBuilder();

			var from = record.AsString("sender");
			var date = record.AsDateTime("creation_time");
			var to = record.AsString("recipient");
			var cclist = record.AsString("cc_list");
			var subject = doesEmailLogSubjectExist(schemaCache) ? record.AsString("x_subject") : "";
			var message = record.AsString("message");

			log.Append(HistoryParsers.BEGIN_EMAIL_LOG_HEADER);
			
			log.AppendLine(HistoryBuilderTokens.LOG_EMAIL_FROM.ToFormat(from));
			log.AppendLine(HistoryBuilderTokens.LOG_EMAIL_DATE.ToFormat(date));
			log.AppendLine(HistoryBuilderTokens.LOG_EMAIL_TO.ToFormat(to));
			if (cclist.IsNotEmpty()) log.AppendLine(HistoryBuilderTokens.LOG_EMAIL_CC.ToFormat(cclist));
			if (subject.IsNotEmpty()) log.AppendLine(HistoryBuilderTokens.LOG_EMAIL_SUBJECT.ToFormat(subject));

			log.Append(HistoryParsers.END_EMAIL_LOG_HEADER);

			log.AppendLine(message);

			historyItem.Detail = log.ToString();
		}

		public static void ForwardActEntry(this ActEntryTemplatePolicyExpression dsl)
		{
			dsl.ActEntry(1100).DisplayName(HistoryBuilderTokens.FORWARDED)
				.GetRelatedRecord("act_entry2reject_msg")
				.WithFields("description")
				.UpdateActivityDTOWith((row, dto) =>
				{
					dto.Detail += Environment.NewLine + row.AsString("description");
				});
		}

		public static void RejectActEntry(this ActEntryTemplatePolicyExpression dsl)
		{
			dsl.ActEntry(2600).DisplayName(HistoryBuilderTokens.REJECTED)
				.GetRelatedRecord("act_entry2reject_msg")
				.WithFields("description")
				.UpdateActivityDTOWith((row, dto) =>
				{
					dto.Detail += Environment.NewLine + row.AsString("description");
				});
		}
	}
}