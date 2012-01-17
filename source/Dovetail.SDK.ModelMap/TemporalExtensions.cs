using System;
using System.Collections.Generic;
using System.Linq;
using FChoice.Foundation;
using FubuCore;

namespace Dovetail.SDK.ModelMap
{
    public interface ISystemTime
    {
        DateTime Now { get; }
    }

    public class SystemTime : ISystemTime
    {
        public DateTime Now
        {
            get { return DateTime.Now; }
        }
    }


    public static class TemporalExtensions
	{
		private static IEnumerable<ElapsedTimeSlot> _timeFormats;

		public static IEnumerable<ElapsedTimeSlot> TimeFormats
		{
			get
			{
				if (_timeFormats == null)
				{
					initializeFormats();
				}

				return _timeFormats;
			}
		}

		private static void initializeFormats()
		{
			_timeFormats = new List<ElapsedTimeSlot>
				{
					new ElapsedTimeSlot {CutOffInSeconds = 1, SpanInSeconds = null, Description = "Just Now", Suffix = String.Empty, Prefix = String.Empty},
					new ElapsedTimeSlot {CutOffInSeconds = 60, SpanInSeconds = 1, Description = "seconds"},
					new ElapsedTimeSlot {CutOffInSeconds = 90, SpanInSeconds = null, Description = "1 minute"},
					new ElapsedTimeSlot {CutOffInSeconds = 3600, SpanInSeconds = 60, Description = "minutes"},
					new ElapsedTimeSlot {CutOffInSeconds = 5400, SpanInSeconds = null, Description = "1 hour"},
					new ElapsedTimeSlot {CutOffInSeconds = 86400, SpanInSeconds = 3600, Description = "hours"},
					new ElapsedTimeSlot {CutOffInSeconds = 129600, SpanInSeconds = null, Description = "1 day"},
					new ElapsedTimeSlot {CutOffInSeconds = 604800, SpanInSeconds = 86400, Description = "days"},
					new ElapsedTimeSlot {CutOffInSeconds = 907200, SpanInSeconds = null, Description = "1 week"},
					new ElapsedTimeSlot {CutOffInSeconds = 2628000, SpanInSeconds = 604800, Description = "weeks"},
					new ElapsedTimeSlot {CutOffInSeconds = 3942000, SpanInSeconds = null, Description = "1 month"},
					new ElapsedTimeSlot {CutOffInSeconds = 31536000, SpanInSeconds = 2628000, Description = "months"},
					new ElapsedTimeSlot {CutOffInSeconds = 47304000, SpanInSeconds = null, Description = "1 year"},
					new ElapsedTimeSlot {CutOffInSeconds = long.MaxValue, SpanInSeconds = 31536000, Description = "years"},
				};
		}

		public static string DescribeAsDaysHoursMinutes(this TimeSpan elapsedTimespan)
		{
			if (elapsedTimespan.TotalMinutes < 1)
				return "0 minutes";

			var totalDays = Math.Floor(elapsedTimespan.TotalDays);

			var elapsedTimeSpanMinusDays = elapsedTimespan.Subtract(TimeSpan.FromDays(totalDays));
			var totalHours = Math.Floor(elapsedTimeSpanMinusDays.TotalHours);

			var elapsedTimespanMinusDaysAndHours = elapsedTimeSpanMinusDays.Subtract(TimeSpan.FromHours(totalHours));
			var totalMinutes = Math.Floor(elapsedTimespanMinusDaysAndHours.TotalMinutes);

			var result = String.Empty;
			if (totalDays > 0)
			{
				result += "{0} day{1}".ToFormat(totalDays, (totalDays > 1) ? "s" : "");
			}

			if (totalHours > 0)
			{
				result += ", {0} hour{1}".ToFormat(totalHours, (totalHours > 1) ? "s" : "");
			}

			if (totalMinutes > 0)
			{
				result += ", {0} minute{1}".ToFormat(totalMinutes, (totalMinutes > 1) ? "s" : "");
			}

			return result.TrimStart(',').Trim();
		}

		public static string Describe(this TimeSpan elapsedTimespan)
		{
			var elapsedTimeSlot = findElapsedTimeSlot(elapsedTimespan);

			if (elapsedTimeSlot == null)
				return String.Empty;

			return elapsedTimeSlot.GetPrettyElapsedTimeFormat(elapsedTimespan, true);
		}

		public static string Describe(this TimeSpan elapsedTimespan, bool showTense)
		{
			return findElapsedTimeSlot(elapsedTimespan).GetPrettyElapsedTimeFormat(elapsedTimespan, showTense);
		}

		public static string ElapsedTimeDescription(this DateTime describeMe, ISystemTime systemTime)
		{
			return ElapsedTimeDescription(describeMe, true, systemTime.Now);
		}

		public static string ElapsedTimeDescription(this DateTime describeMe, DateTime basedOn)
		{
			return ElapsedTimeDescription(describeMe, true, basedOn);
		}


		public static string ElapsedTimeDescription(this DateTime describeMe, bool showTense, DateTime basedOn)
		{
			if (describeMe == FCGeneric.MIN_DATE)
				return String.Empty;

			return basedOn.Subtract(describeMe).Describe(showTense);
		}

		public static string DescribeDateRelativeToToday(this DateTime describeMe)
		{
			var now = DateTime.Now;
			if (now.Date == describeMe.Date)
			{
				return "Today";
			}

			if (now.Subtract(TimeSpan.FromDays(1)).Date == describeMe.Date)
			{
				return "Yesterday";
			}

			return describeMe.Date.ToString("D");
		}

		private static ElapsedTimeSlot findElapsedTimeSlot(TimeSpan elapsedTimespan)
		{
			var elapsedTimeInSeconds = (long)Math.Abs(elapsedTimespan.TotalSeconds);
			return (TimeFormats.Where(t => t.CutOffInSeconds >= elapsedTimeInSeconds)).FirstOrDefault();
		}


		public class ElapsedTimeSlot
		{
			private string _prefix = "In ";
			private string _suffix = " ago";
			public long CutOffInSeconds { get; set; }
			public string Description { get; set; }
			public long? SpanInSeconds { get; set; }

			public string Prefix
			{
				get { return _prefix; }
				set { _prefix = value; }
			}
			public string Suffix
			{
				get { return _suffix; }
				set { _suffix = value; }
			}

			public string GetPrettyElapsedTimeFormat(TimeSpan timeSpan, bool showTense)
			{
				var suffix = GetTenseSuffix(timeSpan, showTense);
				var prefix = suffix.IsEmpty() && showTense && timeSpan.TotalSeconds != 0 ? Prefix : string.Empty;

				if (!SpanInSeconds.HasValue)
				{
					return "{0}{1}{2}".ToFormat(prefix, Description, suffix);
				}

				var spanLength = Math.Abs((double)(timeSpan.TotalSeconds / SpanInSeconds));

				return "{0}{1:.} {2}{3}".ToFormat(prefix, spanLength, Description, suffix);
			}

			private string GetTenseSuffix(TimeSpan timeSpan, bool showTense)
			{
				if (showTense == false || timeSpan.TotalSeconds < 0)
				{
					return String.Empty;
				}

				return Suffix;
			}
		}
	}
}