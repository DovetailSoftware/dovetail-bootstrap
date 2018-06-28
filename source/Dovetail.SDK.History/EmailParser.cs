using System;
using System.Collections.Generic;
using System.Text;
using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;
using FubuCore;

namespace Dovetail.SDK.History
{
	public class EmailParser : IEmailParser
	{
		private readonly IParagraphEndLocator _paragraphEndLocator;
		private readonly IParagraphAggregator _paragraphAggregator;
		private readonly IHistoryItemParser _itemParser;
		private readonly HtmlEncodeOutputEncoder _encoder;
		private readonly IHistoryItemObjectRenderer _objectRenderer;
		public const string Ticks = "```";

		public EmailParser(IParagraphEndLocator paragraphEndLocator, IParagraphAggregator paragraphAggregator, IHistoryItemParser itemParser, HtmlEncodeOutputEncoder encoder, IHistoryItemObjectRenderer objectRenderer)
		{
			_paragraphEndLocator = paragraphEndLocator;
			_paragraphAggregator = paragraphAggregator;
			_itemParser = itemParser;
			_encoder = encoder;
			_objectRenderer = objectRenderer;
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


		public IDictionary<string, object> Parse(string input)
		{
			if (input.IsEmpty())
				return new Dictionary<string, object>();

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

				output = _encoder.Encode(output);
				outputBuilder.Append(output);
			});

			var emailLog = _itemParser.ParseEmailLogForJson(outputBuilder.ToString());

			return _objectRenderer.Render(emailLog);
		}
	}
}