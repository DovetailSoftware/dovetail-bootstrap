using System.Collections.Generic;
using System.Linq;
using System.Text;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History.Parser
{
	public interface IHistoryItemHtmlRenderer
	{
		string Render(IEnumerable<IItem> items);
	}

	public class HistoryItemHtmlRenderer : IHistoryItemHtmlRenderer
	{
		private static long _idIndex;

		public string Render(IEnumerable<IItem> items)
		{
			var output = new StringBuilder();

			foreach (var item in items)
			{
				if (item.GetType().CanBeCastTo<EmailHeader>())
				{
					renderEmailHeader(item as EmailHeader, output);
					continue;
				}

				if (item.GetType().CanBeCastTo<BlockQuote>())
				{
					renderBlockQuote(item as BlockQuote, output);
					continue;
				}

				if (item.GetType().CanBeCastTo<OriginalMessage>())
				{
					renderOriginalMessage(item as OriginalMessage, output);
					continue;
				}

				renderItem(item, output);
			}

			return output.ToString();
		}

		private static void renderEmailHeader(EmailHeader emailHeader, StringBuilder output)
		{
			_idIndex += 1;
			var id = "emailHeader" + _idIndex;
			output.AppendLine(@"<div id=""{0}"" class=""history-email-header"">".ToFormat(id));

			if (emailHeader.Headers.Any())
			{
				output.AppendLine(@"<div class=""history-inline-content""><ul>");
				foreach (var header in emailHeader.Headers)
				{
					output.AppendLine(@"<li><span class=""email-header-name"">{0}</span> <span class=""email-header-text"">{1}</span></li>".ToFormat(header.Title.ToLower().Capitalize(), header.Text));
				}
				output.Append(@"</ul></div>");
			}

			output.AppendLine("</div>");
		}

		private static void renderBlockQuote(BlockQuote blockQuote, StringBuilder output)
		{
			output.AppendLine(@"<blockquote>");

			foreach (var line in blockQuote.Lines)
			{
				output.AppendLine(line + "<br/>");
			}

			output.AppendLine(@"</blockquote>");
		}

		private void renderOriginalMessage(OriginalMessage message, StringBuilder output)
		{
			_idIndex += 1;
			var id = "originalMessage" + _idIndex;
			output.AppendLine(@"<div id=""{0}"" class=""history-inline-message""></i>".ToFormat(id));

			output.AppendLine(@"<h5 class=""history-inline-header"">{0}</h5><div class=""history-inline-content"">".ToFormat(message.Header));

			output.Append(Render(message.Items));

			output.AppendLine(@"</div></div>");
		}

		private static void renderItem(IItem item, StringBuilder output)
		{
			output.AppendLine("<p>{0}</p>".ToFormat(item));
		}
	}
}