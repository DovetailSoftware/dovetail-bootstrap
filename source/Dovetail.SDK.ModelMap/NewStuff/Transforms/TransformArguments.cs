using System.Collections;
using System.Collections.Generic;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public class TransformArguments : IEnumerable<KeyValuePair<string, object>>
	{
		private readonly IServiceLocator _services;
		private readonly IDictionary<string, object> _values;

		public TransformArguments(IServiceLocator services, IDictionary<string, object> values)
		{
			_services = services;
			_values = values;
		}

		public object Get(string key)
		{
			var value = _values[key];
			if (value == null) return null;

			if (value is IDynamicValue)
				return value.As<IDynamicValue>().Resolve(_services);

			if (value is string)
			{
				var expander = _services.GetInstance<IMappingVariableExpander>();
				if (expander.IsVariable(value.ToString()))
				{
					return expander.Expand(value.ToString());
				}
			}

			return value;
		}

		public T Get<T>(string key)
		{
			var value = Get(key);
			if (value == null)
				return default(T);
			
			return value.As<T>();
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