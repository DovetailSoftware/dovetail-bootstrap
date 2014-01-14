using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;
using NUnit.Framework;
using FubuCore;
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
			var historyItemParser = new HistoryItemParser(new HistoryParsers(new HistorySettings()), _logger);
			_cut = new HistoryOutputParser(new ParagraphEndLocator(), new ParagraphAggregator(), historyItemParser, new HistoryItemHtmlRenderer(), new HtmlEncodeOutputEncoder(), new UrlLinkifier());
		}
//
//		[Test]
//		public void should_find_paragraph_ends()
//		{
//			const string input = "para1line1\npara1line2\n   \npara2line1\npara2line2";
////			const string input = "line1\nline2\n        \n\n\n\nline3";
//			var output = _cut.Encode(input);
//
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