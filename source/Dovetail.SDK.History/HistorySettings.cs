using System;
using System.IO;

namespace Dovetail.SDK.History
{
	public class HistorySettings
	{
		public HistorySettings()
		{
			Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "History");
			MergeCaseHistoryChildSubcases = true;

			HistoryConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.config");
		}

		public string Directory { get; set; }
		public bool EnableCache { get; set; }
		public string HistoryConfigPath { get; set; }

		public bool MergeCaseHistoryChildSubcases { get; set; }
	}
}
