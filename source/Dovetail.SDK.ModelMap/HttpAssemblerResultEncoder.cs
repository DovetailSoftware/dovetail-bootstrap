using System.Web;

namespace Dovetail.SDK.ModelMap
{
    public class HttpAssemblerResultEncoder : IModelBuilderResultEncoder
    {
        public string Encode(string encodeMe)
        {
            return HttpUtility.HtmlEncode(encodeMe);
        }
    }
}