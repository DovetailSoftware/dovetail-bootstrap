using System;
using System.Linq;
using System.Text.RegularExpressions;
using FubuLocalization;

namespace Dovetail.SDK.Bootstrap.History.Configuration
{
	public class HistorySettings
	{
		private StringToken[] _logEmailHeaderTokens;
		
		public HistorySettings()
		{
			_logEmailHeaderTokens = new[]
			{
				HistoryBuilderTokens.LOG_EMAIL_DATE,
				HistoryBuilderTokens.LOG_EMAIL_FROM,
				HistoryBuilderTokens.LOG_EMAIL_TO,
				HistoryBuilderTokens.LOG_EMAIL_CC,
				HistoryBuilderTokens.LOG_EMAIL_SUBJECT,
				HistoryBuilderTokens.LOG_EMAIL_SENDTO,
				HistoryBuilderTokens.LOG_EMAIL_SENT
			};
		}

		public StringToken[] GetLogEmailHeaderTokens()
		{
			return _logEmailHeaderTokens;
		}

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

		public string LogEmailHeaderKeys
		{
			get
			{
				return String.Join(",", _logEmailHeaderTokens.Select(t => t.Key));
			}
			set
			{
				var keys = value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
				_logEmailHeaderTokens = keys.Select(StringToken.FromKeyString).ToArray();
			}
		}

		/// <summary>
		/// Should history events concerning a case's subcases show up in case history
		/// </summary>
		public bool MergeCaseHistoryChildSubcases { get; set; }
	}
}