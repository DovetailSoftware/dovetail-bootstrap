using System;
using Dovetail.SDK.ModelMap;

namespace Dovetail.SDK.History
{
	public class HistoryResult
	{
		public ModelData[] Items { get; set; }
		public DateTime? Since { get; set; }
		public int TotalResults { get; set; }
		public int HistoryItemLimit { get; set; }
	}
}