using System;
using System.Linq;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.Clarify
{
    public class ClarifySessionListener : IClarifySessionListener
    {
        private readonly ILocaleCache _locale;

        public ClarifySessionListener(ILocaleCache locale)
        {
            _locale = locale;
        }

        public void Created(IClarifySession session)
        {
            var utcTimezone = _locale.TimeZones.FirstOrDefault(t => t.UtcOffsetSeconds == 0);

            if (utcTimezone == null)
                throw new ApplicationException("No timezone with a zero GMT offset was found.");

            var clarify = session.AsClarifySession();

            clarify.LocalTimeZone = utcTimezone;
            clarify.SetNullStringsToEmpty = true;

            // This option is here because we do not check schema field sizes when writing to the database.
            // It could be removed if this is resolved in the future.
            clarify.TruncateStringFields = true;
        }

        public void Started(IClarifySession session)
        {
        }

        public void Closed(IClarifySession session)
        {
        }
    }
}