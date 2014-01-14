using System;
using System.Globalization;
using System.Linq;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;
using FubuCore;
using NUnit.Framework;
using Sprache;

namespace Dovetail.SDK.Bootstrap.Tests
{
	[TestFixture]
	public class history_parsers
	{
		private HistoryParsers _parser;

		[SetUp]
		public void beforeEach()
		{
			_parser = new HistoryParsers(new HistorySettings());
		}

		[Test]
		public void detect_item()
		{
			const string input = "line1\nline2";

			var item = _parser.EmailItem.Parse(input);

			item.ToString().ShouldEqual("line1");
		}

		[Test]
		public void detect_paragraph_end()
		{
			const string input = ParagraphEndLocator.ENDOFPARAGRAPHTOKEN;

			var p = _parser.ParagraphEnd.Parse(input);

			p.ShouldBeOfType<ParagraphEnd>();
		}

		[Test]
		public void detect_items_having_paragraph_end()
		{
			const string input = "line1\n" + ParagraphEndLocator.ENDOFPARAGRAPHTOKEN + "\nline2";

			var items = _parser.EmailItem.Many().Parse(input).ToArray();

			items[0].ToString().ShouldEqual("line1");
			items[1].ShouldBeOfType<ParagraphEnd>();
			items[2].ToString().ShouldEqual("line2");
		}

		[Test]
		public void unicode_non_breaking_alone_not_considered_a_line()
		{
			const string input = "line1\r\n&#160;\r\nline2";

			var items = _parser.EmailItem.Many().Parse(input).ToArray();

			items[0].ToString().ShouldEqual("line1");
			items[1].ToString().ShouldEqual("line2");
		}

		[Test]
		public void item_white_space_is_removed()
		{
			const string input = "   line1 \r\n   line2    ";

			var items = _parser.EmailItem.Many().Parse(input).ToArray();

			items[0].ToString().ShouldEqual("line1");
			items[1].ToString().ShouldEqual("line2");
		}

		[Test]
		public void email_header_items_are_detected()
		{
			const string input = "send to: yadda\nto:email@example.com\nsubject:math";

			var item = _parser.EmailItem.Parse(input);

			var emailHeader = (EmailHeader) item;
			emailHeader.ShouldNotBeNull();
			emailHeader.Headers.Count().ShouldEqual(3);
		}

		[Test]
		public void email_header_properties_are_populated()
		{
			const string input = "from: yadda\r\nan item";

			var item = _parser.EmailItem.Parse(input);

			var emailHeaderItem = ((EmailHeader) item).Headers.First();
			emailHeaderItem.Title.ShouldEqual("from");
			emailHeaderItem.Text.ShouldEqual("yadda");
		}

		[Test]
		public void email_header_titles_ignores_case()
		{
			const string input = "FROM: other content\r\nan item";

			var item = _parser.EmailItem.Parse(input);

			var emailHeaderItem = ((EmailHeader) item).Headers.First();
			emailHeaderItem.Title.ShouldEqual("FROM");
			emailHeaderItem.Text.ShouldEqual("other content");
		}

		[Test]
		public void email_header_ignores_lines_with_only_dashes()
		{
			const string input = "FROM: other content\n----------------------\nTo: a gal";

			var item = _parser.EmailItem.Parse(input);

			var emailHeaders = ((EmailHeader) item).Headers.ToArray();

			emailHeaders.First().Title.ShouldEqual("FROM");
			emailHeaders.First().Text.ShouldEqual("other content");
			emailHeaders.Count().ShouldEqual(2);
		}

		[Test]
		public void item_similar_to_an_email_header_is_still_just_an_item()
		{
			const string input = "notaheader: yadda\r\nan item";

			var item = _parser.EmailItem.Parse(input);

			(item as EmailHeader).ShouldBeNull();
		}

		[Test]
		public void quoted_content_is_rolled_into_a_block_quote_item()
		{
			const string input = "line1\n&gt; quote line1\n&gt; quote line2\n&gt; quote line3";

			var items = _parser.EmailItem.Many().End().Parse(input).ToArray();

			items.Length.ShouldEqual(2);
			var blockQuote = (BlockQuote) items[1];
			blockQuote.Lines.Count().ShouldEqual(3);
		}

		const string originalMessageInput = @"On Tue, Nov 3, 2009 at 12:34 PM, Sam Tyson <dude@gmail.com> wrote:

Here are the config files. Please let me know what else I can get you.

From: Dude Wee [mailto:dude@gmail.com]
Sent: Tuesday, November 03, 2009 12:12 PM
To: A Guy
Subject: Re: test

test received

&gt; Thanks,
&gt;
&gt;  Dude Wee
&gt;
&gt;  On Tue, Nov 3, 2009 at 11:09 AM, A Guy <guy@place.com&gt;
&gt; wrote:
&gt;
&gt; Test
&gt; A Guy";

		[Test]
		public void original_message()
		{
			var originalMessage = _parser.OriginalMessage.Parse(originalMessageInput);
			originalMessage.Header.ShouldContain("dude@gmail.com");

			verifyOriginalMessage(originalMessage);
		}

		[Test]
		public void email_item_parser_should_return_original_messages()
		{
			var items = _parser.EmailItem.Many().Parse(originalMessageInput).ToArray();
			items.Length.ShouldEqual(1);

			var originalMessage = (OriginalMessage) items[0];

			verifyOriginalMessage(originalMessage);
		}

		public void verifyOriginalMessage(OriginalMessage originalMessage)
		{
			originalMessage.Header.ShouldContain("dude@gmail.com");

			var originalMessageItems = originalMessage.Items.ToArray();
			originalMessageItems.Length.ShouldEqual(4);
		}

		[Test]
		public void iso_date()
		{
			var isoDate = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);
			var input = @"Date:{0}{1}".ToFormat(HistoryParsers.BEGIN_ISODATE_HEADER, isoDate);

			var emailHeaderItem = _parser.EmailHeaderItem.Parse(input);

			emailHeaderItem.Title.ShouldEqual("Date");
			emailHeaderItem.Text.ShouldContain("time-format");
			emailHeaderItem.Text.ShouldContain(isoDate);
		}

		[Test]
		public void email_header_with_iso_date()
		{
			var isoDate = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);
			var input = @"__BEGIN EMAIL_HEADER
From: annie
Date: __BEGIN_ISODATE_HEADER__    2013-10-29T14:56:12
To: mmiller@anotherexample.com
__END EMAIL_HEADER

fsdafasdfsafsaf
new sig".ToFormat(HistoryParsers.BEGIN_ISODATE_HEADER, isoDate);

			var header = _parser.LogEmailHeader.Parse(input);

			var headers = header.Headers.ToArray();

			headers.Count().ShouldEqual(3);
			headers[1].Text.ShouldContain("time-format");
		}

		[Test]
		public void email_logs_with_non_breaking_spaces_should_work()
		{
			const string input = @"__BEGIN EMAIL_HEADER
Date: __BEGIN_ISODATE_HEADER__2013-10-30T13:09:58
From: annie@localhost.fcs.local
To: support@dovetailsoftware.com
__END EMAIL_HEADER
Those kittens are adorable.&#160;&#160;&#160;
&#160;
SUBJECT: Re: Please create a case - About Case 30822
TO: admin@localhost.fcs.local
SENT: Wednesday, October 30, 2013 10:43 AM";

			var email = _parser.LogEmail.Parse(input);
			var items = email.Items.ToArray();
			items[0].ShouldBeOfType<Line>();
			items[1].ShouldBeOfType<EmailHeader>();
		}
	}
}