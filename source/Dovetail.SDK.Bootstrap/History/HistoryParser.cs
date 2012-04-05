using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace Dovetail.SDK.Bootstrap.History
{
    public class Document
    {
        public IEnumerable<Item> Items { get; set; }
    }

    public class Item
    {
    }

    public class Content : Item
    {
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public class EmailHeader : Item
    {
        public IEnumerable<EmailHeaderItem> Headers { get; set; }
    }

    public class EmailHeaderItem
    {
        public string Title { get; set; }
        public string Text { get; set; }
    }

    public class HistoryParser
    {
        public static readonly string[] EmailHeaderFields = new[] {"from", "to", "Send to", "cc", "subject"};

        public static readonly Parser<char> UntilEndOfLine = Parse.CharExcept(c => c == '\r' || c == '\n', "start of line ending");

        public static readonly Parser<EmailHeaderItem> EmailHeaderItem =
            from title in Parse.Letter.Many().Text().Token()
            where EmailHeaderFields.Any(h => h.Equals(title, StringComparison.InvariantCultureIgnoreCase))
            from _1 in Parse.Char(':')
            from text in UntilEndOfLine.Many().Token().Text()
            from rest in Parse.WhiteSpace.Many()
            select new EmailHeaderItem {Title = title, Text = text};

        public static readonly Parser<EmailHeader> EmailHeader =
            from items in EmailHeaderItem.Many()
            select new EmailHeader {Headers = items};

        public static readonly Parser<Content> Content =
            from text in UntilEndOfLine.Many().Token().Text()
            from rest in Parse.WhiteSpace.Many()
            select new Content {Text = text};

        public static readonly Parser<Item> Item =
            EmailHeader.Select(n => (Item) n)
                .Or(Content);

        public static readonly Parser<Document> Document =
            from leading in Parse.WhiteSpace.Many()
            from items in Item.Many().End()
            select new Document {Items = items};
    }
}