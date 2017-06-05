using System;
using System.Collections.Generic;
using System.Linq;

namespace Dovetail.SDK.ModelMap.Serialization
{
    public class BuildObjectContext
    {
        private readonly Type _type;
        private readonly IDictionary<string, string> _values;

        public BuildObjectContext(Type type, IDictionary<string, string> values)
        {
            _type = type;
            _values = values.ToDictionary(_ => _.Key.ToLower(), _ => _.Value);
        }

        public Type Type
        {
            get { return _type; }
        }

        public bool Has(string key)
        {
            return _values
                .Keys
                .Select(_ => _.ToLower())
                .Contains(key.ToLower());
        }

        public object GetValue(string key, Type targetType)
        {
            object value = _values[key.ToLower()];
            if (value.GetType() != targetType)
                value = Convert.ChangeType(value, targetType);

            return value;
        }
    }
}