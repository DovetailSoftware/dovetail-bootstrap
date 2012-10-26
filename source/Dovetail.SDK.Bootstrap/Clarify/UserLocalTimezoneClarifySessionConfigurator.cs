using System;
using System.Linq;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface IUserClarifySessionConfigurator
	{
		void Configure(ClarifySession session);
	}

	public class UserLocalTimezoneClarifySessionConfigurator : IUserClarifySessionConfigurator
	{
		private readonly Func<ICurrentSDKUser> _user;

		public UserLocalTimezoneClarifySessionConfigurator(Func<ICurrentSDKUser> user)
		{
			_user = user;
		}

		public void Configure(ClarifySession session)
		{
			var currentSdkUser = _user();

			session.LocalTimeZone = currentSdkUser.Timezone;
		}
	}

	public class UTCTimezoneUserClarifySessionConfigurator : IUserClarifySessionConfigurator
	{
		private readonly ILocaleCache _localeCache;

		public UTCTimezoneUserClarifySessionConfigurator(ILocaleCache localeCache)
		{
			_localeCache = localeCache;
		}

		public void Configure(ClarifySession session)
		{
			var utcTimezone = _localeCache.TimeZones.FirstOrDefault(t => t.UtcOffsetSeconds == 0);

			if (utcTimezone == null)
				throw new ApplicationException("No timezone with a zero GMT offset was found.");

			session.LocalTimeZone = utcTimezone;
		}
	}
}