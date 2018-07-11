using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.ModelMap.Transforms;
using FubuCore;

namespace Dovetail.SDK.ModelMap
{
    public class ModelData : IEnumerable<object>
    {
        private readonly IDictionary<string, object> _values = new Dictionary<string, object>();
		private readonly IList<string> _tags = new List<string>();

        public string Name { get; set; }
		public string Entity { get; set; }

        public object this[string key]
        {
            get
            {
                return _values.ContainsKey(key) ? _values[key] : null;
            }
            set { _values[key] = value; }
        }

	    public void AddTag(string tag)
	    {
		    _tags.Add(tag);
	    }

        public ModelData Child(string key)
        {
            return Get<ModelData>(key);
        }

		public IEnumerable<ModelData> Children(string key)
		{
			return Get<IEnumerable<ModelData>>(key);
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
                if (child != null && pair.Key != ModelDataPath.This)
                {
                    values.Add(pair.Key, child.ToValues());
                    continue;
                }

                var children = pair.Value as IEnumerable<ModelData>;
                if (children != null)
                {
                    values.Add(pair.Key, children.Select(_ => _.ToValues()).ToArray());
                    continue;
                }

                values.Add(pair.Key, pair.Value);
            }

            return values;
        }

	    public bool IsEmpty()
	    {
		    return _values.Count == 0;
	    }

	    public IEnumerable<string> Tags
	    {
		    get { return _tags; }
	    }

        public IEnumerator<object> GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

	    public bool HasTag(string tag)
	    {
		    return _tags.Contains(tag);
	    }
    }
}
