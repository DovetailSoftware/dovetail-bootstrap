using System.Linq;
using Dovetail.SDK.Bootstrap.History.Parser;
using NUnit.Framework;

namespace Dovetail.SDK.Bootstrap.Tests
{
    [TestFixture]
    public class history_item_parser
    {
        [Test]
        public void finds_all_items()
        {
            const string input = "item1\r\nitem2\nitem3";

            var items = new HistoryItemParser().Parse(input).ToArray();

            items[0].ToString().ShouldEqual("item1");
            items[1].ToString().ShouldEqual("item2");
            items[2].ToString().ShouldEqual("item3");
        }

        [Test]
        public void finds_email_headers_mixed_with_items()
        {
            const string input = "item1\r\nto: kmiller@dovetailsoftware.com\nitem3";

            var items = new HistoryItemParser().Parse(input).ToArray();
            items[0].ToString().ShouldEqual("item1");

            var emailHeaderItem = ((EmailHeader) items[1]).Headers.First();
            emailHeaderItem.Title.ShouldEqual("to");
            emailHeaderItem.Text.ShouldEqual("kmiller@dovetailsoftware.com");

            items[2].ToString().ShouldEqual("item3");
        }
    }
}