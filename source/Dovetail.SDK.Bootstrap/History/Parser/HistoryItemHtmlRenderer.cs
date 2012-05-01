using System.Collections.Generic;
using System.Linq;
using System.Text;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History.Parser
{
    //TODO pull our renderers into a their own types
    public class HistoryItemHtmlRenderer
    {
        private readonly StringBuilder _output;

        public HistoryItemHtmlRenderer()
        {
            _output = new StringBuilder();
        }

        private static long _idIndex;
        public string Render(IEnumerable<IItem> items)
        {
            foreach (var item in items)
            {
                if(item.GetType().CanBeCastTo<EmailHeader>())
                {
                    renderEmailHeader(item as EmailHeader);
                    continue;
                }

                if (item.GetType().CanBeCastTo<BlockQuote>())
                {
                    renderBlockQuote(item as BlockQuote);
                    continue;
                }

                if (item.GetType().CanBeCastTo<OriginalMessage>())
                {
                    renderOriginalMessage(item as OriginalMessage);
                    continue;
                }

                renderItem(item);
            }

            return _output.ToString();
        }

        private void renderEmailHeader(EmailHeader emailHeader)
        {
            _idIndex += 1;
            var id = "emailHeader" + _idIndex;
			var shouldCollapse = emailHeader.Headers.Count() > 1;
            _output
				//.AppendLine(@"<div id=""{0}"" class=""history-email-header collapse in""><i class=""icon-envelope"" title=""Click to expand"" data-toggle=""collapse"" data-target=""#{0}""></i>"
				.AppendLine(@"<div id=""{0}"" class=""history-email-header {1}""><i class=""icon-envelope"" {2} data-target=""#{0}""></i>"
				.ToFormat(id, shouldCollapse ? "collapse in" : "", shouldCollapse ? @"data-toggle=""collapse"" title=""Click to expand""" : ""));

            var emailHeaderItem = emailHeader.Headers.First();
            _output
				.AppendLine(@"<h5 class=""history-inline-header"">{0} : {1}</h5>"
				.ToFormat(emailHeaderItem.Title.ToLower().Capitalize(), emailHeaderItem.Text));

        	var rest = emailHeader.Headers.Skip(1).ToArray();
			if (rest.Any())
			{
				_output.AppendLine(@"<div class=""history-inline-content""><ul>");
				foreach (var header in rest)
				{
					_output.AppendLine(@"<li><span class=""email-header-name"">{0}</span> <span class=""email-header-text"">{1}</span></li>".ToFormat(header.Title.ToLower().Capitalize(), header.Text));
				}
				_output.Append(@"</ul></div>");
			}
        	
        	_output.AppendLine("</div>");
        }

        private void renderBlockQuote(BlockQuote blockQuote)
        {
            _output.AppendLine(@"<blockquote>");

            foreach (var line in blockQuote.Lines)
            {
                _output.AppendLine(line + "<br/>");
            }

            _output.AppendLine(@"</blockquote>");
        }

        private void renderOriginalMessage(OriginalMessage message)
        {
            _idIndex += 1;
            var id = "originalMessage" + _idIndex;
            _output.AppendLine(@"<div id=""{0}"" class=""history-inline-message collapse in ""><i class=""icon-comment"" title=""Click to expand"" data-toggle=""collapse"" data-target=""#{0}""></i>".ToFormat(id));

            _output.AppendLine(@"<h5 class=""history-inline-header"">{0}</h5><div class=""history-inline-content"">".ToFormat(message.Header));

            Render(message.Items);

            _output.AppendLine(@"</div></div>");
        }


        private void  renderItem(IItem item)
        {
            _output.AppendLine("<p>{0}</p>".ToFormat(item));
        }
    }
}