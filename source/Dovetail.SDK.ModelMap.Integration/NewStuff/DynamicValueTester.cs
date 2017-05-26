using Dovetail.SDK.ModelMap.NewStuff;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff
{
	[TestFixture]
	public class DynamicValueTester
	{
		[Test]
		public void resolves_the_value()
		{
			var value = new DynamicValue("${foo}");
			var services = new InMemoryServiceLocator();
			services.Add<IMappingVariableExpander>(new StubExpander());

			value.Resolve(services).ShouldEqual("bar");
		}

		private class StubExpander : IMappingVariableExpander
		{
			public bool IsVariable(string value)
			{
				return true;
			}

			public object Expand(string value)
			{
				return "bar";
			}

			public void PushContext(VariableExpanderContext context)
			{
			}

			public void PopContext()
			{
			}
		}
	}
}