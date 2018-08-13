using System;
using FubuCore;

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
		public bool EntryTimeExclusive { get; set; }
		public bool FindRepeatingTimestamp { get; set; }

		public int SqlLimit()
		{
			return FindRepeatingTimestamp ? int.MaxValue : HistoryItemLimit;
		}

		public string EntryTimeArg()
		{
			var @operator = ReverseOrder ? ">" : "<";
			if (!EntryTimeExclusive)
			{
				@operator += "=";
			}

			if (FindRepeatingTimestamp)
				@operator = "=";

			return Since.HasValue
				? " AND entry_time {0} '{1}'".ToFormat(@operator, Since.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"))
				: "";
		}

		public string SortOrder()
		{
			return ReverseOrder ? "ASC" : "DESC";
		}
	}
}
