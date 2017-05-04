using System;
using System.Reflection;
using Dovetail.SDK.ModelMap.NewStuff.Serialization;
using FubuCore.Reflection;
using FubuCore.Util;

namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public class MappingTransformRegistry : IMappingTransformRegistry
	{
		private static readonly Cache<string, Type> Types;

		static MappingTransformRegistry()
		{
			Types = new Cache<string, Type>(_ => null);
			Reset();
		}

		public bool HasPolicy(string name)
		{
			return Types.Has(name.ToLower());
		}

		public Type FindPolicy(string name)
		{
			return Types[name.ToLower()];
		}

		public static void WithTransform<TTransform>(Action action) where TTransform : IMappingTransform
		{
			Types.ClearAll();
			try
			{
				fillType(typeof(TTransform));
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

			foreach (var type in TypeScanner.ConcreteImplementationsOf<IMappingTransform>())
			{
				fillType(type);
			}
		}

		private static void fillType(Type type)
		{
			var name = type.Name;
			if (type.HasAttribute<TransformAliasAttribute>())
				name = type.GetCustomAttribute<TransformAliasAttribute>().Alias;

			Types.Fill(name.ToLower().Replace("transform", ""), type);
		}
	}
}