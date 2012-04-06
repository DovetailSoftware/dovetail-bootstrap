using System;
using System.Linq;
using Dovetail.SDK.Bootstrap.History.Parser;
using NUnit.Framework;
using Sprache;

namespace Dovetail.SDK.Bootstrap.Tests
{
    [TestFixture]
    public class history_parsers
    {
        [Test]
        public void detect_item()
        {
            const string input = "line1\r\nline2";

            var item = HistoryParsers.Item.Parse(input);

            item.ToString().ShouldEqual("line1");
        }

        [Test]
        public void unicode_non_breaking_alone_not_considered_a_line()
        {
            const string input = "line1\r\n&#160;\r\nline2";

            var items = HistoryParsers.Item.Many().Parse(input).ToArray();

            items[0].ToString().ShouldEqual("line1");
            items[1].ToString().ShouldEqual("line2");
        }

        [Test]
        public void item_white_space_is_removed()
        {
            const string input = "   line1 \r\n   line2    ";

            var items = HistoryParsers.Item.Many().Parse(input).ToArray();

            items[0].ToString().ShouldEqual("line1");
            items[1].ToString().ShouldEqual("line2");
        }

        [Test]
        public void email_header_items_are_detected()
        {
            const string input = "send to: yadda\nto:email@example.com\nsubject:math";

            var item = HistoryParsers.Item.Parse(input);

            var emailHeader = (item as EmailHeader);
            emailHeader.ShouldNotBeNull();
            emailHeader.Headers.Count().ShouldEqual(3);
        }

        [Test]
        public void email_header_properties_are_populated()
        {
            const string input = "from: yadda\r\nan item";

            var item = HistoryParsers.Item.Parse(input);

            var emailHeaderItem = ((EmailHeader)item).Headers.First();
            emailHeaderItem.Title.ShouldEqual("from");
            emailHeaderItem.Text.ShouldEqual("yadda");
        }

        [Test]
        public void email_header_titles_ignores_case()
        {
            const string input = "FROM: other content\r\nan item";

            var item = HistoryParsers.Item.Parse(input);

            var emailHeaderItem = ((EmailHeader)item).Headers.First();
            emailHeaderItem.Title.ShouldEqual("FROM");
            emailHeaderItem.Text.ShouldEqual("other content");
        }

        [Test]
        public void email_header_ignores_lines_with_only_dashes()
        {
            const string input = "FROM: other content\n----------------------\nTo: a gal";

            var item = HistoryParsers.Item.Parse(input);

            var emailHeaders = ((EmailHeader)item).Headers;

            emailHeaders.First().Title.ShouldEqual("FROM");
            emailHeaders.First().Text.ShouldEqual("other content");
            emailHeaders.Count().ShouldEqual(2);
        }

        [Test]
        public void item_similar_to_an_email_header_is_still_just_an_item()
        {
            const string input = "notaheader: yadda\r\nan item";

            var item = HistoryParsers.Item.Parse(input);

            (item as EmailHeader).ShouldBeNull();
        }

        [Test]
        public void quoted_content_is_rolled_into_a_block_quote_item()
        {
            const string input = "line1\n&gt; quote line1\n&gt; quote line2\n&gt; quote line3";

            var items = HistoryParsers.Item.Many().End().Parse(input).ToArray();

            items.Length.ShouldEqual(2);
            var blockQuote = (BlockQuote)items[1];
            blockQuote.Lines.Count().ShouldEqual(3);
        }

        [Test]
        public void original_message()
        {
            const string input = @"On Tue, Nov 3, 2009 at 12:34 PM, Sam Tyson <styson@gmail.com> wrote:

Here are the config files. Please let me know what else I can get you.

From: Sam Tyson [mailto:styson@gmail.com]
Sent: Tuesday, November 03, 2009 12:12 PM
To: Andy Hagerty
Subject: Re: test

test received

&gt; Thanks,
&gt;
&gt;  Sam Tyson
&gt;
&gt;  On Tue, Nov 3, 2009 at 11:09 AM, Andy Hagerty <andy.hagerty@consona.com&gt;
&gt; wrote:
&gt;
&gt; Test
&gt; Andy Hagerty";

            var items = HistoryParsers.Item.Many().End().Parse(input).ToArray();
            items.Length.ShouldEqual(1);
            
            var originalMessage = (OriginalMessage)items[0];
            originalMessage.Header.ShouldContain("styson@gmail.com");

            var originalMessageItems = originalMessage.Items.ToArray();
            originalMessageItems.Length.ShouldEqual(15);
        }
    }
}