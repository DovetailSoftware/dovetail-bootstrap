using System;
using FChoice.Foundation.Clarify;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration
{
    [TestFixture]
	public class Temporal_extensionT
	{
		private ISystemTime _systemTime;

		[SetUp]
		public void beforeEach()
		{
			_systemTime = new SystemTime();
		}

		[Test]
		public void should_describe_date_relative_to_today()
		{
			_systemTime.Now.DescribeDateRelativeToToday().ShouldEqual("Today");
			
			var yesterday = _systemTime.Now.Subtract(TimeSpan.FromDays(1));
			
			yesterday.DescribeDateRelativeToToday().ShouldEqual("Yesterday");

			var twoDaysAgo = _systemTime.Now.Subtract(TimeSpan.FromDays(2));
			
			twoDaysAgo.DescribeDateRelativeToToday().ShouldEqual(twoDaysAgo.ToString("D"));
		}
        
		[Test]
		public void should_describe_timespan()
		{
			TimeSpan.FromSeconds(0).Describe().ShouldEqual("Just Now");
			TimeSpan.FromSeconds(14).Describe().ShouldEqual("14 seconds ago");
			TimeSpan.FromSeconds(59).Describe().ShouldEqual("59 seconds ago");
			TimeSpan.FromSeconds(61).Describe().ShouldEqual("1 minute ago");
			TimeSpan.FromMinutes(3.49).Describe().ShouldEqual("3 minutes ago");
			TimeSpan.FromMinutes(3.5).Describe().ShouldEqual("4 minutes ago");
			TimeSpan.FromHours(1.01).Describe().ShouldEqual("1 hour ago");
			TimeSpan.FromHours(2.31).Describe().ShouldEqual("2 hours ago");
			TimeSpan.FromHours(2.51).Describe().ShouldEqual("3 hours ago");
			TimeSpan.FromHours(2.98).Describe().ShouldEqual("3 hours ago");
			TimeSpan.FromDays(3.23).Describe().ShouldEqual("3 days ago");
			TimeSpan.FromDays(365 * 100).Describe().ShouldEqual("100 years ago");
			TimeSpan.FromDays(365 * 201).Describe().ShouldEqual("201 years ago");
			TimeSpan.FromDays(365 * 2001).Describe().ShouldEqual("2001 years ago");
		}

		[Test]
		public void should_not_have_a_suffix_if_it_is_a_negative_timespan()
		{
			TimeSpan.FromSeconds(-2).Describe().ShouldEqual("In 2 seconds");
			TimeSpan.FromDays(-3.23).Describe().ShouldEqual("In 3 days");
		}

		[Test]
		public void should_return_human_readable_description_for_a_date_from_right_now()
		{
			_systemTime.Now.Add(TimeSpan.FromSeconds(45)).ElapsedTimeDescription(_systemTime).ShouldEqual("In 45 seconds");
			_systemTime.Now.Add(TimeSpan.FromSeconds(62)).ElapsedTimeDescription(_systemTime).ShouldEqual("In 1 minute");
			_systemTime.Now.Add(TimeSpan.FromMinutes(45)).ElapsedTimeDescription(_systemTime).ShouldEqual("In 45 minutes");
			_systemTime.Now.Add(TimeSpan.FromMinutes(120)).ElapsedTimeDescription(_systemTime).ShouldEqual("In 2 hours");
			_systemTime.Now.Add(TimeSpan.FromDays(1.02)).ElapsedTimeDescription(_systemTime).ShouldEqual("In 1 day");

			_systemTime.Now.Subtract(TimeSpan.FromSeconds(62)).ElapsedTimeDescription(_systemTime).ShouldEqual("1 minute ago");
			_systemTime.Now.Subtract(TimeSpan.FromDays(4.51)).ElapsedTimeDescription(_systemTime).ShouldEqual("5 days ago");

			_systemTime.Now.Subtract(TimeSpan.FromDays(4.51)).ElapsedTimeDescription(false, _systemTime.Now).ShouldEqual("5 days");
		}

		[Test]
		public void should_return_a_human_readable_description_of_a_timespan_including_days_hours_and_minutes()
		{
			TimeSpan.FromSeconds(-10).DescribeAsDaysHoursMinutes().ShouldEqual("0 minutes");
			TimeSpan.FromSeconds(0).DescribeAsDaysHoursMinutes().ShouldEqual("0 minutes");
			TimeSpan.FromSeconds(25).DescribeAsDaysHoursMinutes().ShouldEqual("0 minutes");

			TimeSpan.FromSeconds(65).DescribeAsDaysHoursMinutes().ShouldEqual("1 minute");

			TimeSpan.FromMinutes(25).DescribeAsDaysHoursMinutes().ShouldEqual("25 minutes");

			TimeSpan.FromHours(1.25).DescribeAsDaysHoursMinutes().ShouldEqual("1 hour, 15 minutes");

			TimeSpan.FromHours(25).DescribeAsDaysHoursMinutes().ShouldEqual("1 day, 1 hour");

			TimeSpan.FromHours(25.25).DescribeAsDaysHoursMinutes().ShouldEqual("1 day, 1 hour, 15 minutes");
			TimeSpan.FromHours(27.25).DescribeAsDaysHoursMinutes().ShouldEqual("1 day, 3 hours, 15 minutes");

			TimeSpan.FromHours(49.25).DescribeAsDaysHoursMinutes().ShouldEqual("2 days, 1 hour, 15 minutes");

			new TimeSpan(400, 1, 1, 1).DescribeAsDaysHoursMinutes().ShouldEqual("400 days, 1 hour, 1 minute");
		}

		[Test]
		public void when_showing_tense_is_turned_off_prefix_and_suffix_should_not_be_emitted()
		{
			TimeSpan.FromHours(2.98).Describe(false).ShouldEqual("3 hours");
			TimeSpan.FromDays(3.23).Describe(false).ShouldEqual("3 days");
			_systemTime.Now.Subtract(TimeSpan.FromDays(4.51)).ElapsedTimeDescription(false, _systemTime.Now).ShouldEqual("5 days");
		}

		[Test]
		public void should_not_display_any_output_for_special_clarify_min_date()
		{
			ClarifyGeneric.MIN_DATE.ElapsedTimeDescription(_systemTime).ShouldBeEmpty();
		}
	}
}