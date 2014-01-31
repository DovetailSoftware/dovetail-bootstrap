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

			if (configSection == null)
			{
				_logger.LogDebug("Could not find a configuration section named '{0}'. Using default original message expressions.".ToFormat(ConfigSectionName));
				return DefaultOriginalMessageExpressions;
			}

			return configSection.AllKeys.Select(k =>
			{
				var expression = configSection[k];
				return new Regex(expression);
			});
		}

		public virtual NameValueCollection getConfiguationSection()
		{
			var section = ConfigurationManager.GetSection(ConfigSectionName);

			if (section == null) return null;

			return (NameValueCollection) section;
		}
	}
}