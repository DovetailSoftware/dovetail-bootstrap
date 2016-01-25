using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		public const string Ticks = "```";

		public HistoryOutputParser(IParagraphEndLocator paragraphEndLocator, IParagraphAggregator paragraphAggregator, IHistoryItemParser itemParser, IHistoryItemHtmlRenderer itemHtmlRenderer, HtmlEncodeOutputEncoder encoder, IUrlLinkifier linkifier)
		{
			_paragraphEndLocator = paragraphEndLocator;
			_paragraphAggregator = paragraphAggregator;
			_itemParser = itemParser;
			_itemHtmlRenderer = itemHtmlRenderer;
			_encoder = encoder;
			_linkifier = linkifier;
		}

		// fenced code blocks need to be protected from paragraph formatting
		public List<string> FencedStrings(string input)
		{
			var arr1 = new List<string>();
			var tickPos = input.IndexOf(Ticks, StringComparison.Ordinal);

			while (tickPos >= 0)
			{
				arr1.Add(input.Substring(0, tickPos));
				input = input.Substring(tickPos);

				var closeTickPos = input.Substring(3).IndexOf(Ticks, StringComparison.Ordinal);

				if (closeTickPos >= 0)
				{
					arr1.Add(input.Substring(0, closeTickPos + 6));
					input = input.Substring(closeTickPos + 6);
					tickPos = input.IndexOf(Ticks, StringComparison.Ordinal);
				}
				else
				{
					tickPos = -1;
				}
			}

			if (input.IsNotEmpty())
			{
				arr1.Add(input);
			}

			return arr1;
		}

		public string Encode(string input)
		{
			if (input.IsEmpty()) return String.Empty;

			var arrays = FencedStrings(input);
			var outputBuilder = new StringBuilder();

			arrays.Each(output =>
			{
				if (output.IndexOf(Ticks, StringComparison.Ordinal) == 0 && output.EndsWith(Ticks) == false)
				{
					output = _paragraphEndLocator.LocateAndReplace(output);
				}

				outputBuilder.Append(output);
			});


			var items = _itemParser.ParseContent(outputBuilder.ToString()).ToArray();

			var paragraphedItems = _paragraphAggregator.CollapseContentItems(items);

			return _itemHtmlRenderer.Render(paragraphedItems);
		}

		public string EncodeEmailLog(string input)
		{
			if (input.IsEmpty()) return String.Empty;

			var arrays = FencedStrings(input);
			var outputBuilder = new StringBuilder();

			arrays.Each(output =>
			{
				// Carrier added a \t for paragraphs for the look and feel
				// but this doesn't play well with Markdown formatting
				output = output.Replace("\n\n\t", "\n");

				if (output.IndexOf(Ticks, StringComparison.Ordinal) == 0 && output.EndsWith(Ticks) == false)
				{
					output = _paragraphEndLocator.LocateAndReplace(output);
				}

				outputBuilder.Append(output);
			});

			var emailLog = _itemParser.ParseEmailLog(outputBuilder.ToString());

			var items = _paragraphAggregator.CollapseContentItems(new IItem[] { emailLog });

			return _itemHtmlRenderer.Render(items);
		}
	}
}
