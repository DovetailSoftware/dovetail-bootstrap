using System;
using System.Collections.Generic;

namespace Dovetail.SDK.Bootstrap.History
{
    public class HistoryItem
	{
		public int Id { get; set; }
		public DateTime When { get; set; }
		public string Kind { get; set; }
		public string Detail { get; set; }
		public string Internal { get; set; }
		public HistoryItemEmployee Who { get; set; }
	}

	public class HistoryItemEmployee
	{
		public int Id { get; set; }
		public string Login { get; set; }
		public string Name { get; set; }
	}
	public class HistoryViewModel 
	{
	    public WorkflowObject WorkflowObject { get; set; }
	    public IEnumerable<HistoryItem> HistoryItems { get; set; }
	}
}