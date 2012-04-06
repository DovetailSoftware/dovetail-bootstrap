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

    public class OriginalMessage : IItem
    {
        public string Header { get; set; }
        public IEnumerable<IItem> Items { get; set; }
    }

    public static class HistoryParsers
    {
        public static readonly Parser<string> HardRule =
            from text in Parse.Char('-').Many().Text().Token()
            select text;

        public static readonly Parser<char> UntilEndOfLine = Parse.CharExcept(c => c == '\r' || c == '\n', "start of line ending");
        public static readonly Parser<IEnumerable<char>> WhiteSpace = Parse.WhiteSpace.Many().Or(Parse.String("&#160;"));
        public static readonly string[] EmailHeaderFields = new[] { "date", "from", "to", "send to", "cc", "subject", "sent" };
        
        public static readonly Parser<EmailHeaderItem> EmailHeaderItem =
            from title in Parse.CharExcept(':').Many().Text().Token()
            where EmailHeaderFields.Any(h => h.Equals(title, StringComparison.InvariantCultureIgnoreCase))
            from _1 in Parse.Char(':')
            from text in UntilEndOfLine.Many().Token().Text()
            from rest in HardRule.Or(WhiteSpace)
            select new EmailHeaderItem { Title = title, Text = text };

        public static readonly Parser<string> BlockQuoteLine =
            from leader in Parse.String("&gt;").AtLeastOnce()
            from text in UntilEndOfLine.Many().Token().Text()
            select text;

        public static readonly Parser<EmailHeader> EmailHeader =
            from items in EmailHeaderItem.Many()
            select new EmailHeader { Headers = items };

        public static readonly Parser<BlockQuote> BlockQuote =
            from lines in BlockQuoteLine.Many()
            select new BlockQuote {Lines = lines};
        
        public static readonly Parser<Content> Content =
            from _1 in WhiteSpace
            from text in UntilEndOfLine.Many().Text().Token()
            select new Content {Text = text.TrimEnd()};

        public static readonly Parser<string> OriginalMessageHeader =
            from _1 in WhiteSpace
            from o in Parse.Char('O')
            from n in Parse.Char('n')
            from middle in Parse.CharExcept('w').Many().Text()
            from footer in Parse.String("wrote:").Token()
            select "On " + middle + "wrote:";

        public static readonly Parser<OriginalMessage> OriginalMessage =
            from header in OriginalMessageHeader
            from items in Content.Many().End()
            select new OriginalMessage {Header = header, Items = items};

        public static readonly Parser<IItem> Item =
            from items in Parse.Ref(() => OriginalMessage).Select(n => (IItem) n)
                .Or(EmailHeader)
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

    //TODO pull our renderers into a their own types
    public class HistoryItemHtmlRenderer
    {
        private readonly StringBuilder _output;

        public HistoryItemHtmlRenderer()
        {
            _output = new StringBuilder();
        }

        private static long _idIndex;
        public string Render(IEnumerable<IItem> items)
        {
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

                if (item.GetType().CanBeCastTo<OriginalMessage>())
                {
                    renderOriginalMessage(item as OriginalMessage);
                    continue;
                }

                renderItem(item);
            }

            return _output.ToString();
        }

        private void renderEmailHeader(EmailHeader emailHeader)
        {
            _idIndex += 1;
            var id = "emailHeader" + _idIndex;

            _output.AppendLine(@"<div id=""{0}"" class=""history-email-header collapse in""><i class=""icon-envelope"" title=""Click to expand"" data-toggle=""collapse"" data-target=""#{0}""></i>".ToFormat(id));

            var emailHeaderItem = emailHeader.Headers.First();
            _output.AppendLine(@"<h5 class=""history-inline-header"">{0} : {1}</h5><div class=""history-inline-content""><ul>".ToFormat(emailHeaderItem.Title, emailHeaderItem.Text));

            foreach (var header in emailHeader.Headers.Skip(1))
            {
                _output.AppendLine(@"<li><span class=""email-header-name"">{0}</span> <span class=""email-header-text"">{1}</span></li>".ToFormat(header.Title.ToLower().Capitalize(), header.Text));
            }

            _output.Append(@"</ul></div></div>");
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

        private void renderOriginalMessage(OriginalMessage message)
        {
            _idIndex += 1;
            var id = "originalMessage" + _idIndex;
            _output.AppendLine(@"<div id=""{0}"" class=""history-inline-message collapse in ""><i class=""icon-comment"" title=""Click to expand"" data-toggle=""collapse"" data-target=""#{0}""></i>".ToFormat(id));

            _output.AppendLine(@"<h5 class=""history-inline-header"">{0}</h5><div class=""history-inline-content"">".ToFormat(message.Header));

            Render(message.Items);

            _output.AppendLine(@"</div></div>");
        }


        private void  renderItem(IItem item)
        {
            _output.AppendLine("<p>{0}</p>".ToFormat(item));
        }
    }
}