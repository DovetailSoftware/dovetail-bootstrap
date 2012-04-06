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

    public class EmailHeaderItem
    {
        public string Title { get; set; }
        public string Text { get; set; }
    }

    public class EmailHeader : IItem
    {
        public IEnumerable<EmailHeaderItem> Headers { get; set; }
    }

    public class BlockQuote : IItem
    {
        public IEnumerable<string> Lines { get; set; }
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

        public static readonly Parser<string> BlockQuoteLine =
            from leader in Parse.String("&gt;")
            from text in UntilEndOfLine.Many().Token().Text()
            select text;

        public static readonly Parser<BlockQuote> BlockQuote =
            from lines in BlockQuoteLine.Many()
            select new BlockQuote {Lines = lines};
        
        public static readonly Parser<EmailHeader> EmailHeader =
            from items in EmailHeaderItem.Many()
            select new EmailHeader {Headers = items};

        public static readonly Parser<Content> Content =
            from _1 in Parse.WhiteSpace.Many()
            from text in UntilEndOfLine.Many().Text().Token()
            select new Content {Text = text.TrimEnd()};

        public static readonly Parser<IItem> Item =
            from items in EmailHeader.Select(n => (IItem) n)
                .Or(BlockQuote)
                .Or(Content)
            select items;
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
        
        private static long _idIndex;
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

                if (item.GetType().CanBeCastTo<BlockQuote>())
                {
                    renderBlockQuote(item as BlockQuote);
                    continue;
                }
                renderItem(item);
            }

            return _output.ToString();
        }

        private void renderEmailHeader(EmailHeader emailHeader)
        {
            _idIndex += 1;
            var id = "histitem" + _idIndex;

            _output.AppendLine(@"<div id=""{0}"" class=""history-email-header collapse in""><i class=""icon-envelope"" title=""Click to expand"" data-toggle=""collapse"" data-target=""#{0}""></i><ul>".ToFormat(id));

            foreach (var header in emailHeader.Headers)
            {
                _output.AppendLine(@"<li><span class=""email-header-name"">{0}</span> <span class=""email-header-text"">{1}</span></li>".ToFormat(header.Title.ToLower().Capitalize(), header.Text));
            }

            _output.Append(@"</ul></div>");
        }

        private void renderBlockQuote(BlockQuote blockQuote)
        {
            _output.AppendLine(@"<blockquote>");

            foreach (var line in blockQuote.Lines)
            {
                _output.AppendLine(line + "<br/>");
            }

            _output.AppendLine(@"</blockquote>");
        }

        private void  renderItem(IItem item)
        {
            _output.AppendLine("<p>{0}</p>".ToFormat(item));
        }
    }
}