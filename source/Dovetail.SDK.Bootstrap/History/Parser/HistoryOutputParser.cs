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

			//items.WriteToConsole();

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
			
			var items = _paragraphAggregator.CollapseContentItems(new IItem[] { emailLog });
			
			var result = _itemHtmlRenderer.Render(items);

			return _linkifier.Linkify(result);
		}
	}
}