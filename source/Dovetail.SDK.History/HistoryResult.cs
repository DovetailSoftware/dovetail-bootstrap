using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.ModelMap;

namespace Dovetail.SDK.History
{
	public class HistoryResult
	{
		public ModelData[] Items { get; set; }
		public DateTime? Since { get; set; }
		public DateTime? NextTimestamp { get; set; }
		public int TotalResults { get; set; }
		public int HistoryItemLimit { get; set; }

		public static DateTime? DetermineNextTimestamp(HistoryRequest request, ActEntryResolution entries)
		{
			return request.HistoryItemLimit > entries.Ids.Count()
				? null
				: entries.LastTimestamp;
		}

		public IDictionary<string, object> ToValues()
		{
			return new Dictionary<string, object>
			{
				{ "since", Since },
				{ "nextTimestamp", NextTimestamp },
				{ "totalResults", TotalResults },
				{ "historyItemLimit", HistoryItemLimit },
				{ "items", Items.Select(_ => _.ToValues()) },
			};
		}
	}
}
