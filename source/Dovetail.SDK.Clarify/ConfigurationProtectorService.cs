using System;
using FChoice.Common;
using FubuCore;

namespace Dovetail.SDK.Clarify
{
	public static class ConfigurationProtectorService
	{
		private static readonly Logger Log = LogManager.GetLogger(typeof(ConfigurationProtectorService));
		private static string _entropy = null;
		private static readonly object LockObject = new object();

		public static void DataProtectionEntropySource(IClarifySession clarifySession, string entropySource)
		{
			Log.LogDebug($"ConfigurationProtectorService is looking for '{entropySource}' configuration item.");

			_entropy = clarifySession?.AsClarifySession().ConfigItems[entropySource]?.StringValue;

			lock (LockObject)
			{
				if (string.IsNullOrEmpty(_entropy) && clarifySession != null)
				{
					Log.LogDebug($"ConfigurationProtectorService has not found entropy string in session cache. Searching the table_config_itm...");

					var dataSet = clarifySession.CreateDataSet();
					var generic = dataSet.CreateGenericWithFields("config_itm", "str_value");
					generic.Filter(f => f.Equals("name", entropySource));
					generic.Query();

					_entropy = generic.Rows.Count > 0 ? generic.Rows[0].AsString("str_value") : null;
				}
			}

			Log.LogDebug($"ConfigurationProtectorService has {(string.IsNullOrEmpty(_entropy) ? "not " : "")}found entropy string.");
		}

		public static string DecryptCredentialString(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return null;

			return input.StartsWith("FCENC:") ? DataProtector.DecryptString(DataProtectionStore.UseMachineStore, input.Substring(6), _entropy) : input;
		}
	}
}
