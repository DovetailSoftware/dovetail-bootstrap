using System;

namespace Dovetail.SDK.History.Conditions
{
	public interface IActEntryConditionRegistry
	{
		bool HasCondition(string name);
		Type FindCondition(string name);
	}
}
