using System;
using System.Timers;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Configuration;
using Dovetail.SDK.ModelMap.Registration;
using StructureMap;
using Topshelf;
using Topshelf.Runtime;

namespace Service
{
	public class BootstrapService : ServiceControl
	{
		private readonly HostSettings _settings;
		private Container _container;
		private CaseMonitor _monitor;

		public BootstrapService(HostSettings settings)
		{
			_settings = settings;
		}

		public bool Start(HostControl hostControl)
		{
			hostControl.RequestAdditionalTime(TimeSpan.FromSeconds(10));

			_container = createContainer();
			_monitor = _container.With(_settings).GetInstance<CaseMonitor>();

			_monitor.Start();

			return true;
		}


		public bool Stop(HostControl hostControl)
		{
			_monitor.Stop();
			return true;
		}

		public bool Pause(HostControl hostControl)
		{
			return Stop(hostControl);
		}

		public bool Continue(HostControl hostControl)
		{
			return Start(hostControl);
		}

		private static Container createContainer()
		{
			var container = new Container(cfg =>
				{
					cfg.AddRegistry<SettingsProviderRegistry>();
					cfg.AddRegistry<BootstrapRegistry>();
					cfg.AddRegistry<ModelMapperRegistry>();

					cfg.Scan(s =>
						{
							s.TheCallingAssembly();
							s.WithDefaultConventions();
							s.AddAllTypesOf(typeof(ModelMap<>));
						});

					//this is only here because there is a current bug in StructureMap where add all types of a open generic don't work
					cfg.For<ModelMap<RecentCaseModel>>().Use<RecentCaseMap>();
				});
			return container;
		}
	}

	public class CaseMonitor 
	{
		private readonly HostSettings _settings;
		private readonly ILogger _logger;
		private readonly ISystemTime _systemTime;
		private readonly IModelBuilder<RecentCaseModel> _caseModelBuilder;
		private DateTime _lastPolled;
		private TimeSpan _interval;
		private Timer _timer;
		private IDisposable _loggingContext;

		public CaseMonitor(HostSettings settings,  ILogger logger, ISystemTime systemTime, IModelBuilder<RecentCaseModel> caseModelBuilder)
		{
			_settings = settings;
			_logger = logger;
			_systemTime = systemTime;
			_caseModelBuilder = caseModelBuilder;
			_interval = TimeSpan.FromSeconds(15);
			_lastPolled = systemTime.Now.Subtract(_interval);
		}

		public bool Start()
		{
			//The logging context will have the instance of this windows service just in case there are multiple services. 
			//This is done just to show a scenario for consuming the host settings object we get from Topshelf.
			_loggingContext = _logger.Push(_settings.InstanceName);

			//create a timer in charge of doing the recurreing polling for open cases
			_timer = new Timer(_interval.TotalMilliseconds);
			_timer.Elapsed += pollForOpenCases;
			_timer.Start();

			return true;
		}
		
		private void pollForOpenCases(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			var from = _lastPolled;
			var to = _systemTime.Now;

			//using a ModelMap to project cases created between the last poll and now
			var cases = _caseModelBuilder.Get(f => f.Between("creation_time", from, to));
			
			_logger.LogInfo("{0} cases were created in the last {1} seconds", cases.Length, to.Subtract(from).TotalSeconds);

			foreach (var kase in cases)
			{
				//the debug log will have more details
				_logger.LogDebug(kase.ToString());
			}

			_lastPolled = to;
		}

		public bool Stop()
		{
			_timer.Stop();
			_loggingContext.Dispose();
			return true;
		}
	}

	//Defining a model map which tells the model builder how to project your query 
	//into RecentCaseModel objects. ModelMaps helps us avoid having to directly use ClarifyGenerics
	//This blog post is slightly out of date but gives an introduction to ModelMaps (then called DovetailMaps)
	//http://www.dovetailsoftware.com/blogs/kmiller/archive/2009/04/27/introducing-dovetail-datamap
	public class RecentCaseMap : ModelMap<RecentCaseModel>
	{
		protected override void MapDefinition()
		{
			FromView("qry_case_view")
				.Assign(d => d.Id).FromIdentifyingField("id_number")
				.Assign(d => d.Title).FromIdentifyingField("title");
		}
	}

	//The object which we'll be populating using ModelMap.
	public class RecentCaseModel
	{
		public string Title { get; set; }
		public string Id { get; set; }

		public override string ToString()
		{
			return String.Format("Case {0} with title {1}", Id, Title);
		}
	}
}