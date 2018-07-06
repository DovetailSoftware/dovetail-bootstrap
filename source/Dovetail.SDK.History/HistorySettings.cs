using System;
using System.IO;

namespace Dovetail.SDK.History
{
	public class HistorySettings
	{
		public HistorySettings()
		{
			Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "History");
		}

		public string Directory { get; set; }
		public bool EnableCache { get; set; }

		public bool MergeCaseHistoryChildSubcases { get; set; }
	}
}