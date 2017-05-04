using Dovetail.SDK.ModelMap.NewStuff;
using Dovetail.SDK.ModelMap.NewStuff.Transforms;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff.Transforms
{
	[TestFixture]
	public class ModelDataPathTester
	{
		[Test]
		public void gets_the_value()
		{
			var data = new ModelData();
			data["child"] = new ModelData();
			data.Child("child")["foo"] = "bar";

			ModelDataPath.Parse("child.foo").Get(data).ShouldEqual("bar");
		}

		[Test]
		public void sets_the_value()
		{
			var data = new ModelData();
			data["child"] = new ModelData();

			ModelDataPath.Parse("child.foo").Set(data, "bar");

			data.Child("child").Get<string>("foo").ShouldEqual("bar");
		}

	}
}