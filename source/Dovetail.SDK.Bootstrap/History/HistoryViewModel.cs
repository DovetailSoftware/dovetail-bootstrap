using System;

namespace Dovetail.SDK.Bootstrap.History
{
    public class HistoryItem
	{
        /// <summary>
        /// Object Id of the item.
        /// </summary>
		public string Id { get; set; }

        /// <summary>
        /// Type (case, subcase, solution) of the item.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// When the activity entry was logged.
        /// </summary>
		public DateTime When { get; set; }
        
        /// <summary>
        /// Placeholder for describing the history when field. If this is desired it should be populated downstream from Bootstrap. This field is NOT populated by Dovetail Bootstrap. 
        /// </summary>
        public string WhenDescribed { get; set; }
        
        /// <summary>
        /// Title of the item. Likely populated fromt he DSL's DisplayName.
        /// </summary>
		public string Title { get; set; }

        /// <summary>
        /// Public details of the item.
        /// </summary>
		public string Detail { get; set; }

        /// <summary>
        /// Internal item details. Not to be displayed to the end user.
        /// </summary>
        /// 
		public string Internal { get; set; }
        
        /// <summary>
        /// User account which logged the activity
        /// </summary>
		public HistoryItemEmployee Who { get; set; }
	}

	public class HistoryItemContact
	{
		public int Id { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }
		public dynamic Custom { get; set; }
	}

	public class HistoryItemEmployee
	{
		public HistoryItemContact PerformedByContact { get; set; }
		public int Id { get; set; }
		public string Login { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }
		public dynamic Custom { get; set; }
	}

	public class HistoryViewModel 
	{
	    public WorkflowObject WorkflowObject { get; set; }
		public HistoryItem[] HistoryItems { get; set; }
	}
}