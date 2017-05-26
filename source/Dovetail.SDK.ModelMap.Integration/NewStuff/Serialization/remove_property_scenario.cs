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
			VerifyInstructions.Assert(theScenario.Instructions, _ =>
			{
				_.Get<BeginModelMap>().Name.ShouldEqual("test");
				_.Get<BeginView>().ViewName.ShouldEqual("qry_case_view");
				_.Get<BeginProperty>().Key.ShouldEqual("id");
				_.Get<EndProperty>();

				_.Get<BeginProperty>().Key.ShouldEqual("title");
				_.Get<EndProperty>();

				_.Get<BeginProperty>().Key.ShouldEqual("ownerUsername");
				_.Get<EndProperty>();

				_.Get<EndView>();
				_.Get<EndModelMap>();
			});

			theScenario.Instructions.Length.ShouldEqual(10);
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}