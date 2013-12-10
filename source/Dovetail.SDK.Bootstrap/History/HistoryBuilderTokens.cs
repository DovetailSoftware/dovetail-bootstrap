using FubuLocalization;

namespace Dovetail.SDK.Bootstrap.History
{
	public class HistoryBuilderTokens : StringToken
	{
		protected HistoryBuilderTokens(string key, string default_EN_US_Text) : base(key, default_EN_US_Text, null, true) { }

		public static readonly StringToken ASSIGNED = new HistoryBuilderTokens("ASSIGNED", "Assigned");
		public static readonly StringToken ACCEPTED = new HistoryBuilderTokens("ACCEPTED", "Accepted");
		public static readonly StringToken ATTACHMENT_ADDED = new HistoryBuilderTokens("ATTACHMENT_ADDED", "Attachment added");
		public static readonly StringToken ATTACHMENT_DELETED = new HistoryBuilderTokens("ATTACHMENT_DELETED", "Attachment deleted");
		public static readonly StringToken CLOSED = new HistoryBuilderTokens("CLOSED", "Closed");
		public static readonly StringToken COMMITTMENT_CREATED = new HistoryBuilderTokens("COMMITTMENT_CREATED", "Committment created");
		public static readonly StringToken COMMITTMENT_MODIFED = new HistoryBuilderTokens("COMMITTMENT_MODIFED", "Committment modified");
		public static readonly StringToken CONTACT_CHANGED = new HistoryBuilderTokens("CONTACT_CHANGED", "Contact changed");
		public static readonly StringToken CREATED = new HistoryBuilderTokens("CREATED", "Created");
		public static readonly StringToken DISPATCHED = new HistoryBuilderTokens("DISPATCHED", "Dispatched");
		public static readonly StringToken FORWARDED = new HistoryBuilderTokens("FORWARDED", "Forwarded");
		public static readonly StringToken LOG_EMAIL_IN = new HistoryBuilderTokens("LOG_EMAIL_IN", "Received email");
		public static readonly StringToken LOG_EMAIL_OUT = new HistoryBuilderTokens("LOG_EMAIL_OUT", "Sent email");
	
		public static readonly StringToken LOG_EMAIL_DATE = new HistoryBuilderTokens("LOG_EMAIL_DATE", "Date");
		public static readonly StringToken LOG_EMAIL_FROM = new HistoryBuilderTokens("LOG_EMAIL_FROM", "From");
		public static readonly StringToken LOG_EMAIL_TO = new HistoryBuilderTokens("LOG_EMAIL_TO", "To");
		public static readonly StringToken LOG_EMAIL_CC = new HistoryBuilderTokens("LOG_EMAIL_CC", "CC");
		public static readonly StringToken LOG_EMAIL_SUBJECT = new HistoryBuilderTokens("LOG_EMAIL_SUBJECT", "Subject");
		public static readonly StringToken LOG_EMAIL_SENDTO = new HistoryBuilderTokens("LOG_EMAIL_SENDTO", "Send To");
		public static readonly StringToken LOG_EMAIL_SENT = new HistoryBuilderTokens("LOG_EMAIL_SENT", "Sent");

		public static readonly StringToken INITIAL_RESPONSE = new HistoryBuilderTokens("INITIAL_RESPONSE", "Initial response");
		public static readonly StringToken LOG_EXPENSES = new HistoryBuilderTokens("LOG_EXPENSES", "Time and expenses logged");
		public static readonly StringToken LOG_EXPENSES_DETAIL = new HistoryBuilderTokens("LOG_EXPENSES_DETAIL", "Time: {1}{0}Expense: {2}{0}Notes: {3}");
		public static readonly StringToken LOG_EXPENSES_EDITTED = new HistoryBuilderTokens("LOG_EXPENSES_EDITTED", "Time and expenses edited");
		public static readonly StringToken LOG_EXPENSES_DELETED = new HistoryBuilderTokens("LOG_EXPENSES_DELETED", "Time and expenses log deleted");
		public static readonly StringToken LOG_RESEARCH = new HistoryBuilderTokens("LOG_RESEARCH", "Research note added");
		public static readonly StringToken LOG_PHONE = new HistoryBuilderTokens("LOG_PHONE", "Phone log added");
		public static readonly StringToken LOG_NOTE = new HistoryBuilderTokens("LOG_NOTE", "Note logged");
		public static readonly StringToken SOLUTION_LINKED = new HistoryBuilderTokens("SOLUTION_LINKED", "Linked to a solution");
		public static readonly StringToken SOLUTION_UNLINKED = new HistoryBuilderTokens("SOLUTION_UNLINKED", "Unlinked from the solution");
		public static readonly StringToken STATUS_CHANGE = new HistoryBuilderTokens("STATUS_CHANGE", "Status changed");
		public static readonly StringToken STATUS_CHANGE_NOTES = new HistoryBuilderTokens("STATUS_CHANGE_NOTES", "Notes:");
		public static readonly StringToken REOPENED = new HistoryBuilderTokens("REOPENED", "Reopened");
		public static readonly StringToken REJECTED = new HistoryBuilderTokens("REJECTED", "Returned to sender");
		public static readonly StringToken SUBCASE_CLOSED = new HistoryBuilderTokens("SUBCASE_CLOSED", "Subcase closed");
      public static readonly StringToken SUBCASE_CREATED = new HistoryBuilderTokens("SUBCASE_CREATED", "Subcase created");
		public static readonly StringToken SUBCASE_CREATED_ADMINISTRATIVE = new HistoryBuilderTokens("SUBCASE_CREATED_ADMINISTRATIVE", "Administrative subcase created");
		public static readonly StringToken SUBCASE_REOPENED = new HistoryBuilderTokens("SUBCASE_REOPENED", "Subcase reopened");
		public static readonly StringToken YANKED = new HistoryBuilderTokens("YANKED", "Yanked");
	}
}