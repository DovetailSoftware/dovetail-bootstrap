using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
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
		private readonly Stack<BeginActEntry> _entries = new Stack<BeginActEntry>();
		private readonly List<string> _privileges = new List<string>();
		private readonly ICurrentSDKUser _user;

		public ActEntryGatherer(IList<int> activityCodes, bool showAll, WorkflowObject workflowObject, IServiceLocator services, ICurrentSDKUser user)
		{
			_activityCodes = activityCodes;
			_showAll = showAll;
			_workflowObject = workflowObject;
			_services = services;
			_user = user;
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
			_entries.Push(instruction);
		}

		public void Visit(EndActEntry instruction)
		{
			var entry = _entries.Pop();
			if (entry.IsVerbose && !_showAll)
			{
				_privileges.Clear();
				return;
			}

			executeInstruction(() => _activityCodes.Add(entry.Code));
			_privileges.Clear();
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

		public void Visit(RequirePrivilege instruction)
		{
			_privileges.Add(instruction.Privilege.Resolve(_services).ToString());
		}

		private void executeInstruction(Action action)
		{
			var authorized = _privileges.All(_user.HasPermission);
			var context = new ActEntryConditionContext(_workflowObject, _services);
			if (authorized && _ignores.All(instruction => instruction.ShouldExecute(context)))
				action();
		}
	}
}
