using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FubuCore;
using Sprache;

namespace Dovetail.SDK.Bootstrap.History.Parser
{
    public interface IItem { }

    public class Content : IItem
    {
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public class HardRule : IItem { }

    public class EmailHeaderItem
    {
        public string Title { get; set; }
        public string Text { get; set; }
    }

    public class EmailHeader : IItem
    {
        public IEnumerable<EmailHeaderItem> Headers { get; set; }
    }

    public class Document
    {
        public IEnumerable<IItem> Items { get; set; }
    }

    public static class HistoryParsers
    {
        public static readonly Parser<string> HardRule =
            from text in Parse.Char('-').Many().Text().Token()
            select text; 
        public static readonly Parser<char> UntilEndOfLine = Parse.CharExcept(c => c == '\r' || c == '\n', "start of line ending");

        public static readonly string[] EmailHeaderFields = new[] { "date", "from", "to", "send to", "cc", "subject", "sent" };
        
        public static readonly Parser<EmailHeaderItem> EmailHeaderItem =
            from title in Parse.CharExcept(':').Many().Text().Token()
            where EmailHeaderFields.Any(h => h.Equals(title, StringComparison.InvariantCultureIgnoreCase))
            from _1 in Parse.Char(':')
            from text in UntilEndOfLine.Many().Token().Text()
            from rest in Parse.WhiteSpace.Many()
            from _2 in HardRule
            select new EmailHeaderItem { Title = title, Text = text };

        public static readonly Parser<EmailHeader> EmailHeader =
            from items in EmailHeaderItem.Many()
            select new EmailHeader {Headers = items};

        public static readonly Parser<Content> Content =
            from text in UntilEndOfLine.Many().Text().Token()
            select new Content {Text = text.TrimEnd()};

        public static readonly Parser<IItem> Item =
            EmailHeader.Select(n => (IItem) n)
                .Or(Content);

        public static readonly Parser<Document> Document =
            from items in Item.Many().End()
            select new Document {Items = items};
    }

    public class HistoryItemParser
    {
        public IEnumerable<IItem> Parse(string input)
        {
            return HistoryParsers.Item.Many().End().Parse(input);
        }
    }

    public class HistoryItemHtmlRenderer
    {
        private StringBuilder _output;

        public string Render(IEnumerable<IItem> items)
        {
            _output = new StringBuilder();
            foreach (var item in items)
            {
                if(item.GetType().CanBeCastTo<EmailHeader>())
                {
                    renderEmailHeader(item as EmailHeader);
                    continue;
                }

                if (item.GetType().CanBeCastTo<HardRule>())
                {
                    _output.AppendLine("<hr/>");
                    continue;
                }

                renderItem(item);
            }

            return _output.ToString();
        }

        private void renderEmailHeader(EmailHeader emailHeader)
        {
            _output.AppendLine(@"<div class=""history-email-header""><ul>");

            foreach (var header in emailHeader.Headers)
            {
                _output.AppendLine(@"<li><span class=""email-header-name"">{0}</span> <span class=""email-header-text"">{1}</span></li>".ToFormat(header.Title.ToLower().Capitalize(), header.Text));
            }

            _output.Append(@"</ul></div>");
        }

        private void  renderItem(IItem item)
        {
            _output.AppendLine("<p>{0}</p>".ToFormat(item));
        }
    }
}