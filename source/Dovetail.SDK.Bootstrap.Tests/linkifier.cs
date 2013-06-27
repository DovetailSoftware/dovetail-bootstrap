using NUnit.Framework;

namespace Dovetail.SDK.Bootstrap.Tests
{
	[TestFixture]
	public class linkifier
	{
		private UrlLinkifier _cut;

		[SetUp]
		public void beforeEach()
		{
			_cut = new UrlLinkifier();
		}

		[Test]
		public void links_should_be_wrapped_in_anchors()
		{
			_cut.Linkify(@"I love me some http://google.com").ShouldEqual(@"I love me some <a href=""http://google.com"">http://google.com</a>");
		}

		[Test]
		public void text_without_links_should_be_untouched()
		{
			_cut.Linkify("foo").ShouldEqual(@"foo");
			_cut.Linkify("bar").ShouldEqual(@"bar");
		}
	}
}