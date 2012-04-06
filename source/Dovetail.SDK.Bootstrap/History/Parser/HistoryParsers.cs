using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// Parsers for slicing apart case history items and projecting them into IItems. I recommend taking a look at this post for details on the Sprache parser.
    /// Sprache Parser introduction - http://nblumhardt.com/2010/01/building-an-external-dsl-in-c/
    /// Monadic Parsers in c# 3.0 - http://blogs.msdn.com/b/lukeh/archive/2007/08/19/monadic-parser-combinators-using-c-3-0.aspx
    /// Parser Combinators - http://msdn.microsoft.com/en-us/magazine/hh580742.aspx
    /// </summary>
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
            from o in Parse.String("On")
            from rest in Parse.AnyChar.Until(Parse.String("wrote:")).Text()
            select "On " + rest + "wrote:";

        public static readonly Parser<OriginalMessage> OriginalMessage =
            from header in OriginalMessageHeader
            from items in Parse.Ref(()=>Content).Many().End()
            select new OriginalMessage {Header = header, Items = items};

        public static readonly Parser<IItem> Item =
            from items in Parse.Ref(() => OriginalMessage).Select(n => (IItem) n)
                .Or(EmailHeader)
                .Or(BlockQuote)
                .Or(Content)
            select items;
    }
}