using System.Diagnostics;
using System.Linq;
using Dovetail.SDK.History.Instructions;
using Dovetail.SDK.ModelMap.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.History.Tests.Serialization
{
	[TestFixture]
	public class duplicates_scenario
	{
		private HistoryMapParsingScenario theScenario;

		[SetUp]
		public void SetUp()
		{
			theScenario = HistoryMapParsingScenario.Create(_ =>
			{
				_.UseFile("duplicates.history.config");
			});
		}

		[Test]
		public void verify_instructions()
		{
			VerifyInstructions.Assert(theScenario.Instructions, _ =>
			{
				_.Verify<BeginModelMap>(__ => __.Name.ShouldEqual(WorkflowObject.KeyFor("case")));

				_.SkipDefaults();

				_.Verify<BeginActEntry>(__ =>
				{
					__.Code.ShouldEqual(333068852);
					__.IsVerbose.ShouldBeFalse();
				});

				_.Verify<BeginProperty>(__ => __.Key.ShouldEqual(AdditionalInfoLexerTransform.Key));
				_.Is<EndProperty>();
				_.Verify<BeginTransform>(__ => __.Name.ShouldEqual("additionalInfoLexer"));
				_.Verify<AddTransformArgument>(__ => __.Name.ShouldEqual("pattern"));
				_.Is<EndTransform>();
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
