using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff.Serialization
{
	[TestFixture]
	public class simple_properties_with_tags_scenario
	{
		private ModelMapParsingScenario theScenario;

		[SetUp]
		public void SetUp()
		{
			theScenario = ModelMapParsingScenario.Create(_ =>
			{
				_.UseFile("simple-case-with-tags.map.config");
			});
		}

		[Test]
		public void verify_instructions()
		{
			theScenario.Get<BeginModelMap>(0).Name.ShouldEqual("test");
			theScenario.Get<AddTag>(1).Tag.ShouldEqual("linkable");
			theScenario.Get<BeginView>(2).ViewName.ShouldEqual("qry_case_view");
			theScenario.Get<BeginProperty>(3).Key.ShouldEqual("id");
			theScenario.Get<EndProperty>(4);

			theScenario.Get<BeginProperty>(5).Key.ShouldEqual("title");
			theScenario.Get<EndProperty>(6);

			theScenario.Get<BeginProperty>(7).Key.ShouldEqual("ownerUsername");
			theScenario.Get<EndProperty>(8);

			theScenario.Get<BeginProperty>(9).Key.ShouldEqual("caseType");
			theScenario.Get<BeginTransform>(10).Name.ShouldEqual("localizedListItem");
			theScenario.Get<AddTransformArgument>(11).Name.ShouldEqual("listName");
			theScenario.Get<AddTransformArgument>(12).Name.ShouldEqual("listValue");
			theScenario.Get<EndTransform>(13);
			theScenario.Get<EndProperty>(14);

			theScenario.Get<EndView>(15);
			theScenario.Get<EndModelMap>(16);

			theScenario.Instructions.Length.ShouldEqual(17);
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}