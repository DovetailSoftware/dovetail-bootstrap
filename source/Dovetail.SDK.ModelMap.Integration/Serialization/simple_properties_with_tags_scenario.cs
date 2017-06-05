using Dovetail.SDK.ModelMap.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.Serialization
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
			VerifyInstructions.Assert(theScenario.Instructions, _ =>
			{
				_.Get<BeginModelMap>().Name.ShouldEqual("test");
				_.Get<AddTag>().Tag.ShouldEqual("linkable");
				_.Get<BeginView>().ViewName.ShouldEqual("qry_case_view");
				_.Get<BeginProperty>().Key.ShouldEqual("id");
				_.Get<EndProperty>();

				_.Get<BeginProperty>().Key.ShouldEqual("title");
				_.Get<EndProperty>();

				_.Get<BeginProperty>().Key.ShouldEqual("ownerUsername");
				_.Get<EndProperty>();

				_.Get<BeginProperty>().Key.ShouldEqual("caseType");
				_.Get<BeginTransform>().Name.ShouldEqual("localizedListItem");
				_.Get<AddTransformArgument>().Name.ShouldEqual("listName");
				_.Get<AddTransformArgument>().Name.ShouldEqual("listValue");
				_.Get<EndTransform>();
				_.Get<EndProperty>();

				_.Get<EndView>();
				_.Get<EndModelMap>();
			});

			theScenario.Instructions.Length.ShouldEqual(17);
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}