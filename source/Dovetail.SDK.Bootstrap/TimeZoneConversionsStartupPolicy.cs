using Dovetail.SDK.Bootstrap.Clarify.Metadata;
using FChoice.Foundation;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap
{
	public class TimeZoneConversionsStartupPolicy : IStartupPolicy
	{
		private readonly ISchemaMetadataCache _metadata;
		private readonly ILogger _logger;

		public TimeZoneConversionsStartupPolicy(ISchemaMetadataCache metadata, ILogger logger)
		{
			_metadata = metadata;
			_logger = logger;
		}

		public void Execute()
		{
			if (!FCApplication.IsInitialized)
			{
				_logger.LogWarn("FCApplication not initialized");
				return;
			}

			foreach (var table in _metadata.Tables)
			{
				foreach (var field in table.Fields)
				{
					_logger.LogDebug("Excluding column from time zone conversion: {0}.{1}".ToFormat(table.Name, field.Name));
					ClarifyApplication.Instance.TimeZoneConversions.Exclude(table.Name, field.Name);
				}
			}
		}
	}
}
