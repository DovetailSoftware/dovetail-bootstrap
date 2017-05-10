using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff.Serialization
{
	[TestFixture]
	public class remove_property_scenario
	{
		private ModelMapParsingScenario theScenario;

		[SetUp]
		public void SetUp()
		{
			theScenario = ModelMapParsingScenario.Create(_ =>
			{
				_.UseFile("simple-case.map.config");
				_.UseFile("remove-property.map.config");
			});
		}

		[Test]
		public void verify_instructions()
		{
			theScenario.Get<BeginModelMap>(0).Name.ShouldEqual("test");
			theScenario.Get<BeginView>(1).ViewName.ShouldEqual("qry_case_view");
			theScenario.Get<BeginProperty>(2).Key.ShouldEqual("id");
			theScenario.Get<EndProperty>(3);

			theScenario.Get<BeginProperty>(4).Key.ShouldEqual("title");
			theScenario.Get<EndProperty>(5);

			theScenario.Get<BeginProperty>(6).Key.ShouldEqual("ownerUsername");
			theScenario.Get<EndProperty>(7);

			theScenario.Get<EndView>(8);
			theScenario.Get<EndModelMap>(9);

			theScenario.Instructions.Length.ShouldEqual(10);
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}