using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History.Configuration
{
	public interface IHistoryOriginalMessageConfiguration
	{
		IEnumerable<Regex> Expressions { get; }
	}

	public class HistoryOriginalMessageConfiguration : IHistoryOriginalMessageConfiguration
	{
		public static readonly Regex[] DefaultOriginalMessageExpressions =
		{
			new Regex(@"(?i)-+\s*Original Message\s*-+"),
			new Regex(@"(?i)On .*,.*wrote:"),
			new Regex(@"(?i)<div style=3D'border:none;border-top:solid #B5C4DF 1.0pt;padding:3.0pt 0in =0in 0in'>"),
			new Regex(@"(?i)-+\s*Ursprüngliche Nachricht\s*-+")
		};

		public const string ConfigSectionName = "HistoryOriginalMessageExpressions";

		private readonly ILogger _logger;

		public HistoryOriginalMessageConfiguration(ILogger logger)
		{
			_logger = logger;
		}

		private IEnumerable<Regex> _expressions;
		public IEnumerable<Regex> Expressions
		{
			get { return _expressions ?? (_expressions = getExpressions().ToList()); }
		}

		private IEnumerable<Regex> getExpressions()
		{
			var configSection = getConfiguationSection();
			IEnumerable<Regex> result;
			if (configSection == null)
			{
				_logger.LogDebug("Could not find a configuration section named '{0}'. Using default original message expressions.".ToFormat(ConfigSectionName));
				result = DefaultOriginalMessageExpressions;
			}
			else
			{
				result = configSection.AllKeys.Select(k =>
				{
					var expression = configSection[k];
					return new Regex(expression);
				}).ToArray();
			}
			logExpressions(result);
			return result;
		}

		private void logExpressions(IEnumerable<Regex> expressions)
		{
			var list = expressions.ToList();
			var expressionText = list.Select(e => e.ToString()).Join("\n");

			_logger.LogDebug("Found {0} regular expressions for original message delimitter detection. Expressions:\n{1}".ToFormat(list.Count, expressionText));
		}

		public virtual NameValueCollection getConfiguationSection()
		{
			var section = ConfigurationManager.GetSection(ConfigSectionName);

			if (section == null) return null;

			return (NameValueCollection) section;
		}
	}
}