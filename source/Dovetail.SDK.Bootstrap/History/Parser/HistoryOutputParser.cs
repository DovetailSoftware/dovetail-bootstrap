using System;
using System.Text.RegularExpressions;
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
		private readonly HistoryItemParser _itemParser;
		private readonly HistoryItemHtmlRenderer _itemHtmlRenderer;
		private readonly HtmlEncodeOutputEncoder _encoder;

		public HistoryOutputParser(HistoryItemParser itemParser, HistoryItemHtmlRenderer itemHtmlRenderer, HtmlEncodeOutputEncoder encoder)
		{
			_itemParser = itemParser;
			_itemHtmlRenderer = itemHtmlRenderer;
			_encoder = encoder;
		}

		private static readonly Regex urlFinderRegEx = new Regex(@"(?<link>(?<protocol>ftp|http|https|mailto|file|webcal):(?:(?:[A-Za-z0-9$_.+!*(),;/?:@&~=-])|%[A-Fa-f0-9]{2}){2,}(?:#(?:[a-zA-Z0-9][a-zA-Z0-9$_.+!*(),;/?:@&~=%-]*))?(?:[A-Za-z0-9$_+!*();/?:~-]))");

		public string Encode(string input)
		{
			if (input.IsEmpty()) return String.Empty;

			var output = _encoder.Encode(input);

			var items = _itemParser.Parse(output);
			var result = _itemHtmlRenderer.Render(items);
			result = urlFinderRegEx.Replace(result, @"<a href=""${link}"">${link}</a>");

			return result;
		}
	}
}