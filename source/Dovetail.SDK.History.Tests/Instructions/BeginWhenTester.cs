using System;
using Dovetail.SDK.History.Conditions;
using Dovetail.SDK.History.Instructions;
using Dovetail.SDK.ModelMap;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.History.Tests.Instructions
{
	[TestFixture]
	public class BeginWhenTester
	{
		private BeginWhen theInstruction;
		private ActEntryConditionContext theContext;
		private HistorySettings theSettings;
		private InMemoryServiceLocator theServices;

		[SetUp]
		public void SetUp()
		{
			theInstruction = new BeginWhen();
			theSettings = new HistorySettings();
			theServices = new InMemoryServiceLocator();
			theServices.Add(theSettings);
			theServices.Add<IActEntryConditionRegistry>(new ActEntryConditionRegistry());
			theContext = new ActEntryConditionContext(new WorkflowObject(), theServices);
		}

		[Test]
		public void should_execute_when_is_child_is_true_and_workflow_object_is_false()
		{
			theInstruction.IsChild = true;
			theContext.WorkflowObject.IsChild = false;

			theInstruction.ShouldExecute(theContext).ShouldBeFalse();
		}

		[Test]
		public void should_execute_when_is_child_is_true_and_workflow_object_is_true()
		{
			theInstruction.IsChild = true;
			theContext.WorkflowObject.IsChild = true;

			theInstruction.ShouldExecute(theContext).ShouldBeTrue();
		}

		[Test]
		public void should_execute_when_is_child_is_false_and_workflow_object_is_false()
		{
			theInstruction.IsChild = false;
			theContext.WorkflowObject.IsChild = false;

			theInstruction.ShouldExecute(theContext).ShouldBeTrue();
		}

		[Test]
		public void should_execute_when_is_child_is_false_and_workflow_object_is_true()
		{
			theInstruction.IsChild = true;
			theContext.WorkflowObject.IsChild = false;

			theInstruction.ShouldExecute(theContext).ShouldBeFalse();
		}

		[Test]
		public void should_execute_when_merge_history_is_true_and_setting_is_false()
		{
			theInstruction.MergeCaseHistory = true;
			theSettings.MergeCaseHistoryChildSubcases = false;

			theInstruction.ShouldExecute(theContext).ShouldBeFalse();
		}

		[Test]
		public void should_execute_when_merge_history_is_true_and_setting_is_true()
		{
			theInstruction.MergeCaseHistory = true;
			theSettings.MergeCaseHistoryChildSubcases = true;

			theInstruction.ShouldExecute(theContext).ShouldBeTrue();
		}

		[Test]
		public void should_execute_when_merge_history_is_false_and_setting_is_false()
		{
			theInstruction.MergeCaseHistory = false;
			theSettings.MergeCaseHistoryChildSubcases = false;

			theInstruction.ShouldExecute(theContext).ShouldBeTrue();
		}

		[Test]
		public void should_execute_when_merge_history_is_false_and_setting_is_true()
		{
			theInstruction.MergeCaseHistory = false;
			theSettings.MergeCaseHistoryChildSubcases = true;

			theInstruction.ShouldExecute(theContext).ShouldBeFalse();
		}

		[Test]
		public void should_execute_when_condition_returns_true()
		{
			ActEntryConditionRegistry.WithCondition<StubCondition>(() =>
			{
				theInstruction.Condition = new PassThruValue("stub");
				StubCondition.Value = true;

				theInstruction.ShouldExecute(theContext).ShouldBeTrue();
			});
		}

		[Test]
		public void should_execute_when_condition_returns_false()
		{
			ActEntryConditionRegistry.WithCondition<StubCondition>(() =>
			{
				theInstruction.Condition = new PassThruValue("stub");
				StubCondition.Value = false;

				theInstruction.ShouldExecute(theContext).ShouldBeFalse();
			});
		}

		[Test]
		public void should_execute_when_condition_cannot_be_found()
		{
			theInstruction.Condition = new PassThruValue(Guid.NewGuid().ToString());
			Assert.Throws<ModelMapException>(() => theInstruction.ShouldExecute(theContext));
		}

		private class StubCondition : IActEntryCondition
		{
			public static bool Value { get; set; }

			public bool ShouldExecute(ActEntryConditionContext conditionContext)
			{
				return Value;
			}
		}

		private class PassThruValue : IDynamicValue
		{
			private readonly string _value;

			public PassThruValue(string value)
			{
				_value = value;
			}

			public object Resolve(IServiceLocator services)
			{
				return _value;
			}
		}
	}
}
