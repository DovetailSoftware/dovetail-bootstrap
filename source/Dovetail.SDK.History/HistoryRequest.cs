using System;

namespace Dovetail.SDK.History
{
	public class HistoryRequest
	{
		public WorkflowObject WorkflowObject { get; set; }
		public bool ShowAllActivities { get; set; }
		public int PageSize { get; set; }
		public DateTime? Since { get; set; }
		public bool ReverseOrder { get; set; }
		public int HistoryItemLimit { get; set; }
	}
}