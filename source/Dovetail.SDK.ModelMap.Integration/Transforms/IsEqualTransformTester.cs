using System.Collections.Generic;
using Dovetail.SDK.ModelMap.Transforms;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.Transforms
{
	[TestFixture]
	public class IsEqualTransformTester
	{
		[Test]
		public void evaluates_equality()
		{
			var services = new InMemoryServiceLocator();
			services.Add<IMappingVariableExpander>(new MappingVariableExpander(new MappingVariableRegistry(new List<IMappingVariableSource>()), services));
			var arguments = new TransformArguments(services, new Dictionary<string, object>
			{
				{ "field", "status" },
				{ "value", "0" }
			});

			var data = new ModelData();
			data["status"] = 0;

			var context = new TransformContext(data, arguments, new InMemoryServiceLocator());
			var transform = new IsEqualTransform();

			transform.Execute(context).ShouldEqual(true);

			data["status"] = 1;
			transform.Execute(context).ShouldEqual(false);
		}
	}
}