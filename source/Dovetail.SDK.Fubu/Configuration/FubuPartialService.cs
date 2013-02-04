using System;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Registration.Querying;
using FubuMVC.Core.Runtime;

namespace Dovetail.SDK.Fubu.Configuration
{
	public interface IFubuPartialService
	{
		string GetText(Type requestType);
		void Invoke(Type requestType);
	}

	//TODO add end to end test for this service
	public class FubuPartialService : IFubuPartialService
	{
		private readonly IPartialFactory _factory;
		private readonly IOutputWriter _writer;
		private readonly IChainResolver _chainResolver;

		public FubuPartialService(IPartialFactory factory, IOutputWriter writer, IChainResolver chainResolver)
		{
			_factory = factory;
			_writer = writer;
			_chainResolver = chainResolver;
		}

		public string GetText(Type requestType)
		{
			var partial = buildPartial(requestType);

			return _writer.Record(partial.InvokePartial).GetText();
		}

		public void Invoke(Type requestType)
		{
			var partial = buildPartial(requestType);

			partial.InvokePartial();
		}

		private IActionBehavior buildPartial(Type requestType)
		{
			var chain = _chainResolver.FindUniqueByType(requestType);
			var partial = _factory.BuildPartial(chain);
			return partial;
		}
	}
}