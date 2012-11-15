using System;
using Dovetail.SDK.Bootstrap.Configuration;
using NUnit.Framework;

namespace Dovetail.SDK.Bootstrap.Tests.Configuration
{
	public class SimpleType
	{
		public string Name { get; set; }
	}

	[TestFixture]
	public class fast_type_activator
	{
		private readonly Type _type = typeof(SimpleType);

		[Test]
		public void create_object_via_type()
		{
			var result = FastYetSimpleTypeActivator.CreateInstance(_type);

			result.ShouldBeOfType(_type);
		}

		[Test]
		public void creates_different_object_each_time()
		{
			var result1 = FastYetSimpleTypeActivator.CreateInstance(_type);
			var result2 = FastYetSimpleTypeActivator.CreateInstance(_type);

			result1.ShouldNotBeTheSameAs(result2);
		}

		[Test]
		public void cannot_create_types_without_default_ctor()
		{
			typeof(ArgumentException).ShouldBeThrownBy(()=>FastYetSimpleTypeActivator.CreateInstance(typeof(String)));
		}
	}
}