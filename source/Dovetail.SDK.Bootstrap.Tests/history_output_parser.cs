using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;
using NUnit.Framework;
using Rhino.Mocks;

namespace Dovetail.SDK.Bootstrap.Tests
{
	[TestFixture]
	public class history_output_parser
	{
		private ILogger _logger;
		private HistoryOutputParser _cut;

		[SetUp]
		public void beforeEach()
		{
			_logger = MockRepository.GenerateStub<ILogger>();
			var historyItemParser = new HistoryItemParser(new HistoryParsers(new HistorySettings(), new HistoryOriginalMessageConfiguration(_logger)), _logger);
			_cut = new HistoryOutputParser(new ParagraphEndLocator(), new ParagraphAggregator(), historyItemParser, new HistoryItemHtmlRenderer(), new HtmlEncodeOutputEncoder(), new UrlLinkifier());
		}

//		[Test]
//		public void should_find_paragraph_ends()
//		{
//			var emailHeader = "{0}From: kdog@albatross.net{1}".ToFormat(HistoryParsers.BEGIN_EMAIL_LOG_HEADER, HistoryParsers.END_EMAIL_LOG_HEADER); 

//			const string input =  @"Thanks, 
//
//Bye
//
//kevin Miller
//Foo@coo.com
//
//On Tue, Nov 3, 2009 at 12:34 PM, Sam Tyson <dude@gmail.com> wrote:
//
//Here are the config files. Please let me know what else I can get you.
//
//From: Dude Wee [mailto:dude@gmail.com]
//Sent: Tuesday, November 03, 2009 12:12 PM
//To: A Guy
//Subject: Re: test
//
//test received";

//			var output = _cut.EncodeEmailLog(emailHeader + "\n" + input);

//			Console.WriteLine(output);
//		}

//		[Test]
//		public void should_find_paragraph_ends()
//		{
//			const string input = @"Thanks, 
//
//Bye
//
//kevin Miller
//Foo@coo.com
//
//On Tue, Nov 3, 2009 at 12:34 PM, Sam Tyson <dude@gmail.com> wrote:
//
//Here are the config files. Please let me know what else I can get you.
//
//From: Dude Wee [mailto:dude@gmail.com]
//Sent: Tuesday, November 03, 2009 12:12 PM
//To: A Guy
//Subject: Re: test
//
//test received";

//			var output = _cut.EncodeEmailLog(input);

//			Console.WriteLine(output);
//		}


//		[Test]
//		public void aggregate_item_line_and_paragraphs()
//		{
//			var items = new IItem[]
//			{
//				new OriginalMessage {Header = "orig1", Items = new IItem[0]}, 
//				new Line {Text = "p1l1"}, 
//				new Line {Text = "p1l2"}, 
//				new ParagraphEnd(),
//				new Line {Text = "p2l1"}, 
//				new ParagraphEnd(),
//				new OriginalMessage {Header = "orig2", Items = new IItem[0]}, 
//			};
//
//			var output = new List<IItem>();
//			var content = new List<Line>();
//
//			items.Each(i =>
//			{
//				var itemType = i.GetType();
//				if (itemType.CanBeCastTo<Line>())
//				{
//					content.Add(i as Line);
//					return;
//				}
//
//				if (itemType.CanBeCastTo<ParagraphEnd>())
//				{
//					output.Add(new Paragraph {Lines = content.ToArray()});
//					content.Clear();
//					return;
//				}
//
//				output.Add(i);
//			});
//
//			output.AddRange(content);
//
//			output.ToArray().WriteToConsole();
//
//		}

	}
}