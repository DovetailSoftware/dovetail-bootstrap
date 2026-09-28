using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;
using NUnit.Framework;
using FubuCore;
using Rhino.Mocks;

namespace Dovetail.SDK.Bootstrap.Tests
{
	[TestFixture]
	public class history_item_parser : Context<HistoryItemParser>
	{
		private ILogger _logger;

		public override void OverrideMocks()
		{
			_logger = MockRepository.GenerateStub<ILogger>();
			var historyParser = new HistoryParsers(new HistorySettings(), new HistoryOriginalMessageConfiguration(_logger));
			Override(historyParser);
		}

		[Test]
		public void finds_all_items()
		{
			const string input = "item1\r\nitem2\nitem3";

			var items = _cut.ParseContent(input).ToArray();

			items[0].ToString().ShouldEqual("item1");
			items[1].ToString().ShouldEqual("item2");
			items[2].ToString().ShouldEqual("item3");
		}

		[Test]
		public void email_log_finds_email_headers_mixed_with_items()
		{
			const string input = HistoryParsers.BEGIN_EMAIL_LOG_HEADER + "to: kmiller@dovetailsoftware.com\r\n" + HistoryParsers.END_EMAIL_LOG_HEADER + "item2\r\nitem3";

			var emailHeader = _cut.ParseEmailLog(input);

			var emailHeaderItem = emailHeader.Header.Headers.First();
			emailHeaderItem.Title.ShouldEqual("to");
			emailHeaderItem.Text.ShouldEqual("kmiller@dovetailsoftware.com");

			var i = emailHeader.Items.ToArray();
			i[0].ToString().ShouldEqual("item2");
			i[1].ToString().ShouldEqual("item3");
		}
		
		[Test]
		public void email_log_finds_original_messages_mixed_with_items()
		{
			const string input = HistoryParsers.BEGIN_EMAIL_LOG_HEADER + "to: kmiller@dovetailsoftware.com\r\n" + HistoryParsers.END_EMAIL_LOG_HEADER + "item1\rOn some day, Kevin Miller wrote:\r\noriginal item 1\r\nto: adude@needs.com\r\nfrom: kmiller@dt.com";
			
			var items = _cut.ParseEmailLog(input).Items.ToArray();
			items[0].GetType().CanBeCastTo<Line>().ShouldBeTrue();
			items[1].GetType().CanBeCastTo<OriginalMessage>().ShouldBeTrue();

			var original = (OriginalMessage)items[1];
			original.Header.ShouldContain("Kevin Miller wrote");
			original.Items.Count().ShouldEqual(2);
		}

		[Test, Timeout(10000)]
		public void email_log_with_a_large_body_and_few_colons_is_parsed_in_linear_time()
		{
			// Each line that is not a header used to be scanned up to the next ':' in the whole email,
			// so a large email with few colons took minutes to parse and timed out the history request.
			var line = String.Join(" ", Enumerable.Repeat("typing more log notes on the show case screen.", 20).ToArray());
			var body = String.Join("\n\n", Enumerable.Repeat(line, 1000).ToArray());
			var input = HistoryParsers.BEGIN_EMAIL_LOG_HEADER + "From: jpope\nTo: support\n" + HistoryParsers.END_EMAIL_LOG_HEADER + "\n" + body;

			var emailLog = _cut.ParseEmailLog(input);

			emailLog.Header.Headers.Count().ShouldEqual(2);
			emailLog.Items.OfType<Line>().Count(l => l.Text.Trim() == line).ShouldEqual(1000);
		}

		[Test]
		public void email_that_cannot_be_parsed_returns_fake_emaillog()
		{
			const string input = "__BEGIN EMAIL_HEADER\r\npl-PL_HistoryBuilderTokens:LOG_EMAIL_FROM\r\npl-PL_HistoryBuilderTokens:LOG_EMAIL_DATE\r\npl-PL_HistoryBuilderTokens:LOG_EMAIL_TO\r\n__END EMAIL_HEADER\r\n\n\nThanks, Hank\r\n";
			_logger.Expect(e => e.LogError(Arg<string>.Matches(s => s.Contains(input)), Arg<Exception>.Is.Anything));

			var emailLog = _cut.ParseEmailLog(input);

			emailLog.Header.Headers.Count().ShouldEqual(0);
			emailLog.Items.Count().ShouldEqual(1);
			var item = emailLog.Items.First();
			((Line) item).Text.ShouldEqual(input);
		}

		[Test]
		public void email_that_cannot_be_parsed_logs_error()
		{
			const string input = "__BEGIN EMAIL_HEADER\r\npl-PL_HistoryBuilderTokens:LOG_EMAIL_FROM\r\npl-PL_HistoryBuilderTokens:LOG_EMAIL_DATE\r\npl-PL_HistoryBuilderTokens:LOG_EMAIL_TO\r\n__END EMAIL_HEADER\r\n\n\nThanks, Hank\r\n";

			_logger.Expect(e => e.LogError(Arg<string>.Matches(s => s.Contains(input)), Arg<Exception>.Is.Anything));

			_cut.ParseEmailLog(input);

			_logger.VerifyAllExpectations();
		}
	}

	public static class CultureExtension
	{
		public static void As(this CultureInfo asCulture, Action action)
		{
			var previousCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = asCulture;
			Thread.CurrentThread.CurrentUICulture = asCulture;

			try
			{
				action();
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = previousCulture;
				Thread.CurrentThread.CurrentUICulture = previousCulture;
			}
		}
	}
}