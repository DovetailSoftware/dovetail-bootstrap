using System;
using FubuCore;
using StructureMap;

namespace Dovetail.SDK.ModelMap.Integration
{

	//TODO why is this required by this test container setup but not by the Web project?
	public class StructureMapServiceLocator : IServiceLocator
	{
		private readonly IContainer _container;

		public StructureMapServiceLocator(IContainer container)
		{
			_container = container;
		}

		public object GetInstance(Type type)
		{
			return _container.GetInstance(type);
		}

		public IContainer Container { get { return _container; } }


		public TService GetInstance<TService>()
		{
			return _container.GetInstance<TService>();
		}

		public TService GetInstance<TService>(string name)
		{
			return _container.GetInstance<TService>(name);
		}
	}
}