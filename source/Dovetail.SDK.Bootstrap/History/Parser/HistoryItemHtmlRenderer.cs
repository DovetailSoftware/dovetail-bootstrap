using System;
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
				if (item.GetType().CanBeCastTo<EmailLog>())
				{
					var emailLog = (EmailLog) item;
					renderEmailLog(emailLog, output);
					continue;
				}

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

		private void renderMarkdownItems(IEnumerable<IItem> items, StringBuilder output)
		{
			// This looks odd and can result in an empty trailing a5-markdown
			// element. However it band-aids an issue in where a single list or
			// code block could be split by ParagraphAggregator. This allows
			// those matching pieces to be rendered by Markdown together.
			output.Append(@"<div class=""a5-markdown"">");
			foreach (var messageItem in items)
			{
				if (messageItem.GetType().CanBeCastTo<EmailLog>() ||
					messageItem.GetType().CanBeCastTo<EmailHeader>() ||
					messageItem.GetType().CanBeCastTo<BlockQuote>() ||
					messageItem.GetType().CanBeCastTo<OriginalMessage>())
				{
					output.Append("</div>");
					output.Append(Render(new IItem[] { messageItem }));
					output.Append(@"<div class=""a5-markdown"">");
				}
				else
				{
					output.Append(Render(new IItem[] { messageItem }));
				}
			}
			output.Append("</div>");
		}

		private void renderEmailLog(EmailLog emailLog, StringBuilder output)
		{
			output.Append(@"<div class=""history-email-wrapper"">");

			renderEmailHeader(emailLog.Header, output);
			renderMarkdownItems(emailLog.Items, output);

			output.AppendLine("</div>");
		}

		private static void renderEmailHeader(EmailHeader emailHeader, StringBuilder output)
		{
			if (!emailHeader.Headers.Any()) return;

			_idIndex += 1;
			var id = "emailHeader" + _idIndex;
			output.AppendLine(@"<div id=""{0}"" class=""history-email-header"">".ToFormat(id));

			var headers = emailHeader.Headers.ToArray();
			output.AppendLine(@"<div class=""history-inline-content""><ul class=""unstyled"">");
			foreach (var header in headers)
			{
				var headerText = header.Text;
				var headerTitle = header.Title.ToLower().Capitalize();

				output.AppendLine(@"<li><span class=""email-header-name"">{0}</span> <span class=""email-header-text"">{1}</span></li>".ToFormat(headerTitle, headerText));
			}
			output.Append(@"</ul></div>");

			output.AppendLine("</div>");
		}

		private static void renderBlockQuote(BlockQuote blockQuote, StringBuilder output)
		{
			output.AppendLine(@"<blockquote>");

			foreach (var line in blockQuote.Lines)
			{
				output.AppendLine(line + "<br/>" + Environment.NewLine);
			}

			output.AppendLine(@"</blockquote>");
		}

		private void renderOriginalMessage(OriginalMessage message, StringBuilder output)
		{
			_idIndex += 1;
			var id = "originalMessage" + _idIndex;

			output.AppendLine(@"<div id=""{0}"" class=""original-message-wrapper"">".ToFormat(id));
			output.AppendLine(@"<div class=""accordion""><div class=""accordion-group"">");

			output.AppendLine(@"<div class=""accordion-heading""><a class=""accordion-toggle collapsed"" data-toggle=""collapse"" href=""#collapse{0}"">".ToFormat(id));
			output.AppendLine(@"<h5>Original Message <span class=""caret arrow-down""></span></h5></a></div>");

			output.AppendLine(@"<div id=""collapse{0}"" class=""accordion-body collapse""><div class=""accordion-inner"">".ToFormat(id));
			renderMarkdownItems(message.Items, output);
			output.AppendLine(@"</div></div></div></div></div>");
		}

		private static void renderItem(IItem item, StringBuilder output)
		{
			if (!item.GetType().CanBeCastTo<IRenderHtml>())
			{
				throw new ArgumentException("IItem {0} type has no HTML rendering mechanism".ToFormat(item.GetType()));
			}

			var htmlRenderer = (IRenderHtml) item;
			output.AppendLine(htmlRenderer.RenderHtml());
		}
	}
}
