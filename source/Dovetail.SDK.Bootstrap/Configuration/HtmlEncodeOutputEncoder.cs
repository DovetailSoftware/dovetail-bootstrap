using System.Web;

namespace Dovetail.SDK.Bootstrap.Configuration
{
	public class HtmlEncodeOutputEncoder : IOutputEncoder
    {
        public string Encode(string encodeMe)
        {
            return HttpUtility.HtmlEncode(encodeMe);
        }
    }
}