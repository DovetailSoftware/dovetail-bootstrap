using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration
{
	[TestFixture]
	public class ModelDataTester
	{
		[Test]
		public void has_tag()
		{
			var model = new ModelData { Name = "case" };
			model.AddTag("test");
			model.HasTag("test").ShouldBeTrue();
		}

		[Test]
		public void does_not_have_tag()
		{
			var model = new ModelData { Name = "case" };
			model.HasTag("test").ShouldBeFalse();

		}
	}
}