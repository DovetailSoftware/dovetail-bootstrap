using System;

namespace Dovetail.SDK.History.Conditions
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ConditionAliasAttribute : Attribute
	{
		public string Alias { get; set; }

		public ConditionAliasAttribute(string alias)
		{
			Alias = alias;
		}
	}
}