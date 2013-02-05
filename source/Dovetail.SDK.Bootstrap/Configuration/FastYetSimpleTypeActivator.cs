using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dovetail.SDK.Bootstrap.Configuration
{
	public class FastYetSimpleTypeActivator
	{
		private static readonly Dictionary<Type, Func<object>> _ctors = new Dictionary<Type, Func<object>>();
		
		public static object CreateInstance(Type type)
		{
			if (!_ctors.ContainsKey(type))
			{
				var exp = Expression.New(type);
				var d = Expression.Lambda<Func<object>>(exp).Compile();
				
				if (!_ctors.ContainsKey(type))
				{
					_ctors.Add(type, d);
				}
			}

			return _ctors[type]();
		}
	}
}