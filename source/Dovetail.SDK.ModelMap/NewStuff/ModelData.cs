using System.Collections;
using System.Collections.Generic;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public class ModelData : IEnumerable<object>
    {
        private readonly IDictionary<string, object> _values = new Dictionary<string, object>();

        public string Name { get; set; }

        public object this[string key]
        {
            get
            {
                return _values.ContainsKey(key) ? _values[key] : null;
            }
            set { _values[key] = value; }
        }

        public ModelData Child(string key)
        {
            return Get<ModelData>(key);
        }

        public T Get<T>(string key)
        {
            return this[key].As<T>();
        }

        public bool Has(string key)
        {
            return _values.ContainsKey(key);
        }

        public IDictionary<string, object> ToValues()
        {
            var values = new Dictionary<string, object>();
            foreach (var pair in _values)
            {
                var child = pair.Value as ModelData;
                if (child != null)
                {
                    values.Add(pair.Key, child.ToValues());
                    continue;
                }

                values.Add(pair.Key, pair.Value);
            }

            return values;
        }

        public IEnumerator<object> GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}