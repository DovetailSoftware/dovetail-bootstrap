using System;
using Dovetail.SDK.Bootstrap.Configuration;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History.Parser
{
	public interface IHistoryOutputParser
	{
		string Encode(string input);
	}

	public class HistoryOutputParser : IHistoryOutputParser
	{
		private readonly IHistoryItemParser _itemParser;
		private readonly IHistoryItemHtmlRenderer _itemHtmlRenderer;
		private readonly HtmlEncodeOutputEncoder _encoder;
		private readonly IUrlLinkifier _linkifier;

		public HistoryOutputParser(IHistoryItemParser itemParser, IHistoryItemHtmlRenderer itemHtmlRenderer, HtmlEncodeOutputEncoder encoder, IUrlLinkifier linkifier)
		{
			_itemParser = itemParser;
			_itemHtmlRenderer = itemHtmlRenderer;
			_encoder = encoder;
			_linkifier = linkifier;
		}

		
		public string Encode(string input)
		{
			if (input.IsEmpty()) return String.Empty;

			var output = _encoder.Encode(input);

			var items = _itemParser.Parse(output);
			var result = _itemHtmlRenderer.Render(items);

			return _linkifier.Linkify(result);
		}
	}
}