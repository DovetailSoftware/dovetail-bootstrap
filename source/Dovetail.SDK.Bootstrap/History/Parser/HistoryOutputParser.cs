using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Configuration;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History.Parser
{
	public static class ItemArrayExtension
	{
		public static void WriteToConsole(this IItem[] items)
		{
			var cnt = 0;

			items.Each(i => Console.WriteLine("{0}: {1}", ++cnt, i));
		}
	}

	public interface IHistoryOutputParser
	{
		string Encode(string input);
		string EncodeEmailLog(string input);
	}

	public class HistoryOutputParser : IHistoryOutputParser
	{
		private readonly IParagraphEndLocator _paragraphEndLocator;
		private readonly IParagraphAggregator _paragraphAggregator;
		private readonly IHistoryItemParser _itemParser;
		private readonly IHistoryItemHtmlRenderer _itemHtmlRenderer;
		private readonly HtmlEncodeOutputEncoder _encoder;
		private readonly IUrlLinkifier _linkifier;

		public HistoryOutputParser(IParagraphEndLocator paragraphEndLocator, IParagraphAggregator paragraphAggregator, IHistoryItemParser itemParser, IHistoryItemHtmlRenderer itemHtmlRenderer, HtmlEncodeOutputEncoder encoder, IUrlLinkifier linkifier)
		{
			_paragraphEndLocator = paragraphEndLocator;
			_paragraphAggregator = paragraphAggregator;
			_itemParser = itemParser;
			_itemHtmlRenderer = itemHtmlRenderer;
			_encoder = encoder;
			_linkifier = linkifier;
		}

		public string Encode(string input)
		{
			if (input.IsEmpty()) return String.Empty;

			var output = _encoder.Encode(input);

			output = _paragraphEndLocator.LocateAndReplace(output);

			var items = _itemParser.ParseContent(output).ToArray();

			items.WriteToConsole();

			var paragraphedItems = _paragraphAggregator.CollapseContentItems(items);

			var result = _itemHtmlRenderer.Render(paragraphedItems);

			return _linkifier.Linkify(result);
		}

		public string EncodeEmailLog(string input)
		{
			if (input.IsEmpty()) return String.Empty;

			var output = _encoder.Encode(input);

			output = _paragraphEndLocator.LocateAndReplace(output);

			var emailLog = _itemParser.ParseEmailLog(output);
			var items = new List<IItem> {emailLog.Header};

			var paragraphedItems = _paragraphAggregator.CollapseContentItems(items);
			items.AddRange(paragraphedItems);

			var result = _itemHtmlRenderer.Render(items);

			return _linkifier.Linkify(result);
		}
	}

	public interface IParagraphAggregator
	{
		IEnumerable<IItem> CollapseContentItems(IEnumerable<IItem> items);
	}

	public class Paragraph : IItem, IRenderHtml
	{
		public IEnumerable<Line> Lines { get; set; }

		public override string ToString()
		{
			return String.Join("\n", Lines.Select(l => l.Text));
		}

		public string RenderHtml()
		{
			return "<p>" + String.Join("<br/>", Lines.Select(l => l.Text)) + "</p>";
		}
	}

	public class ParagraphAggregator : IParagraphAggregator
	{
		public IEnumerable<IItem> CollapseContentItems(IEnumerable<IItem> items)
		{
			var output = new List<IItem>();
			var content = new List<Line>();

			items.Each(i =>
			{
				var itemType = i.GetType();
				if (itemType.CanBeCastTo<Line>())
				{
					content.Add(i as Line);
					return;
				}

				if (itemType.CanBeCastTo<ParagraphEnd>())
				{
					output.Add(new Paragraph { Lines = content.ToArray() });
					content.Clear();
					return;
				}

				output.Add(i);
			});

			output.AddRange(content);

			return output;
		}
	}
}