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
		public void evaluates_equality_of_int_and_string()
		{
			var services = new InMemoryServiceLocator();
			services.Add<IMappingVariableExpander>(new MappingVariableExpander(new MappingVariableRegistry(new List<IMappingVariableSource>()), services));
			var arguments = new TransformArguments(services, new Dictionary<string, object>
			{
				{ "field", "status" },
				{ "value", "0" }
			});

			var data = new ModelData();

			var context = new TransformContext(data, arguments, new InMemoryServiceLocator());
			var transform = new IsEqualTransform();

			data["status"] = 0;
			transform.Execute(context).ShouldEqual(true);

			data["status"] = 1;
			transform.Execute(context).ShouldEqual(false);

			arguments = new TransformArguments(services, new Dictionary<string, object>
			{
				{ "field", "status" },
				{ "value", 0 }
			});

			context = new TransformContext(data, arguments, new InMemoryServiceLocator());

			data["status"] = "0";
			transform.Execute(context).ShouldEqual(true);

			data["status"] = "1";
			transform.Execute(context).ShouldEqual(false);
		}

		[Test]
		public void evaluates_equality_of_int_and_bool()
		{
			var services = new InMemoryServiceLocator();
			services.Add<IMappingVariableExpander>(new MappingVariableExpander(new MappingVariableRegistry(new List<IMappingVariableSource>()), services));
			var arguments = new TransformArguments(services, new Dictionary<string, object>
			{
				{ "field", "status" },
				{ "value", true }
			});

			var data = new ModelData();

			var context = new TransformContext(data, arguments, new InMemoryServiceLocator());
			var transform = new IsEqualTransform();

			data["status"] = 1;
			transform.Execute(context).ShouldEqual(true);

			data["status"] = 0;
			transform.Execute(context).ShouldEqual(false);

			arguments = new TransformArguments(services, new Dictionary<string, object>
			{
				{ "field", "status" },
				{ "value", 1 }
			});

			context = new TransformContext(data, arguments, new InMemoryServiceLocator());

			data["status"] = true;
			transform.Execute(context).ShouldEqual(true);

			data["status"] = false;
			transform.Execute(context).ShouldEqual(false);
		}
	}
}