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
	public class history_item_parser
	{
		private HistoryItemParser _itemParser;
		private ILogger _logger;

		[SetUp]
		public void beforeEach()
		{
			_logger = MockRepository.GenerateStub<ILogger>();
			_itemParser = new HistoryItemParser(new HistoryParsers(new HistorySettings()), _logger);
		}

		[Test]
		public void finds_all_items()
		{
			const string input = "item1\r\nitem2\nitem3";

			var items = _itemParser.ParseContent(input).ToArray();

			items[0].ToString().ShouldEqual("item1");
			items[1].ToString().ShouldEqual("item2");
			items[2].ToString().ShouldEqual("item3");
		}

		[Test]
		public void email_log_finds_email_headers_mixed_with_items()
		{
			const string input = HistoryParsers.BEGIN_EMAIL_LOG_HEADER + "to: kmiller@dovetailsoftware.com\r\n" + HistoryParsers.END_EMAIL_LOG_HEADER + "item2\r\nitem3";

			var emailHeader = _itemParser.ParseEmailLog(input);

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
			
			var items = _itemParser.ParseEmailLog(input).Items.ToArray();
			items[0].GetType().CanBeCastTo<Line>().ShouldBeTrue();
			items[1].GetType().CanBeCastTo<OriginalMessage>().ShouldBeTrue();

			var original = (OriginalMessage)items[1];
			original.Header.ShouldContain("Kevin Miller wrote");
			original.Items.Count().ShouldEqual(2);
		}

		[Test]
		public void email_that_cannot_be_parsed_returns_fake_emaillog()
		{
			const string input = "__BEGIN EMAIL_HEADER\r\npl-PL_HistoryBuilderTokens:LOG_EMAIL_FROM\r\npl-PL_HistoryBuilderTokens:LOG_EMAIL_DATE\r\npl-PL_HistoryBuilderTokens:LOG_EMAIL_TO\r\n__END EMAIL_HEADER\r\n\n\nThanks, Hank\r\n";
			_logger.Expect(e => e.LogError(Arg<string>.Matches(s => s.Contains(input)), Arg<Exception>.Is.Anything));

			var emailLog = _itemParser.ParseEmailLog(input);

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

			_itemParser.ParseEmailLog(input);

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