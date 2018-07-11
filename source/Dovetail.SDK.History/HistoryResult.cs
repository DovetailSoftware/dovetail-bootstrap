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
		public int TotalResults { get; set; }
		public int HistoryItemLimit { get; set; }

		public IDictionary<string, object> ToValues()
		{
			return new Dictionary<string, object>
			{
				{ "since", Since },
				{ "totalResults", TotalResults },
				{ "historyItemLimit", HistoryItemLimit },
				{ "items", Items.Select(_ => _.ToValues()) },
			};
		}
	}
}
