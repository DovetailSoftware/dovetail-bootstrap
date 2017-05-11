using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff.Serialization
{
	[TestFixture]
	public class add_root_property_scenario
	{
		private ModelMapParsingScenario theScenario;

		[SetUp]
		public void SetUp()
		{
			theScenario = ModelMapParsingScenario.Create(_ =>
			{
				_.UseFile("simple-case.map.config");
				_.UseFile("add-root-property.map.config");
			});
		}

		[Test]
		public void verify_instructions()
		{
			theScenario.Get<BeginModelMap>(0).Name.ShouldEqual("test");
			theScenario.Get<BeginView>(1).ViewName.ShouldEqual("qry_case_view");

			theScenario.Get<BeginProperty>(2).Key.ShouldEqual("anotherTitle");
			theScenario.Get<EndProperty>(3);

			theScenario.Get<BeginProperty>(4).Key.ShouldEqual("id");
			theScenario.Get<EndProperty>(5);

			theScenario.Get<BeginProperty>(6).Key.ShouldEqual("title");
			theScenario.Get<EndProperty>(7);

			theScenario.Get<BeginProperty>(8).Key.ShouldEqual("ownerUsername");
			theScenario.Get<EndProperty>(9);

			theScenario.Get<BeginProperty>(10).Key.ShouldEqual("caseType");
			theScenario.Get<BeginTransform>(11).Name.ShouldEqual("localizedListItem");
			theScenario.Get<AddTransformArgument>(12).Name.ShouldEqual("listName");
			theScenario.Get<AddTransformArgument>(13).Name.ShouldEqual("listValue");
			theScenario.Get<EndTransform>(14);
			theScenario.Get<EndProperty>(15);

			theScenario.Get<EndView>(16);
			theScenario.Get<EndModelMap>(17);

			theScenario.Instructions.Length.ShouldEqual(18);
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}