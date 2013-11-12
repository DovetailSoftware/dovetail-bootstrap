using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FubuCore;
using FubuLocalization;
using FubuLocalization.Basic;

namespace Dovetail.SDK.Bootstrap.Configuration
{
	//TODO remove this when this issue is addressed https://github.com/DarthFubuMVC/FubuMVC.Localization/issues/5 
	public class BootstrapLocalizationMissingHandler : ILocalizationMissingHandler
	{
		private readonly ILocalizationStorage _storage;
		private readonly CultureInfo _defaultCulture;
		private readonly ILogger _logger;

		public BootstrapLocalizationMissingHandler(ILocalizationStorage storage, CultureInfo defaultCulture, ILogger logger)
		{
			_storage = storage;
			_defaultCulture = defaultCulture;
			_logger = logger;
		}

		public string FindMissingText(StringToken key, CultureInfo culture)
		{
			var defaultValue = culture.Name + "_" + key.ToLocalizationKey();
			if (key.DefaultValue.IsNotEmpty() && culture.Equals(_defaultCulture))
			{
				defaultValue = key.DefaultValue;
			}

			writeMissing(key.ToLocalizationKey().ToString(), defaultValue, culture);

			return defaultValue;
		}

		public string FindMissingProperty(PropertyToken property, CultureInfo culture)
		{
			var defaultValue = culture.Equals(_defaultCulture)
				? property.Header ?? property.DefaultHeaderText(culture) ?? BreakUpCamelCase(property.PropertyName)
				: property.DefaultHeaderText(culture) ?? culture.Name + "_" + property.PropertyName;

			writeMissing(property.StringTokenKey, defaultValue, culture);

			return defaultValue;
		}

		private void writeMissing(string key, string defaultValue, CultureInfo culture)
		{
			try
			{
				_storage.WriteMissing(key, defaultValue, culture);
			}
			catch (Exception e)
			{
				_logger.LogDebug("Could not write missing key {0} to xml storage. Continuing with the default value {1} Exception: {2}", key, defaultValue, e);
			}
		}

		public static string BreakUpCamelCase(string fieldName)
		{
			var patterns = new[]
			{
				"([a-z])([A-Z])",
				"([0-9])([a-zA-Z])",
				"([a-zA-Z])([0-9])"
			};
			var output = patterns.Aggregate(fieldName,
				(current, pattern) => Regex.Replace(current, pattern, "$1 $2", RegexOptions.IgnorePatternWhitespace));
			return output.Replace('_', ' ');
		}
	}
}