using System;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.ModelMap.Serialization.Filters
{
	public class IsInPolicy : IFilterPolicy
	{
		private readonly string _field;
		private readonly string _dataType;
		private readonly string[] _values;

		public IsInPolicy(string field, string dataType, params string[] values)
		{
			_field = field;
			_dataType = dataType;
			_values = values;
		}

		public Filter CreateFilter()
		{
			var expression = new FilterExpression();
			var dataType = PropertyTypes.Parse(_dataType);

			if (dataType == typeof(string))
				return expression.IsIn(_field, _values);

			throw new NotSupportedException("Unsupported data type: " + _dataType);
		}
	}
}