using System;
using System.Diagnostics;
using Dovetail.SDK.ModelMap.Transforms;
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
				_.UseFile("subcase.history.config");
			});
		}

		[Test]
		public void verify_instructions()
		{
			theScenario.WhatDoIHave();
			var container = TestContainer.getContainer();
			var settings = container.GetInstance<HistorySettings>();
			settings.MergeCaseHistoryChildSubcases = true;
			var provider = container.With(theScenario.Cache).With(settings).GetInstance<HistoryProvider>();
			var data = provider.HistoryFor(new HistoryRequest
			{
				ShowAllActivities = true,
				WorkflowObject = new WorkflowObject
				{
					Id = "53",
					Type = "case",
					IsChild = false,
				},
				Since = DateTime.Parse("2018-06-27 14:16:45"),
				HistoryItemLimit = 3
			});

			foreach (var item in data.Items)
			{
				Debug.WriteLine(item.Get<int>("id"));
			}
			
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}

	public class RelatedToEmailLog : CancellationPolicy
	{
		public override bool ShouldCancel(TransformContext context)
		{
			var data = context.Model;
			var details = data.Child("details");
			if (details == null)
				return false;

			return details["relatedEmailLogId"] != null;
		}
	}
}