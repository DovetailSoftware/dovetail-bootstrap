using System.Diagnostics;
using System.Linq;
using Dovetail.SDK.History.Instructions;
using Dovetail.SDK.ModelMap.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.History.Tests.Serialization
{
	[TestFixture]
	public class email_scenario
	{
		private HistoryMapParsingScenario theScenario;

		[SetUp]
		public void SetUp()
		{
			theScenario = HistoryMapParsingScenario.Create(_ =>
			{
				_.UseFile("emails.history.config");
			});
		}

		[Test]
		public void verify_instructions()
		{
			theScenario.WhatDoIHave();
			var container = TestContainer.getContainer();
			var provider = container.With(theScenario.Cache).GetInstance<HistoryProvider>();
			var data = provider.HistoryFor(new HistoryRequest
			{
				ShowAllActivities = false,
				WorkflowObject = new WorkflowObject
				{
					Id = "1",
					Type = "case",
					IsChild = false,
				},
				HistoryItemLimit = 5
			});

			Debug.WriteLine(data.Items.Length);
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}