using System.Web;

namespace Dovetail.SDK.Bootstrap.Configuration
{
	public interface IWebApplicationUrl
	{
		string Url { get; }
	}

	public class AspNetWebApplicationUrl : IWebApplicationUrl
	{
		public HttpContextWrapper Context { get; set; }

		public AspNetWebApplicationUrl()
		{
			if(HttpContext.Current != null)
			{
				Context = new HttpContextWrapper(HttpContext.Current);
			}
		}

		public string Url { get { return getUrl(); } }

		private string getUrl()
		{
			if (Context == null)
				return string.Empty;

			var request = Context.Request;

			return string.Format("{0}://{1}{2}", request.Url.Scheme, request.ServerVariables["HTTP_HOST"], (request.ApplicationPath.Equals("/")) ? string.Empty : request.ApplicationPath);
		}
	}
}