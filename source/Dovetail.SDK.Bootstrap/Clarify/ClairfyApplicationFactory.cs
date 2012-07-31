using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using FChoice.Common.State;
using FChoice.Foundation;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.Bootstrap.Clarify
{
    public class DovetailDatabaseSettings 
    {
    	public DovetailDatabaseSettings()
    	{
    		ApplicationUsername = "sa";
    	}

    	public string Type { get; set; }
        public string ConnectionString { get; set; }
        public double SessionTimeoutInMinutes { get; set; }
    	public string ApplicationUsername { get; set; }
    }

    public interface IClarifyApplicationFactory
    {
        IClarifyApplication Create();
    }

    public class ClarifyApplicationFactory : IClarifyApplicationFactory
    {
        private readonly DovetailDatabaseSettings _dovetailDatabaseSettings;
        private static readonly object SyncRoot = new object();

        public ClarifyApplicationFactory(DovetailDatabaseSettings dovetailDatabaseSettings)
        {
            _dovetailDatabaseSettings = dovetailDatabaseSettings;
        }

        public IClarifyApplication Create()
        {
            if (FCApplication.IsInitialized)
            {
                return ClarifyApplication.Instance;
            }
            lock (SyncRoot)
            {
                if (FCApplication.IsInitialized)
                {
                    return ClarifyApplication.Instance;
                }

                var configuration = GetDovetailSDKConfiguration(_dovetailDatabaseSettings);

                var application = ClarifyApplication.Initialize(configuration);

                setSessionDefaultTimeout(_dovetailDatabaseSettings);

                return application;
            }
        }

        public static void setSessionDefaultTimeout(DovetailDatabaseSettings dovetailDatabaseSettings)
        {
            StateManager.StateTimeout = TimeSpan.FromMinutes(dovetailDatabaseSettings.SessionTimeoutInMinutes);
        }

        public static NameValueCollection GetDovetailSDKConfiguration(DovetailDatabaseSettings dovetailDatabaseSettings)
        {
            var configuration = new NameValueCollection
			                    {
			                    	{"fchoice.dbtype", dovetailDatabaseSettings.Type},
			                    	{"fchoice.connectionstring", dovetailDatabaseSettings.ConnectionString},
			                    	{"fchoice.disableloginfromfcapp", "false"},
									{"fchoice.sessionpasswordrequired", "false"},
			                    	{"fchoice.nocachefile", "true"}
			                    };

            return MergeSDKSettings(configuration, ConfigurationManager.AppSettings);
        }

        public static NameValueCollection MergeSDKSettings(NameValueCollection target, NameValueCollection source)
        {
            var result = new NameValueCollection(target);

            foreach (var settingKey in source.AllKeys.Where(settingKey => settingKey.StartsWith("fchoice.")).Where(settingKey => target[settingKey] == null))
            {
                result.Add(settingKey, source[settingKey]);
            }

            return result;
        }
    }
}