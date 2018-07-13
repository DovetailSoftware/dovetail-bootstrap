using FubuCore;

namespace Dovetail.SDK.History.Conditions
{
	public class ActEntryConditionContext
	{
		private readonly WorkflowObject _workflowObject;
		private readonly IServiceLocator _services;

		public ActEntryConditionContext(WorkflowObject workflowObject, IServiceLocator services)
		{
			_workflowObject = workflowObject;
			_services = services;
		}

		public WorkflowObject WorkflowObject
		{
			get { return _workflowObject; }
		}

		public HistorySettings Settings
		{
			get { return Service<HistorySettings>(); }
		}

		public IServiceLocator Services
		{
			get {  return _services; }
		}

		public TService Service<TService>()
		{
			return _services.GetInstance<TService>();
		}
	}
}
