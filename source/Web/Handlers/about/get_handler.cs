using System.Reflection;

namespace Bootstrap.Web.Handlers.about
{
    public class get_handler
    {
        public AboutModel Execute(AboutRequest request)
        {
            return new AboutModel{ Version = Assembly.GetCallingAssembly().GetName().Version.ToString()};
        }
    }
    
    public class AboutRequest
    {
    }

    public class AboutModel 
    {
        public string Version { get; set; }
    }
}