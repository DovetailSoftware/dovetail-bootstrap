using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dovetail.SDK.Bootstrap.Configuration;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Authentication
{
	public interface IRequestPathAuthenticationPolicy
	{
		bool PathRequiresAuthentication(string path);
	}

	public class RequestPathAuthenticationPolicy : IRequestPathAuthenticationPolicy
	{
		private readonly WebsiteSettings _settings;
		private readonly ILogger _logger;
		public const string DefaultExtensionWhiteList = "gif, jpg, css, js, png, htm, html, ico";
		private readonly HashSet<string> _whiteListExtensions;

		public RequestPathAuthenticationPolicy(WebsiteSettings settings, ILogger logger)
		{
			_settings = settings;
			_logger = logger;
			var ignoredFilesSetting = _settings.AnonymousAccessFileExtensions;
			if (ignoredFilesSetting.IsEmpty())
			{
				_logger.LogDebug("Whitelisting authentication for default file extensions: {0}", DefaultExtensionWhiteList);
				ignoredFilesSetting = DefaultExtensionWhiteList;
			}
			else
			{
				_logger.LogDebug("Whitelisting authentication for file extensions from settings : {0}", ignoredFilesSetting);
			}
			_whiteListExtensions = new HashSet<string>(GetWhiteListedExtensions(ignoredFilesSetting), StringComparer.OrdinalIgnoreCase);
		}

		public static IEnumerable<string> GetWhiteListedExtensions(string extensionSetting)
		{
			return extensionSetting
				.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(orig => orig[0] == '.' ? orig : "." + orig);
		}

		public bool PathRequiresAuthentication(string path)
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
	}
}