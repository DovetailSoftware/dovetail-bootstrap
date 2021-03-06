﻿using System.Collections.Generic;
using Dovetail.SDK.ModelMap.Transforms;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.Transforms
{
	[TestFixture]
	public class StringConcatTransformTester
	{
		[Test]
		public void concats_the_strings()
		{
			var arguments = new TransformArguments(new InMemoryServiceLocator(), new Dictionary<string, object>
			{
				{ "arg1", "Hello, " },
				{ "arg2", "World!" }
			});

			var context = new TransformContext(new ModelData(), arguments, new InMemoryServiceLocator());
			var transform = new StringConcatTransform();

			transform.Execute(context).ShouldEqual("Hello, World!");
		}

		[Test]
		public void concats_n_number_of_strings()
		{
			var arguments = new TransformArguments(new InMemoryServiceLocator(), new Dictionary<string, object>
			{
				{ "arg1", "Hello" },
				{ "arg2", "," },
				{ "arg3", " " },
				{ "arg4", "World" },
				{ "arg5", "!" }
			});

			var context = new TransformContext(new ModelData(), arguments, new InMemoryServiceLocator());
			var transform = new StringConcatTransform();

			transform.Execute(context).ShouldEqual("Hello, World!");
		}
	}
}