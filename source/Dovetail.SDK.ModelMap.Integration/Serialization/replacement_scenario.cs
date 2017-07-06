using Dovetail.SDK.ModelMap.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.Serialization
{
	[TestFixture]
	public class replacement_scenario
	{
		private ModelMapParsingScenario theScenario;

		[SetUp]
		public void SetUp()
		{
			theScenario = ModelMapParsingScenario.Create(_ =>
			{
				_.UseFile("advanced-case.map.config");
				_.UseFile("site.partial.config");
				_.UseFile("replacement.map.config");
			});
		}

		[Test]
		public void verify_instructions()
		{
			VerifyInstructions.Assert(theScenario.Instructions, _ =>
			{
				_.Get<BeginModelMap>().Name.ShouldEqual("test");
				_.Get<BeginView>().ViewName.ShouldEqual("custom_view");
				_.Get<BeginProperty>().Key.ShouldEqual("id");
				_.Get<EndProperty>();

				_.Get<BeginProperty>().Key.ShouldEqual("title");
				_.Get<EndProperty>();

				_.Get<EndView>();
				_.Get<EndModelMap>();
			});

			theScenario.Instructions.Length.ShouldEqual(8);
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}