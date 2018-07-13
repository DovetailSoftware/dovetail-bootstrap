using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.History.Conditions;
using Dovetail.SDK.History.Instructions;
using Dovetail.SDK.ModelMap.Instructions;
using FubuCore;

namespace Dovetail.SDK.History
{
	public class ActEntryGatherer : IHistoryModelMapVisitor
	{
		private readonly IList<int> _activityCodes;
		private readonly bool _showAll;
		private readonly WorkflowObject _workflowObject;
		private readonly IServiceLocator _services;
		private readonly Stack<BeginWhen> _ignores = new Stack<BeginWhen>();

		public ActEntryGatherer(IList<int> activityCodes, bool showAll, WorkflowObject workflowObject, IServiceLocator services)
		{
			_activityCodes = activityCodes;
			_showAll = showAll;
			_workflowObject = workflowObject;
			_services = services;
		}

		public void Visit(BeginModelMap instruction)
		{
		}

		public void Visit(BeginTable instruction)
		{
		}

		public void Visit(EndTable instruction)
		{
		}

		public void Visit(BeginView instruction)
		{
		}

		public void Visit(EndView instruction)
		{
		}

		public void Visit(EndModelMap instruction)
		{
		}

		public void Visit(BeginProperty instruction)
		{
		}

		public void Visit(EndProperty instruction)
		{
		}

		public void Visit(BeginAdHocRelation instruction)
		{
		}

		public void Visit(BeginRelation instruction)
		{
		}

		public void Visit(EndRelation instruction)
		{
		}

		public void Visit(BeginMappedProperty instruction)
		{
		}

		public void Visit(EndMappedProperty instruction)
		{
		}

		public void Visit(BeginMappedCollection instruction)
		{
		}

		public void Visit(EndMappedCollection instruction)
		{
		}

		public void Visit(FieldSortMap instruction)
		{
		}

		public void Visit(AddFilter instruction)
		{
		}

		public void Visit(BeginTransform instruction)
		{
		}

		public void Visit(EndTransform instruction)
		{
		}

		public void Visit(AddTransformArgument instruction)
		{
		}

		public void Visit(RemoveProperty instruction)
		{
		}

		public void Visit(RemoveMappedProperty instruction)
		{
		}

		public void Visit(RemoveMappedCollection instruction)
		{
		}

		public void Visit(AddTag instruction)
		{
		}

		public void Visit(PushVariableContext instruction)
		{
		}

		public void Visit(PopVariableContext instruction)
		{
		}

		public void Visit(BeginActEntry instruction)
		{
			if (!_showAll || instruction.IsVerbose)
				return;

			executeInstruction(() => _activityCodes.Add(instruction.Code));
		}

		public void Visit(EndActEntry instruction)
		{
		}

		public void Visit(BeginCancellationPolicy instruction)
		{
		}

		public void Visit(EndCancellationPolicy instruction)
		{
		}

		public void Visit(BeginWhen instruction)
		{
			_ignores.Push(instruction);
		}

		public void Visit(EndWhen instruction)
		{
			_ignores.Pop();
		}

		private void executeInstruction(Action action)
		{
			var context = new ActEntryConditionContext(_workflowObject, _services);
			if (_ignores.All(instruction => instruction.ShouldExecute(context)))
				action();
		}
	}
}
