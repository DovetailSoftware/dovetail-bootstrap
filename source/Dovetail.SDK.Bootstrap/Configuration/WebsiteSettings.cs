namespace Dovetail.SDK.Bootstrap.Configuration
{
	public class WebsiteSettings
	{
		public string ApplicationName { get; set; }
		public string AnonymousAccessFileExtensions { get; set; }
		public string PublicRootUrl { get; set; }
		public bool IsPublicRootVirtual { get; set; }
	}
}