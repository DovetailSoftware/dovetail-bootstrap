using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.History.Conditions;
using Dovetail.SDK.History.Serialization;
using Dovetail.SDK.History.Tests.Serialization;
using FChoice.Foundation.DataObjects;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.History.Tests
{
	[TestFixture]
	public class ActEntryGathererTester
	{
		[TestFixture]
		public class when_is_child
		{
			[Test]
			public void and_workflow_object_is_child()
			{
				ActEntryGathererScenario.Create(_ =>
				{
					_.UseFile("is-child.history.config");
					_.TheWorkflowObjectIs(new WorkflowObject
					{
						Type = "case",
						Id = "1",
						IsChild = true
					});
				}).ShouldHaveTheSameElementsAs(100, 200, 300);
			}

			[Test]
			public void and_workflow_object_is_not_child()
			{
				ActEntryGathererScenario.Create(_ =>
				{
					_.UseFile("is-child.history.config");
					_.TheWorkflowObjectIs(new WorkflowObject
					{
						Type = "case",
						Id = "1",
						IsChild = false
					});
				}).ShouldHaveTheSameElementsAs(400, 500);
			}
		}

		[TestFixture]
		public class when_merge_history
		{
			[Test]
			public void and_setting_is_true()
			{
				ActEntryGathererScenario.Create(_ =>
				{
					_.UseFile("merge.history.config");
					_.TheWorkflowObjectIs(new WorkflowObject
					{
						Type = "case",
						Id = "1",
						IsChild = false
					});
					_.MergeHistory(true);
				}).ShouldHaveTheSameElementsAs(101, 202, 303);
			}

			[Test]
			public void and_setting_is_false()
			{
				ActEntryGathererScenario.Create(_ =>
				{
					_.UseFile("merge.history.config");
					_.TheWorkflowObjectIs(new WorkflowObject
					{
						Type = "case",
						Id = "1",
						IsChild = false
					});
					_.MergeHistory(false);
				}).ShouldHaveTheSameElementsAs(404, 505);
			}
		}

		[TestFixture]
		public class when_requires_privilege
		{
			[Test]
			public void it_filters_by_user()
			{
				ActEntryGathererScenario.Create(_ =>
				{
					_.UseFile("privilege.history.config");
					_.TheWorkflowObjectIs(new WorkflowObject
					{
						Type = "case",
						Id = "1",
						IsChild = false
					});
					_.TheUserHasPrivilege("a");
				}).ShouldHaveTheSameElementsAs(1001, 1003);

				ActEntryGathererScenario.Create(_ =>
				{
					_.UseFile("privilege.history.config");
					_.TheWorkflowObjectIs(new WorkflowObject
					{
						Type = "case",
						Id = "1",
						IsChild = false
					});
					_.TheUserHasPrivilege("b");
				}).ShouldHaveTheSameElementsAs(1002, 1003);
			}
		}

		[TestFixture]
		public class when_is_verbose
		{
			[Test]
			public void and_the_request_is_verbose()
			{
				ActEntryGathererScenario.Create(_ =>
				{
					_.UseFile("verbose.history.config");
					_.TheWorkflowObjectIs(new WorkflowObject
					{
						Type = "case",
						Id = "1",
						IsChild = false
					});
					_.ShowAll(true);
				}).ShouldHaveTheSameElementsAs(2000, 2001, 2002, 2003);
			}

			[Test]
			public void and_the_request_is_not_verbose()
			{
				ActEntryGathererScenario.Create(_ =>
				{
					_.UseFile("verbose.history.config");
					_.TheWorkflowObjectIs(new WorkflowObject
					{
						Type = "case",
						Id = "1",
						IsChild = false
					});
					_.ShowAll(false);
				}).ShouldHaveTheSameElementsAs(2000, 2003);
			}
		}

		[TestFixture]
		public class when_custom_condition
		{
			[Test]
			public void it_filters_the_act_entries()
			{
				ActEntryTest.Value = true;
				ActEntryConditionRegistry.WithCondition<ActEntryTest>(() =>
				{
					ActEntryGathererScenario.Create(_ =>
					{
						_.UseFile("custom-condition.history.config");
						_.TheWorkflowObjectIs(new WorkflowObject
						{
							Type = "case",
							Id = "1",
							IsChild = false
						});
					}).ShouldHaveTheSameElementsAs(101, 202, 303);
				});

				ActEntryTest.Value = false;
				ActEntryConditionRegistry.WithCondition<ActEntryTest>(() =>
				{
					ActEntryGathererScenario.Create(_ =>
					{
						_.UseFile("custom-condition.history.config");
						_.TheWorkflowObjectIs(new WorkflowObject
						{
							Type = "case",
							Id = "1",
							IsChild = false
						});
					}).ShouldBeEmpty();
				});
			}

			private class ActEntryTest : IActEntryCondition
			{
				public static bool Value { get; set; }

				public bool ShouldExecute(ActEntryConditionContext conditionContext)
				{
					return Value;
				}
			}
		}

		[TestFixture]
		public class nested_when
		{
			[Test]
			public void evalutes_the_conditions()
			{
				ActEntryGathererScenario.Create(_ =>
				{
					_.UseFile("nested.history.config");
					_.TheWorkflowObjectIs(new WorkflowObject
					{
						Type = "case",
						Id = "1",
						IsChild = true
					});
					_.MergeHistory(true);
				}).ShouldHaveTheSameElementsAs(100, 200, 300, 101, 202, 303);

				ActEntryGathererScenario.Create(_ =>
				{
					_.UseFile("nested.history.config");
					_.TheWorkflowObjectIs(new WorkflowObject
					{
						Type = "case",
						Id = "1",
						IsChild = false
					});
					_.MergeHistory(true);
				}).ShouldHaveTheSameElementsAs(400, 500, 401);
			}
		}

		private class ActEntryGathererScenario
		{
			private readonly WorkflowObject _workflowObject;
			private readonly bool _showAll;

			public ActEntryGathererScenario(WorkflowObject workflowObject, bool showAll)
			{
				_workflowObject = workflowObject;
				_showAll = showAll;
			}

			public int[] ResolveActCodes(HistoryMapParsingScenario scenario)
			{
				var actCodes = new List<int>();
				var gatherer = new ActEntryGatherer(actCodes, _showAll,
					_workflowObject, scenario.Services, scenario.Services.GetInstance<ICurrentSDKUser>(), new HistoryPrivilegePolicyCache(new HistorySettings()));

				scenario.Map.Accept(gatherer);

				return actCodes.ToArray();
			}

			public static int[] Create(Action<ActEntryGathererScenarioExpression> configure)
			{
				ActEntryGathererScenario actEntryScenario = null;
				var scenario = HistoryMapParsingScenario.Create(_ =>
				{
					var actEntryExpression = new ActEntryGathererScenarioExpression(_);
					configure(actEntryExpression);

					actEntryScenario = actEntryExpression.As<IScenarioBuilder>().Create();
				});

				return actEntryScenario.ResolveActCodes(scenario);
			}
		}

		private interface IScenarioBuilder
		{
			ActEntryGathererScenario Create();
		}

		private class ActEntryGathererScenarioExpression : IScenarioBuilder
		{
			private readonly HistoryMapParsingScenario.ParsingExpression _inner;
			private readonly StubCurrentSDKUser _user;
			private WorkflowObject _workflowObject;
			private bool _showAll;

			public ActEntryGathererScenarioExpression(HistoryMapParsingScenario.ParsingExpression inner)
			{
				_inner = inner;
				_user = new StubCurrentSDKUser();

				_inner.UseService<IActEntryConditionRegistry>(new ActEntryConditionRegistry());
				_inner.UseService<ICurrentSDKUser>(_user);
			}

			public void TheUserHasPrivilege(string privilege)
			{
				_user.Privileges.Add(privilege);
			}

			public void UseFile(string filePath)
			{
				_inner.UseFile(filePath);
			}

			public void TheWorkflowObjectIs(WorkflowObject workflowObject)
			{
				_workflowObject = workflowObject;
			}

			public void MergeHistory(bool flag)
			{
				_inner.Services.GetInstance<HistorySettings>().MergeCaseHistoryChildSubcases = flag;
			}

			public void ShowAll(bool flag)
			{
				_showAll = flag;
			}

			ActEntryGathererScenario IScenarioBuilder.Create()
			{
				return new ActEntryGathererScenario(_workflowObject, _showAll);
			}
		}

		private class StubCurrentSDKUser : ICurrentSDKUser
		{
			public StubCurrentSDKUser()
			{
				Privileges = new List<string>();
			}

			public string Username { get; set; }
			public string ImpersonatingUsername { get; set; }
			public string Fullname { get; set; }
			public string Sitename { get; set; }
			public bool IsAuthenticated { get; set; }

			public ITimeZone Timezone { get; set; }
			public IEnumerable<SDKUserQueue> Queues { get; set; }
			public string Workgroup { get; set; }
			public string PrivClass { get; set; }

			public List<string> Privileges { get; set; }

			public bool HasPermission(string permission)
			{
				return Privileges.Contains(permission);
			}

			public void SignOut()
			{
				throw new NotImplementedException();
			}

			public void SetUser(string clarifyLoginName)
			{
				throw new NotImplementedException();
			}

			public void SetTimezone(ITimeZone timezone)
			{
				throw new NotImplementedException();
			}
		}
	}
}
