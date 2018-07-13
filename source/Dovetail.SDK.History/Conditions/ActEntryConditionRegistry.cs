using System;
using System.Reflection;
using Dovetail.SDK.ModelMap.Serialization;
using FubuCore.Reflection;
using FubuCore.Util;

namespace Dovetail.SDK.History.Conditions
{
	public class ActEntryConditionRegistry : IActEntryConditionRegistry
	{
		private static readonly Cache<string, Type> Types;

		static ActEntryConditionRegistry()
		{
			Types = new Cache<string, Type>(_ => null);
			Reset();
		}

		public bool HasCondition(string name)
		{
			return Types.Has(name.ToLower());
		}

		public Type FindCondition(string name)
		{
			return Types[name.ToLower()];
		}

		public static void WithCondition<TCondition>(Action action) where TCondition : IActEntryCondition
		{
			Types.ClearAll();
			try
			{
				fillType(typeof(TCondition));
				action();
			}
			finally
			{
				Reset();
			}
		}

		public static void Reset()
		{
			Types.ClearAll();

			foreach (var type in TypeScanner.ConcreteImplementationsOf<IActEntryCondition>())
			{
				fillType(type);
			}
		}

		private static void fillType(Type type)
		{
			var name = type.Name;
			if (type.HasAttribute<ConditionAliasAttribute>())
				name = type.GetCustomAttribute<ConditionAliasAttribute>().Alias;

			Types.Fill(name.ToLower().Replace("condition", ""), type);
		}
	}
}