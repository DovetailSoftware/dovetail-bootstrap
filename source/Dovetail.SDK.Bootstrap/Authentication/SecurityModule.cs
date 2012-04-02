using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using StructureMap;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Authentication
{
    public class WebsiteSettings
    {
        public string AnonymousAccessFileExtensions { get; set; }
    }

    public class SecurityModule : IHttpModule
    {
        public const string DefaultExtensionWhiteList = "gif, jpg, css, js, png, htm, html";
        private HashSet<string> _whiteListExtensions;
        
        public void Init(HttpApplication context)
        {
            var ignoredFilesSetting = ObjectFactory.GetInstance<WebsiteSettings>().AnonymousAccessFileExtensions;
            if (ignoredFilesSetting.IsEmpty())
            {
                Trace.WriteLine("Whitelisting authentication for default file extensions: {0}", DefaultExtensionWhiteList);
                ignoredFilesSetting = DefaultExtensionWhiteList;
            }
            else
            {
                Trace.WriteLine("Whitelisting authentication for file extensions from settings : {0}", ignoredFilesSetting);
            }
            InitializeWhiteList(ignoredFilesSetting);

            context.AuthenticateRequest += onContextOnAuthenticateRequest;
        }

        private void onContextOnAuthenticateRequest(object sender, EventArgs e)
        {
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.IsAuthenticated && PathRequiresPrincipal(httpRequest.Path))
            {
                ObjectFactory.Container.GetInstance<IAuthenticationContextService>().SetupAuthenticationContext();
            }
        }

        public void InitializeWhiteList(string ignoredFilesSetting)
        {
            _whiteListExtensions = new HashSet<string>(GetWhiteListedExtensions(ignoredFilesSetting), StringComparer.OrdinalIgnoreCase);
        }

        public static IEnumerable<string> GetWhiteListedExtensions(string extensionSetting)
        {
            return extensionSetting
                .Split(new[] {',', ';', ' '}, StringSplitOptions.RemoveEmptyEntries)
                .Select(orig => orig[0] == '.' ? orig : "." + orig);
        }

        public bool PathRequiresPrincipal(string path)
        {
            if (path.IsEmpty()) return true;
            var extension = Path.GetExtension(path) ?? "";
            var pathRequiresPrincipal = !_whiteListExtensions.Contains(extension);
            if (!pathRequiresPrincipal)
            {
                Trace.WriteLine("Not loading principal for whitelisted extension on " + path);
            }
            return pathRequiresPrincipal;
        }
        
        public void Dispose()
        {
        }
    }
}