using System.Collections.Generic;
using Sprache;

namespace Dovetail.SDK.Bootstrap.History.Parser
{
	public interface IHistoryItemParser
	{
		EmailLog ParseEmailLog(string input);
		IEnumerable<Content> ParseContent(string input);
	}

	public class HistoryItemParser : IHistoryItemParser
	{
		private readonly HistoryParsers _historyParser;

		public HistoryItemParser(HistoryParsers historyParser)
		{
			_historyParser = historyParser;
		}
		
		public IEnumerable<Content> ParseContent(string input)
		{
			return _historyParser.Content.Many().End().Parse(input);
		}

		public EmailLog ParseEmailLog(string input)
		{
			return _historyParser.LogEmail.Parse(input);
		}
	}
}