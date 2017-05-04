using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public class TransformContext
	{
		private readonly IServiceLocator _services;

		public TransformContext(ModelData model, TransformArguments arguments, IServiceLocator services)
		{
			Model = model;
			Arguments = arguments;
			_services = services;
		}

		public ModelData Model { get; private set; }
		public TransformArguments Arguments { get; private set; }

		public TService Service<TService>()
		{
			return _services.GetInstance<TService>();
		}
	}
}