using System;
using FChoice.Common;
using FubuCore;

namespace Dovetail.SDK.Clarify
{
	public static class ConfigurationProtectorService
	{
		private static readonly Logger Log = LogManager.GetLogger(typeof(ConfigurationProtectorService));
		private static string _entropy = null;

		public static void DataProtectionEntropySource(IClarifySession clarifySession, string entropySource)
		{
			Log.LogDebug($"ConfigurationProtectorService is looking for '{entropySource}' configuration item.");

			_entropy = clarifySession?.AsClarifySession().ConfigItems[entropySource]?.StringValue;

			Log.LogDebug($"ConfigurationProtectorService has {(string.IsNullOrEmpty(_entropy) ? "not " : "")}found entropy string.");
		}

		public static string DecryptCredentialString(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return null;

			return input.StartsWith("FCENC:") ? DataProtector.DecryptString(DataProtectionStore.UseMachineStore, input.Substring(6), _entropy) : input;
		}
	}
}
