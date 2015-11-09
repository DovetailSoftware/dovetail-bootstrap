using System.Diagnostics;
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

		[Test]
		public void matches_known_urls()
		{
			matches("http://www.RegExr.com");
			matches("http://RegExr.com");
			matches("http://www.RegExr.com?2rjl6");
			matches("http://www.RegExr.com?2rjl6&x=99&498");
			matches("http://www.RegExr.com?2rjl6(v85)&x=99&498");
			matches("http://RegExr.com#foo");
			matches("http://RegExr.com#foo(bar)");
			matches("This is my website (http://www.microsoft.com)");
			matches("This is my website [http://www.microsoft.com]");
			matches("http://foo.com/blah_blah");
			matches("http://foo.com/blah_blah/");
			matches("(Something like http://foo.com/blah_blah)");
			matches("http://foo.com/blah_blah_(wikipedia)");
			matches("http://foo.com/more_(than)_one_(parens)");
			matches("(Something like http://foo.com/blah_blah_(wikipedia))");
			matches("http://foo.com/blah_(wikipedia)#cite-1");
			matches("http://foo.com/blah_(wikipedia)_blah#cite-1");
			matches("http://foo.com/unicode_(✪)_in_parens");
			matches("http://foo.com/(something)?after=parens");
			matches("http://foo.com/blah_blah.");
			matches("http://foo.com/blah_blah/.");
			matches("<http://foo.com/blah_blah>");
			matches("<http://foo.com/blah_blah/>");
			matches("http://foo.com/blah_blah,");
			matches("http://www.extinguishedscholar.com/wpglob/?p=364.");
			matches("http://✪df.ws/1234");
			matches("rdar://1234");
			matches("rdar:/1234");
			matches("x-yojimbo-item://6303E4C1-6A6E-45A6-AB9D-3A908F59AE0E");
			matches("message://%3c330e7f840905021726r6a4ba78dkf1fd71420c1bf6ff@mail.gmail.com%3e");
			matches("http://➡.ws/䨹");
			matches("www.c.ws/䨹");
			matches("<tag>http://example.com</tag>");
			matches("Just a www.example.com link.");
			matches("http://example.com/something?with,commas,in,url, but not at end");
			matches("What about <mailto:gruber@daringfireball.net?subject=TEST> (including brokets).");
			matches("mailto:name@example.com");
			matches("bit.ly/foo");
			matches("“is.gd/foo/”");
			matches("WWW.EXAMPLE.COM");
			matches("http://www.asianewsphoto.com/(S(neugxif4twuizg551ywh3f55))/Web_ENG/View_DetailPhoto.aspx?PicId=752");
			matches("http://www.asianewsphoto.com/(S(neugxif4twuizg551ywh3f55))");
			matches("http://lcweb2.loc.gov/cgi-bin/query/h?pp/horyd:@field(NUMBER+@band(thc+5a46634))");
			matches("https://some.logstash-url.com/#/doc/logstash-*/logstash-2015.11.09/eventlog?id=AVDthjgPPwD8moz0X0gd&_g=()");
		}

		private static void matches(string input)
		{
			var match = UrlLinkifier.UrlExpression.IsMatch(input);
			if (!match) Debug.WriteLine("Match failed for: " + input);

			match.ShouldBeTrue();
		}
	}
}
