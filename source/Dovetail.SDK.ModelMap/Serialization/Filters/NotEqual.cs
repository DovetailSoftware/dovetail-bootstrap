using System;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.ModelMap.Serialization.Filters
{
    public class NotEqualPolicy : IFilterPolicy
    {
        private readonly string _field;
        private readonly string _value;
        private readonly string _dataType;

        public NotEqualPolicy(string field, string value, string dataType)
        {
            _field = field;
            _value = value;
            _dataType = dataType;
        }

        public Filter CreateFilter()
        {
            var expression = new FilterExpression();
            var dataType = PropertyTypes.Parse(_dataType);
            var value = Convert.ChangeType(_value, dataType);

            if (dataType == typeof(int))
                return expression.NotEqual(_field, (int) value);

            if (dataType == typeof(string))
                return expression.NotEqual(_field, (string)value);

            if (dataType == typeof(DateTime))
                return expression.NotEqual(_field, (DateTime)value);

			if (dataType == typeof(decimal))
				return expression.NotEqual(_field, (decimal)value);

			throw new NotSupportedException("Unsupported data type: " + _dataType);
        }
    }
}