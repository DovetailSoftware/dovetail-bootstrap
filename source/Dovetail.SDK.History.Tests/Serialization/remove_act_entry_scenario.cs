using Dovetail.SDK.History.Instructions;
using Dovetail.SDK.ModelMap.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.History.Tests.Serialization
{
	[TestFixture]
	public class remove_act_entry_scenario
	{
		private HistoryMapParsingScenario theScenario;

		[SetUp]
		public void SetUp()
		{
			theScenario = HistoryMapParsingScenario.Create(_ =>
			{
				_.UseFile("simple.history.config");
				_.UseFile("remove-act-entry.history.config");
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

				// The first used to be 3400 but now it should be gone

				_.Verify<BeginActEntry>(__ =>
				{
					__.Code.ShouldEqual(8900);
					__.IsVerbose.ShouldBeTrue();
				});
			});
		}
	}
}