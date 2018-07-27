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
				//_.UseFile("duplicates.partial.config");
			});
		}

		[Test]
		public void verify_instructions()
		{
			theScenario.WhatDoIHave();
			VerifyInstructions.Assert(theScenario.Instructions, _ =>
			{
				_.Verify<BeginModelMap>(__ => __.Name.ShouldEqual(WorkflowObject.KeyFor("case")));

				_.SkipDefaults();

				//_.Verify<BeginActEntry>(__ =>
				//{
				//	__.Code.ShouldEqual(200);
				//	__.IsVerbose.ShouldBeFalse();
				//});
				//_.Is<BeginRelation>();
				//_.Verify<BeginProperty>(__ => __.Key.ShouldEqual("summary"));
				//_.Is<EndProperty>();
				//_.Is<EndRelation>();
				//_.Is<EndActEntry>();
				//_.Is<PushVariableContext>();
				//_.Is<PopVariableContext>();
				//_.Is<EndModelMap>();
				_.Verify<BeginActEntry>(__ =>
				{
					__.Code.ShouldEqual(333068852);
					__.IsVerbose.ShouldBeFalse();
				});

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
