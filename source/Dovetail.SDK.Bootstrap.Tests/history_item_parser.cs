using System.Linq;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;
using NUnit.Framework;

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
	}
}