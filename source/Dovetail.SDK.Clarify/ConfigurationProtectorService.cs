using System;
using FChoice.Common;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Clarify
{
	public static class ConfigurationProtectorService
	{
		private static readonly Logger Log = LogManager.GetLogger(typeof(ConfigurationProtectorService));
		private static string _entropy = null;
		private static readonly object LockObject = new object();
		private const string IsLookingMessageString = "ConfigurationProtectorService is looking for '{0}' configuration item.";
		private const string SearchResultMessageString = "ConfigurationProtectorService has {0}found entropy string.";

		private static string RetrieveEntropy(ClarifyDataSet dataSet, string entropySource)
		{
			Log.LogDebug($"ConfigurationProtectorService has not found entropy string in session cache. Searching the table_config_itm...");

			var generic = dataSet.CreateGenericWithFields("config_itm", "str_value");
			generic.Filter(f => f.Equals("name", entropySource));
			generic.Query();

			return generic.Rows.Count > 0 ? generic.Rows[0].AsString("str_value") : null;
		}

		public static void DataProtectionEntropySource(IClarifySession clarifySession, string entropySource)
		{
			lock (LockObject)
			{
				Log.LogDebug(IsLookingMessageString.ToFormat(entropySource));

				_entropy = clarifySession?.AsClarifySession().ConfigItems[entropySource]?.StringValue;

				if (string.IsNullOrEmpty(_entropy) && clarifySession != null)
				{
					var dataSet = clarifySession.CreateDataSet();

					_entropy = RetrieveEntropy(dataSet, entropySource);
				}

				Log.LogDebug(SearchResultMessageString.ToFormat((string.IsNullOrEmpty(_entropy) ? "not " : "")));
			}
		}

		public static void DataProtectionEntropySource(FChoice.Foundation.Clarify.ClarifySession clarifySession, string entropySource)
		{
			lock (LockObject)
			{
				Log.LogDebug(IsLookingMessageString.ToFormat(entropySource));

				_entropy = clarifySession?.ConfigItems[entropySource]?.StringValue;

				if (string.IsNullOrEmpty(_entropy) && clarifySession != null)
				{
					var dataSet = new ClarifyDataSet(clarifySession);

					_entropy = RetrieveEntropy(dataSet, entropySource);
				}

				Log.LogDebug(SearchResultMessageString.ToFormat((string.IsNullOrEmpty(_entropy) ? "not " : "")));
			}
		}

		public static string DecryptCredentialString(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return null;

			return input.StartsWith("FCENC:") ? DataProtector.DecryptString(DataProtectionStore.UseMachineStore, input.Substring(6), _entropy) : input;
		}
	}
}
