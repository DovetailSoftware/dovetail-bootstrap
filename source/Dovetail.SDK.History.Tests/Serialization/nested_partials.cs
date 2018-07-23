using NUnit.Framework;

namespace Dovetail.SDK.History.Tests.Serialization
{
	[TestFixture]
	public class nested_partials
	{
		private HistoryMapParsingScenario theScenario;

		[SetUp]
		public void SetUp()
		{
			theScenario = HistoryMapParsingScenario.Create(_ =>
			{
				_.UseFile("nested-partial.history.config");
				_.UseFile("partials1.partial.config");
				_.UseFile("partials2.partial.config");
				_.UseFile("partials3.partial.config");
				_.UseFile("partials4.partial.config");
			});
		}

		[Test]
		public void verify_instructions()
		{
			theScenario.WhatDoIHave();
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}
