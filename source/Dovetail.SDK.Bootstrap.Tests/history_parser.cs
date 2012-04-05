using System.Linq;
using Dovetail.SDK.Bootstrap.History;
using NUnit.Framework;
using Sprache;

namespace Dovetail.SDK.Bootstrap.Tests
{
    [TestFixture]
    public class history_parser_document
    {
        [Test]
        public void finds_all_items()
        {
            const string input = "item1\r\nitem2\nitem3";

            var document = HistoryParser.Document.Parse(input);

            var items = document.Items.ToArray();
            items[0].ToString().ShouldEqual("item1");
            items[1].ToString().ShouldEqual("item2");
            items[2].ToString().ShouldEqual("item3");
        }

        [Test]
        public void finds_email_headers_mixed_with_items()
        {
            const string input = "item1\r\nto: kmiller@dovetailsoftware.com\nitem3";

            var document = HistoryParser.Document.Parse(input);

            var items = document.Items.ToArray();
            items[0].ToString().ShouldEqual("item1");

            var emailHeaderItem = ((EmailHeader) items[1]).Headers.First();
            emailHeaderItem.ShouldEqual("to");
            emailHeaderItem.Text.ShouldEqual("kmiller@dovetailsoftware.com");

            items[2].ToString().ShouldEqual("item3");
        }
    }

    [TestFixture]
    public class history_parser_items
    {
        [Test]
        public void detect_item()
        {
            const string input = "line1\r\nline2";

            var item = HistoryParser.Item.Parse(input);

            item.ToString().ShouldEqual("line1");
        }

        [Test]
        public void item_white_space_is_removed()
        {
            const string input = "   line1\r\n   line2";

            var item = HistoryParser.Item.Parse(input);

            item.ToString().ShouldEqual("line1");
        }

        [Test]
        public void email_header_items_are_detected()
        {
            const string input = "from: yadda\nto:email@example.com\nsubject:math";

            var item = HistoryParser.Item.Parse(input);

            var emailHeader = (item as EmailHeader);
            emailHeader.ShouldNotBeNull();
            emailHeader.Headers.Count().ShouldEqual(3);
        }

        [Test]
        public void email_header_properties_are_populated()
        {
            const string input = "from: yadda\r\nan item";

            var item = HistoryParser.Item.Parse(input);

            var emailHeaderItem = ((EmailHeader)item).Headers.First();
            emailHeaderItem.Title.ShouldEqual("from");
            emailHeaderItem.Text.ShouldEqual("yadda");
        }

        [Test]
        public void email_header_titles_ignores_case()
        {
            const string input = "FROM: other content\r\nan item";

            var item = HistoryParser.Item.Parse(input);

            var emailHeaderItem = ((EmailHeader)item).Headers.First();
            emailHeaderItem.Title.ShouldEqual("FROM");
            emailHeaderItem.Text.ShouldEqual("other content");
        }

        [Test]
        public void item_similar_to_an_email_header_is_still_just_an_item()
        {
            const string input = "notaheader: yadda\r\nan item";

            var item = HistoryParser.Item.Parse(input);

            (item as EmailHeader).ShouldBeNull();
        }

    }
}