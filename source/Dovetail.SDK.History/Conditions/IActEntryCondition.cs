namespace Dovetail.SDK.History.Conditions
{
	public interface IActEntryCondition
	{
		bool ShouldExecute(ActEntryConditionContext conditionContext);
	}
}
