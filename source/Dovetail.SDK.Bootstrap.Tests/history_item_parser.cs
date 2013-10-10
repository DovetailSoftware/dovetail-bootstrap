using System.Linq;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;
using NUnit.Framework;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Tests
{
	[TestFixture]
	public class history_item_parser
	{
		private HistoryItemParser _itemParser;

		[SetUp]
		public void beforeEach()
		{
			_itemParser = new HistoryItemParser(new HistoryParsers(new HistorySettings()));
		}

		[Test]
		public void finds_all_items()
		{
			const string input = "item1\r\nitem2\nitem3";

			var items = _itemParser.Parse(input).ToArray();

			items[0].ToString().ShouldEqual("item1");
			items[1].ToString().ShouldEqual("item2");
			items[2].ToString().ShouldEqual("item3");
		}

		[Test]
		public void finds_email_headers_mixed_with_items()
		{
			const string input = "item1\r\nto: kmiller@dovetailsoftware.com\nitem3";

			var items = _itemParser.Parse(input).ToArray();
			items[0].ToString().ShouldEqual("item1");

			var emailHeaderItem = ((EmailHeader) items[1]).Headers.First();
			emailHeaderItem.Title.ShouldEqual("to");
			emailHeaderItem.Text.ShouldEqual("kmiller@dovetailsoftware.com");

			items[2].ToString().ShouldEqual("item3");
		}

		[Test]
		public void finds_original_messages_mixed_with_items()
		{
			const string input = "to: kmiller@dovetailsoftware.com\nitem1\rOn some day Kevin Miller wrote:\r\noriginal item 1\r\nto: adude@needs.com\r\nfrom: kmiller@dt.com";

			var items = _itemParser.Parse(input).ToArray();
			items[0].GetType().CanBeCastTo<EmailHeader>().ShouldBeTrue();
			items[1].GetType().CanBeCastTo<Content>().ShouldBeTrue();
			items[2].GetType().CanBeCastTo<OriginalMessage>().ShouldBeTrue();

			var original = (OriginalMessage)items[2];
			original.Header.ShouldContain("Kevin Miller wrote");
			original.Items.Count().ShouldEqual(2);
		}
	}
}