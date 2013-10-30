using System.Globalization;
using FubuCore.Descriptions;
using FubuLocalization;
using FubuMVC.Core;
using FubuMVC.Core.Behaviors;

namespace Bootstrap.Web.Security
{
	public class LocalizationSettings
	{
		public LocalizationSettings()
		{
			Culture = System.Threading.Thread.CurrentThread.CurrentUICulture.ToString();
		}

		public string Culture { get; set; }
	}
	/// <summary>
	/// Sets the current culture context based on the user's preference 
	/// </summary>
	public class BasicLocalizationBehavior : BasicBehavior, DescribesItself
	{
		private readonly ICurrentCultureContext _cultureContext;
		private readonly LocalizationSettings _settings;

		public BasicLocalizationBehavior(ICurrentCultureContext cultureContext, LocalizationSettings settings)
			: base(PartialBehavior.Ignored)
		{
			_cultureContext = cultureContext;
			_settings = settings;
		}

		protected override DoNext performInvoke()
		{
			// sets the culture information
			var cultureInfo = new CultureInfo(_settings.Culture);
			_cultureContext.CurrentCulture = cultureInfo;
			_cultureContext.CurrentUICulture = cultureInfo;

			return DoNext.Continue;
		}

		public void Describe(Description description)
		{
			description.ShortDescription = "Set the CurrentCulture and CurrentUICulture based app setting.";
		}
	}
}