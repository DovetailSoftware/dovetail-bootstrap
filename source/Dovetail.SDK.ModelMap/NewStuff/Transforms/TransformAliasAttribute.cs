using System;

namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TransformAliasAttribute : Attribute
	{
		public string Alias { get; set; }

		public TransformAliasAttribute(string alias)
		{
			Alias = alias;
		}
	}
}