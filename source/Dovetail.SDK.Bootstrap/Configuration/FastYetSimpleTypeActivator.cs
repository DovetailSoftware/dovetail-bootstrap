using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dovetail.SDK.Bootstrap.Configuration
{
	public class FastYetSimpleTypeActivator
	{
		private static readonly object Lock = new object();
		private static readonly Dictionary<Type, Func<object>> Ctors = new Dictionary<Type, Func<object>>();

		public static object CreateInstance(Type type)
		{
			if (!Ctors.ContainsKey(type))
			{
				var exp = Expression.New(type);
				var d = Expression.Lambda<Func<object>>(exp).Compile();

				lock (Lock)
				{
					if (!Ctors.ContainsKey(type))
					{
						Ctors.Add(type, d);
					}
				}
			}

			return Ctors[type]();
		}
	}
}
