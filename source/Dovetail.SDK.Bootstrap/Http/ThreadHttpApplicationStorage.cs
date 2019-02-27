using System.Collections.Generic;

namespace Dovetail.SDK.Bootstrap.Http
{
	public class ThreadHttpApplicationStorage : IHttpApplicationStorage
	{
		private static readonly IDictionary<string, object> Values = new Dictionary<string, object>();

		public bool Has(string key)
		{
			return Values.ContainsKey(key);
		}

		public void Store(string key, object value)
		{
			Values.Add(key, value);
		}

		public object Get(string key)
		{
			return Values[key];
		}

		public void Remove(string key)
		{
			Values.Remove(key);
		}
	}
}
