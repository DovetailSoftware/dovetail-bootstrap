using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dovetail.SDK.Bootstrap.History.Configuration
{
	public class HistorySettings
	{
		/// <summary>
		/// Comma separated Titles to detect in email headers (e.g. to, from, subject)
		/// </summary>
		public string LogEmailHeaderTitles { get; set; }


		public Regex[] OriginalMessageDetectionExpressions
		{
			get
			{
				return new[]
				{
					new Regex(@"-+\s*Original Message\s*-+", RegexOptions.IgnoreCase), 
					new Regex(@"On .*,.*wrote:", RegexOptions.IgnoreCase), 
					new Regex(@"<div style=3D'border:none;border-top:solid #B5C4DF 1.0pt;padding:3.0pt 0in =0in 0in'>", RegexOptions.IgnoreCase),
					new Regex(@"-+\s*Ursprüngliche Nachricht\s*-+", RegexOptions.IgnoreCase)
				};
			}
		}

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