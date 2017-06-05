using System.Collections.Generic;

namespace Dovetail.SDK.ModelMap
{
	public class VariableExpanderContext
	{
		private readonly ModelData _data;
		private readonly IDictionary<string, object> _values;

		public VariableExpanderContext(ModelData data, IDictionary<string, object> values)
		{
			_data = data;
			_values = values;
		}

		public ModelData Data
		{
			get { return _data; }
		}

		public bool Has(string key)
		{
			return _values.ContainsKey(key);
		}

		public object Get(string key)
		{
			return _values[key];
		}
	}
}