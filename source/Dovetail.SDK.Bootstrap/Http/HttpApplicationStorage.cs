using System.Linq;
using System.Web;

namespace Dovetail.SDK.Bootstrap.Http
{
	public class HttpApplicationStorage : IHttpApplicationStorage
	{
		private readonly HttpApplicationState _application;

		public HttpApplicationStorage()
		{
			_application = HttpContext.Current.ApplicationInstance.Application;
		}

		public object Get(string key)
		{
			return _application.Get(key);
		}

		public void Remove(string key)
		{
			_application.Remove(key);
		}

		public bool Has(string key)
		{
			return _application.AllKeys.Contains(key);
		}

		public void Store(string key, object value)
		{
			_application.Add(key, value);
		}
	}
}
