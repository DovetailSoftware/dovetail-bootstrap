using System.Collections.Generic;
using Dovetail.SDK.ModelMap.NewStuff;
using Dovetail.SDK.ModelMap.NewStuff.Transforms;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff.Transforms
{
	[TestFixture]
	public class IsEqualTransformTester
	{
		[Test]
		public void evaluates_equality()
		{
			var arguments = new TransformArguments(new Dictionary<string, object>
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