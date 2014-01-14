using Dovetail.SDK.Bootstrap.History.Parser;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.Bootstrap.Tests
{
	[TestFixture]
	public class paragraph_end_locator
	{
		private ParagraphEndLocator _cut;

		[SetUp]
		public void beforeEach()
		{
			_cut = new ParagraphEndLocator();
		}

		[Test]
		public void should_find_ends_with_carrage_returns()
		{
			const string input = "P1L1\nP1L2\n\nP2L1\nP2L2";

			var result = _cut.LocateAndReplace(input);

			result.ShouldEqual("P1L1\nP1L2\n{0}\nP2L1\nP2L2".ToFormat(ParagraphEndLocator.ENDOFPARAGRAPHTOKEN));
		}

		[Test]
		public void should_find_ends_with_carrage_return_and_line_feeds()
		{
			const string input = "P1L1\r\nP1L2\r\n\r\nP2L1\r\nP2L2";

			var result = _cut.LocateAndReplace(input);

			result.ShouldEqual("P1L1\r\nP1L2\n{0}\nP2L1\r\nP2L2".ToFormat(ParagraphEndLocator.ENDOFPARAGRAPHTOKEN));
		}
	}
}