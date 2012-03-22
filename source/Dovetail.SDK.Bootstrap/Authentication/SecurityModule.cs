using System;
using System.Collections.Generic;
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
        private ILogger _logger;

        public void Init(HttpApplication context)
        {
            _logger = ObjectFactory.GetInstance<ILogger>();

            InitializeWhiteList();

            context.AuthenticateRequest += (sender, e) =>
                                               {
                                                   var httpRequest = HttpContext.Current.Request;
                                                   if (httpRequest.IsAuthenticated && PathRequiresPrincipal(httpRequest.Path))
                                                   {
                                                       ObjectFactory.Container.GetInstance<IAuthenticationContextService>().SetupAuthenticationContext();
                                                   }
                                               };

            context.EndRequest += context_EndRequest;
        }

        public void InitializeWhiteList()
        {
            var ignoredFilesSetting = ObjectFactory.GetInstance<WebsiteSettings>().AnonymousAccessFileExtensions;
            if (ignoredFilesSetting.IsEmpty())
            {
                _logger.LogInfo("Whitelisting authentication for default file extensions: {0}", DefaultExtensionWhiteList);
                ignoredFilesSetting = DefaultExtensionWhiteList;
            }
            else
            {
                _logger.LogInfo("Whitelisting authentication for file extensions from settings : {0}", ignoredFilesSetting);
            }

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
                _logger.LogDebug("Not loading principal for whitelisted extension on " + path);
            }
            return pathRequiresPrincipal;
        }


        protected void context_EndRequest(object sender, EventArgs e)
        {
            var request = HttpContext.Current.Request;
            var response = HttpContext.Current.Response;

            if ((request.HttpMethod != "POST") || (response.StatusCode != 404 || response.SubStatusCode != 13)) return;

            throw new ApplicationException("The size of your request was too large.");
        }


        public void Dispose()
        {
        }
    }
}