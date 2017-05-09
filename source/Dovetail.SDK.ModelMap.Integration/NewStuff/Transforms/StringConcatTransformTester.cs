using System.Collections.Generic;
using Dovetail.SDK.ModelMap.NewStuff;
using Dovetail.SDK.ModelMap.NewStuff.Transforms;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff.Transforms
{
	[TestFixture]
	public class StringConcatTransformTester
	{
		[Test]
		public void concats_the_strings()
		{
			var arguments = new TransformArguments(new Dictionary<string, object>
			{
				{ "arg1", "Hello, " },
				{ "arg2", "World!" }
			});

			var context = new TransformContext(new ModelData(), arguments, new InMemoryServiceLocator());
			var transform = new StringConcatTransform();

			transform.Execute(context).ShouldEqual("Hello, World!");
		}
	}
}