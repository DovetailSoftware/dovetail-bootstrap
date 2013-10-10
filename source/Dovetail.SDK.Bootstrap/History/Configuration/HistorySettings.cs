using System;
using System.Linq;

namespace Dovetail.SDK.Bootstrap.History.Configuration
{
	public class HistorySettings
	{
		/// <summary>
		/// Comma separated Titles to detect in email headers (e.g. to, from, subject)
		/// </summary>
		public string LogEmailHeaderTitles { get; set; }

		public string[] LogEmailHeaders
		{
			get
			{
				return LogEmailHeaderTitles.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(s=>s.Trim()).ToArray();
			}
		}

		/// <summary>
		/// Should history events concerning a case's subcases show up in case history
		/// </summary>
		public bool MergeCaseHistoryChildSubcases { get; set; }

		public HistorySettings()
		{
			LogEmailHeaderTitles = "date, from, to, send to, cc, subject, sent";
		}
	}
}