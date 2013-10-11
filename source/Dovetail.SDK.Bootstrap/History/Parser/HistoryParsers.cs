using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Sprache;

namespace Dovetail.SDK.Bootstrap.History.Parser
{
	public interface IItem
	{
	}

	public class Content : IItem
	{
		public string Text { get; set; }

		public override string ToString()
		{
			return Text;
		}
	}

	public class EmailHeaderItem
	{
		public string Title { get; set; }
		public string Text { get; set; }
	}

	public class EmailLog
	{
		public EmailHeader Header { get; set; }
		public IEnumerable<IItem> Items { get; set; }
	}

	public class EmailHeader : IItem
	{
		public bool IsLogHeader { get; set; }
		public IEnumerable<EmailHeaderItem> Headers { get; set; }
	}

	public class BlockQuote : IItem
	{
		public IEnumerable<string> Lines { get; set; }
	}

	public class OriginalMessage : IItem
	{
		public string Header { get; set; }
		public IEnumerable<IItem> Items { get; set; }
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

		public HistoryParsers(HistorySettings settings)
		{
			_settings = settings;
		}

		public const string BEGIN_EMAIL_LOG_HEADER = "__BEGIN EMAIL_HEADER\r\n";
		public const string END_EMAIL_LOG_HEADER = "__END EMAIL_HEADER\r\n";

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

		public Parser<Content> Content
		{
			get
			{
				return from _1 in WhiteSpace
					from text in UntilEndOfLine.Many().Text().Token()
					select new Content {Text = text.TrimEnd()};
			}
		}
		
		public Parser<string> OriginalMessageHeader
		{
			get
			{
				return from text in UntilEndOfLine.Many().Token().Text()
					   where _settings.OriginalMessageDetectionExpressions.Any(h => h.IsMatch(text))
				select text;
			}
		}

		public Parser<OriginalMessage> OriginalMessage
		{
			get
			{
				return from header in OriginalMessageHeader
					from items in Item().Many()
					select new OriginalMessage {Header = header, Items = items};
			}
		}

		public Parser<EmailHeaderItem> EmailHeaderItem
		{
			get
			{
				return from title in Parse.CharExcept(':').Many().Text().Token()
					where _settings.LogEmailHeaders.Any(h => h.Equals(title, StringComparison.InvariantCultureIgnoreCase))
					from _1 in Parse.Char(':')
					from text in UntilEndOfLine.Many().Token().Text()
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

		public Parser<EmailHeader> EmailHeader
		{
			get
			{
				return from items in EmailHeaderItem.Many()
					select new EmailHeader {Headers = items};
			}
		}

		public Parser<EmailLog> LogEmail
		{
			get
			{
				return from header in LogEmailHeader
					from items in Item().Many()
					select new EmailLog {Header = header, Items = items};
			}
		}

		public Parser<IItem> Item()
		{
			return from items in Parse.Ref(() =>OriginalMessage).Select(n => (IItem) n)
				.Or(EmailHeader)
				.Or(BlockQuote)
				.Or(Content)
				select items;
		}
	}
}