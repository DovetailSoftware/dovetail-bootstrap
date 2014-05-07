using System;
using System.Linq;
using FubuLocalization;

namespace Dovetail.SDK.Bootstrap.History.Configuration
{
	public class HistorySettings
	{
		public bool UseDovetailSDKCompatibileAttachmentFinder { get; set; }

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