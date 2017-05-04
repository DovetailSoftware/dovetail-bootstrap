using System.Collections.Generic;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public class TransformArguments
	{
		private readonly IDictionary<string, object> _values;

		public TransformArguments(IDictionary<string, object> values)
		{
			_values = values;
		}

		public object Get(string key)
		{
			return _values[key];
		}

		public T Get<T>(string key)
		{
			return _values[key].As<T>();
		}

		public bool Has(string key)
		{
			return _values.ContainsKey(key);
		}
	}
}