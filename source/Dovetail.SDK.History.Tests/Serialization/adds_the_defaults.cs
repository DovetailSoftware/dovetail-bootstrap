using Dovetail.SDK.History.Instructions;
using Dovetail.SDK.ModelMap.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.History.Tests.Serialization
{
	[TestFixture]
	public class adds_the_defaults
	{
		private HistoryMapParsingScenario theScenario;

		[SetUp]
		public void SetUp()
		{
			theScenario = HistoryMapParsingScenario.Create(_ =>
			{
				_.UseFile("defaults.history.config");
			});
		}

		[Test]
		public void verify_instructions()
		{
			VerifyInstructions.Assert(theScenario.Instructions, _ =>
			{
				_.Verify<BeginModelMap>(__ => __.Name.ShouldEqual(WorkflowObject.KeyFor("case")));

				_.Get<BeginProperty>().Key.ShouldEqual("id");
				_.Is<EndProperty>();

				_.Get<BeginProperty>().Key.ShouldEqual("type");
				_.Is<EndProperty>();

				_.Get<BeginProperty>().Key.ShouldEqual("timestamp");
				_.Is<EndProperty>();

				_.Get<BeginMappedProperty>().Key.ShouldEqual("performedBy");
				_.Get<BeginRelation>().RelationName.ShouldEqual("act_entry2user");
				_.Get<BeginProperty>().Key.ShouldEqual("username");
				_.Is<EndProperty>();
				_.Get<BeginRelation>().RelationName.ShouldEqual("user2employee");
				_.Get<BeginProperty>().Key.ShouldEqual("firstName");
				_.Is<EndProperty>();
				_.Get<BeginProperty>().Key.ShouldEqual("lastName");
				_.Is<EndProperty>();
				_.Is<EndRelation>();
				_.Is<EndRelation>();
				_.Is<EndMappedProperty>();

				_.Get<BeginMappedProperty>().Key.ShouldEqual("impersonatedBy");
				_.Get<BeginAdHocRelation>().ToTableName.ShouldEqual("empl_user");
				_.Get<BeginProperty>().Key.ShouldEqual("id");
				_.Is<EndProperty>();
				_.Get<BeginProperty>().Key.ShouldEqual("username");
				_.Is<EndProperty>();
				_.Get<BeginProperty>().Key.ShouldEqual("firstName");
				_.Is<EndProperty>();
				_.Get<BeginProperty>().Key.ShouldEqual("lastName");
				_.Is<EndProperty>();
				_.Is<EndRelation>();
				_.Is<EndMappedProperty>();

				_.Verify<BeginActEntry>(__ =>
					{
						__.Code.ShouldEqual(8900);
						__.IsVerbose.ShouldBeTrue();
					});
				_.Is<EndActEntry>();

				_.Is<EndModelMap>();
			});
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}
