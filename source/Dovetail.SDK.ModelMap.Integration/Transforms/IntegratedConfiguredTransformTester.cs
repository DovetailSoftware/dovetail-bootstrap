using System.Collections.Generic;
using Dovetail.SDK.ModelMap.Transforms;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.Transforms
{
	[TestFixture]
	public class IntegratedConfiguredTransformTester
	{
		[Test]
		public void sets_the_value_from_a_value_argument()
		{
			var transform = new StubTransform();
			var services = new InMemoryServiceLocator();
			services.Add<ISimpleService>(new SimpleService());
			services.Add<IMappingVariableExpander>(new MappingVariableExpander(new MappingVariableRegistry(new List<IMappingVariableSource>()), services));

			var path = ModelDataPath.Parse("child.grandChild.property");

			var data = new ModelData();
			data["child"] = new ModelData();
			data.Child("child")["grandChild"] = new ModelData();

			var arguments = new List<ITransformArgument>
			{
				new ValueArgument("foo", "bar")
			};

			var configuredTransform = new ConfiguredTransform(path, transform, arguments, new MappingVariableExpander(new MappingVariableRegistry(new List<IMappingVariableSource>()), services), services);
			configuredTransform.Execute(data, services);

			data.Child("child").Child("grandChild").Get<string>("property").ShouldEqual("BAR");
		}

		[Test]
		public void sets_the_value_from_a_field_argument()
		{
			var transform = new StubTransform();
			var services = new InMemoryServiceLocator();
			services.Add<ISimpleService>(new SimpleService());
			services.Add<IMappingVariableExpander>(new MappingVariableExpander(new MappingVariableRegistry(new List<IMappingVariableSource>()), services));

			var path = ModelDataPath.Parse("child.grandChild.property");

			var data = new ModelData();
			data["child"] = new ModelData();
			data["test"] = "testing";
			data.Child("child")["grandChild"] = new ModelData();

			var arguments = new List<ITransformArgument>
			{
				new FieldArgument("foo", ModelDataPath.Parse("test"))
			};

			var configuredTransform = new ConfiguredTransform(path, transform, arguments, new MappingVariableExpander(new MappingVariableRegistry(new List<IMappingVariableSource>()), services), services);
			configuredTransform.Execute(data, services);

			data.Child("child").Child("grandChild").Get<string>("property").ShouldEqual("TESTING");
		}

		public interface ISimpleService
		{
			string ToUpper(string value);
		}

		public class SimpleService : ISimpleService
		{
			public string ToUpper(string value)
			{
				return value.ToUpper();
			}
		}

		public class StubTransform : IMappingTransform
		{
			public object Execute(TransformContext context)
			{
				var service = context.Service<ISimpleService>();
				var arg = context.Arguments.Get<string>("foo");

				return service.ToUpper(arg);
			}
		}
	}
}