using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.History.Conditions;
using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Instructions;
using FubuCore;

namespace Dovetail.SDK.History.Instructions
{
	public class BeginWhen : IModelMapInstruction
	{
		public bool? IsChild { get; set; }
		public bool? MergeCaseHistory { get; set; }
		public IDynamicValue Condition { get; set; }

		public void Accept(IModelMapVisitor visitor)
		{
			visitor.As<IHistoryModelMapVisitor>().Visit(this);
		}

		public bool ShouldExecute(ActEntryConditionContext context)
		{
			if (Condition != null)
				return ExecuteCondition(context);

			if (IsChild.HasValue)
				return IsChild.Value == context.WorkflowObject.IsChild;

			if (!MergeCaseHistory.HasValue)
				return true;

			return MergeCaseHistory.Value == context.Settings.MergeCaseHistoryChildSubcases;
		}

		public bool ExecuteCondition(ActEntryConditionContext context)
		{
			var name = Condition.Resolve(context.Services).ToString();
			var registry = context.Service<IActEntryConditionRegistry>();
			if (!registry.HasCondition(name))
			{
				throw new ModelMapException("Could not find condition: \"{0}\"".ToFormat(name));
			}

			var type = registry.FindCondition(name);
			var condition = (IActEntryCondition)FastYetSimpleTypeActivator.CreateInstance(type);

			return condition.ShouldExecute(context);
		}
	}
}
