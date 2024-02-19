using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FChoice.Common.Data;
using FChoice.Common.State;
using FChoice.Foundation;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.DataObjects;
using FChoice.Foundation.Schema;
using FubuCore;
using StructureMap;

namespace Dovetail.SDK.Clarify
{
    public class ClarifyContext : IClarifyContext
    {
        private static readonly object SyncRoot = new object();
        private readonly ClarifyApplication _clarifyApplication;
        private readonly DovetailDatabaseSettings _settings;
        private readonly DovetailCRMSettings _crmSettings;
        private readonly IContainer _container;
        private readonly ILogger _logger;

        public ClarifyContext(DovetailDatabaseSettings settings, IContainer container, ILogger logger, DovetailCRMSettings crmSettings)
        {
            _settings = settings;
            _container = container;
            _logger = logger;
            _crmSettings = crmSettings;
            _clarifyApplication = InitializeClarifyApplication();
        }

        public ClarifyApplication Application
        {
            get { return _clarifyApplication; }
        }

        public ISchemaCache SchemaCache
        {
            get { return _clarifyApplication.SchemaCache; }
        }

        public ILocaleCache LocaleCache
        {
            get { return _clarifyApplication.LocaleCache; }
        }

        public ITimeZone ServerTimeZone
        {
            get { return _clarifyApplication.ServerTimeZone; }
        }

        public IClarifySession CreateSession()
        {
            var session = _clarifyApplication.CreateSession();
            var manager = _container.GetInstance<IClarifySessionManager>();
            return new ClarifySessionAdapter(session, manager);
        }

        public IClarifySession CreateSession(string userName, ClarifyLoginType loginType)
        {
            var session = _clarifyApplication.CreateSession(userName, loginType);
            var manager = _container.GetInstance<IClarifySessionManager>();
            return new ClarifySessionAdapter(session, manager);
        }

        private ClarifyApplication InitializeClarifyApplication()
        {
            lock (SyncRoot)
            {
                return FCApplication.IsInitialized
                           ? ClarifyApplication.Instance
                           : InitializeClarify();
            }
        }

        private ClarifyApplication InitializeClarify()
        {
            var configuration = GetDovetailSdkConfiguration(_settings, _crmSettings);
            DbProviderFactory.Provider = DbProviderFactory.CreateProvider(_settings.Type);

            var settings = new StringBuilder();
            foreach (var key in configuration.AllKeys)
            {
                var configString = $"{key} = {(key.Contains("connectionstring") ? Regex.Replace(configuration[key], "(.*)((password|pwd)=)([^;]+)(.*)", "$1$2*********$5", RegexOptions.Compiled | RegexOptions.IgnoreCase) : configuration[key])}";
                settings.AppendLine($"{key}={configString}");
            }

            _logger.LogDebug("Initializing Clarify with settings:\n{0}", settings.ToString());

            var application = ClarifyApplication.Initialize(configuration);

            SetSessionDefaultTimeout(_settings);

            return application;
        }

        private static NameValueCollection GetDovetailSdkConfiguration(DovetailDatabaseSettings settings, DovetailCRMSettings crmSettings)
        {
            var configuration = new NameValueCollection
            {
                {"fchoice.dbtype", settings.Type},
                {"fchoice.connectionstring", settings.ConnectionString},
                {"fchoice.disableloginfromfcapp", "false"},
                {"fchoice.sessionpasswordrequired", "false"},
                {"fchoice.nocachefile", "true"}
            };

            var source = configuration;
            if (crmSettings.DatabaseConnectionString.IsNotEmpty())
            {
                source = new NameValueCollection
                {
                    {"fchoice.dbtype", crmSettings.DatabaseType},
                    {"fchoice.connectionstring", crmSettings.DatabaseConnectionString},
                    {"fchoice.disableloginfromfcapp", "false"},
                    {"fchoice.sessionpasswordrequired", "false"},
                    {"fchoice.nocachefile", "true"}
                };
            }

            return Merge(source, ConfigurationManager.AppSettings);
        }

        private void SetSessionDefaultTimeout(DovetailDatabaseSettings dovetailDatabaseSettings)
        {
            var stateTimeoutTimespan = TimeSpan.FromMinutes(dovetailDatabaseSettings.SessionTimeoutInMinutes);

            _logger.LogDebug("Setting session time out to be {0} minutes long.", stateTimeoutTimespan);

            StateManager.StateTimeout = stateTimeoutTimespan;
        }

        public static NameValueCollection Merge(NameValueCollection target, NameValueCollection source)
        {
            var result = new NameValueCollection(target);

            foreach (var settingKey in source.AllKeys.Where(settingKey => settingKey.StartsWith("fchoice.")))
            {
                result[settingKey] = source[settingKey];
            }

            return result;
        }
    }
}
