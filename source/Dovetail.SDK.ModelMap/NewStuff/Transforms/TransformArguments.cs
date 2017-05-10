using System.Collections;
using System.Collections.Generic;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public class TransformArguments : IEnumerable<KeyValuePair<string, object>>
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

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return _values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}