using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using FChoice.Common.Data;
using FChoice.Foundation;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.DataObjects;
using FChoice.Foundation.Schema;
using StructureMap;

namespace Dovetail.SDK.Clarify
{
    public class ClarifyContext : IClarifyContext
    {
        private static readonly object SyncRoot = new object();
        private readonly ClarifyApplication _clarifyApplication;
        private readonly DovetailCRMSettings _settings;
        private readonly IContainer _container;
        private readonly ILogger _logger;

        public ClarifyContext(DovetailCRMSettings settings, IContainer container, ILogger logger)
        {
            _settings = settings;
            _container = container;
            _logger = logger;
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

        private ClarifyApplication InitializeClarifyApplication()
        {
            lock (SyncRoot)
            {
                return FCApplication.IsInitialized
                           ? ClarifyApplication.Instance
                           : initializeClarify();
            }
        }

        private ClarifyApplication initializeClarify()
        {
            var configuration = GetDovetailSdkConfiguration(_settings);
            DbProviderFactory.Provider = DbProviderFactory.CreateProvider(_settings.DatabaseType);

            var settings = new StringBuilder();
            foreach (var key in configuration.AllKeys)
            {
                settings.AppendLine(string.Format("{0}={1}", key, configuration[key]));
            }

            _logger.LogDebug("Initializing Clarify with settings: {0}", settings.ToString());

            return ClarifyApplication.Initialize(configuration);
        }

        private static NameValueCollection GetDovetailSdkConfiguration(DovetailCRMSettings settings)
        {
            var configuration = new NameValueCollection
            {
                {"fchoice.dbtype", settings.DatabaseType},
                {"fchoice.connectionstring", settings.DatabaseConnectionString},
                {"fchoice.disableloginfromfcapp", "false"},
                {"fchoice.sessionpasswordrequired", "false"},
                {"fchoice.nocachefile", "true"}
            };

            return Merge(configuration, ConfigurationManager.AppSettings);
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