using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dovetail.SDK.Bootstrap.History.Configuration;
using FubuCore;
using Sprache;

namespace Dovetail.SDK.Bootstrap.History.Parser
{
	public interface IItem { }

	public interface IHasNestedItems
	{
		IEnumerable<IItem> Items { get; set; }
	}

	public interface IRenderHtml
	{
		string RenderHtml();
	}

	public class Line : IItem, IRenderHtml
	{
		public string Text { get; set; }

		public override string ToString()
		{
			return Text;
		}

		public string RenderHtml()
		{
			return Text + "<br/>";
		}
	}

	public class ParagraphEnd : IItem { }

	public class EmailHeaderItem
	{
		public string Title { get; set; }
		public string Text { get; set; }
	}

	public class EmailLog : IItem, IHasNestedItems
	{
		public EmailHeader Header { get; set; }
		public IEnumerable<IItem> Items { get; set; }

		public override string ToString()
		{
			var builder = new StringBuilder("Email Log");
			builder.AppendLine(Header.ToString());
			Items.Each(item => builder.AppendLine(item.ToString()));
			return builder.ToString();
		}
	}

	public class EmailHeader : IItem
	{
		public bool IsLogHeader { get; set; }
		public IEnumerable<EmailHeaderItem> Headers { get; set; }

		public override string ToString()
		{
			var builder = new StringBuilder("Email Header\n");
			Headers.Each(h => builder.AppendFormat("{0}: {1}\r\n", h.Title, h.Text));
			return builder.ToString();
		}
	}

	public class BlockQuote : IItem
	{
		public IEnumerable<string> Lines { get; set; }

		public override string ToString()
		{
			var builder = new StringBuilder();
			Lines.Each(l => builder.AppendFormat("> {0}\r\n", l));
			return builder.ToString();
		}
	}

	public class OriginalMessage : IItem, IHasNestedItems
	{
		public string Header { get; set; }
		public IEnumerable<IItem> Items { get; set; }

		public override string ToString()
		{
			var builder = new StringBuilder("Original Message\n");
			builder.AppendLine(Header);
			Items.Each(item => builder.AppendLine(item.ToString()));
			return builder.ToString();
		}
	}

	/// <summary>
	/// Parsers for slicing apart case history items and projecting them into IItems. I recommend taking a look at this post for details on the Sprache parser.
	/// Sprache Parser introduction - http://nblumhardt.com/2010/01/building-an-external-dsl-in-c/
	/// Monadic Parsers in c# 3.0 - http://blogs.msdn.com/b/lukeh/archive/2007/08/19/monadic-parser-combinators-using-c-3-0.aspx
	/// Parser Combinators - http://msdn.microsoft.com/en-us/magazine/hh580742.aspx
	/// </summary>
	public class HistoryParsers
	{
		private readonly HistorySettings _settings;
		private readonly HistoryOriginalMessageConfiguration _originalMessageConfiguration;

		public HistoryParsers(HistorySettings settings, HistoryOriginalMessageConfiguration originalMessageConfiguration)
		{
			_settings = settings;
			_originalMessageConfiguration = originalMessageConfiguration;
		}

		public const string BEGIN_EMAIL_LOG_HEADER = "__BEGIN EMAIL_HEADER__";
		public const string END_EMAIL_LOG_HEADER = "__END EMAIL_HEADER__";
		public const string BEGIN_ISODATE_HEADER = "__BEGIN_ISODATE_HEADER__";

		public static readonly Parser<string> HardRule =
			from text in Parse.Char('-').Many().Text().Token()
			select text;

		public static readonly Parser<char> UntilEndOfLine = Parse.CharExcept(c => c == '\r' || c == '\n', "start of line ending");
		public static readonly Parser<IEnumerable<char>> WhiteSpace = Parse.WhiteSpace.Many().Or(Parse.String("&#160;"));

		public static readonly Parser<string> BlockQuoteLine =
			from leader in Parse.String("&gt;").AtLeastOnce()
			from text in UntilEndOfLine.Many().Token().Text()
			select text;

		public static readonly Parser<BlockQuote> BlockQuote =
			from lines in BlockQuoteLine.Many()
			select new BlockQuote {Lines = lines};

		public Parser<IItem> Line
		{
			get
			{
				return from _1 in WhiteSpace
					from text in UntilEndOfLine.Many().Text().Token()
					select new Line {Text = text.TrimEnd()};
			}
		}

		public Parser<IItem> ParagraphEnd
		{
			get
			{
				return 
					from _1 in WhiteSpace
					from eop in Parse.String(ParagraphEndLocator.ENDOFPARAGRAPHTOKEN).Token()
					from _2 in WhiteSpace
					select new ParagraphEnd();
			}
		}
		
		public Parser<string> OriginalMessageHeader
		{
			get
			{
				return from text in UntilEndOfLine.Many().Token().Text()
					   where _originalMessageConfiguration.Expressions.Any(h => h.IsMatch(text))
				select text;
			}
		}

		public Parser<OriginalMessage> OriginalMessage
		{
			get
			{
				return from header in OriginalMessageHeader
					from items in EmailItem.Many()
					select new OriginalMessage {Header = header, Items = items};
			}
		}

		public Parser<EmailHeaderItem> EmailHeaderItem
		{
			get
			{
				return from _1 in WhiteSpace
					from title in Parse.CharExcept(':').Many().Text().Token()
					where _settings.GetLogEmailHeaderTokens().Any(h =>
					{
						var key = h.ToString();
						var isMatch = key.Equals(title, StringComparison.CurrentCultureIgnoreCase);
						return isMatch;
					})
					from _2 in Parse.Char(':')
					from text in IsoDate.Or(UntilEndOfLine.Many().Token().Text())
					from rest in HardRule.Or(WhiteSpace)
					select new EmailHeaderItem {Title = title, Text = text};
			}
		}

		public Parser<EmailHeader> LogEmailHeader
		{
			get
			{
				return from _start in Parse.String(BEGIN_EMAIL_LOG_HEADER)
					from items in EmailHeaderItem.Many()
					from _end in Parse.String(END_EMAIL_LOG_HEADER)
					select new EmailHeader {Headers = items, IsLogHeader = true};
			}
		}

		public Parser<string> IsoDate
		{
			get
			{
				return
					from _start in Parse.String(BEGIN_ISODATE_HEADER).Token()
					from date in UntilEndOfLine.Many().Token().Text()
					select @"<span class=""time-format"">{0}</span>".ToFormat(date);
			}
		}

		public Parser<EmailHeader> EmailHeader
		{
			get
			{
				return from items in EmailHeaderItem.Many()
					select new EmailHeader {Headers = items};
			}
		}

		public Parser<IItem> ContentItem
		{
			get { return from items in ParagraphEnd.Or(Line) select items; }
		}

		public Parser<IItem> EmailItem
		{
			get
			{
				return from items in Parse.Ref(() => OriginalMessage).Select(n => (IItem) n)
					.Or(EmailHeader)
					.Or(BlockQuote)
					.Or(ContentItem)
					select items;
			}
		}

		public Parser<EmailLog> LogEmail
		{
			get
			{
				return from header in LogEmailHeader
					from items in EmailItem.Many()
					select new EmailLog {Header = header, Items = items};
			}
		}
	}
}