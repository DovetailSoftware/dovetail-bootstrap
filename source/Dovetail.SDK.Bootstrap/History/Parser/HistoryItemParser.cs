using System.Collections.Generic;
using Sprache;

namespace Dovetail.SDK.Bootstrap.History.Parser
{
	public interface IHistoryItemParser
	{
		IEnumerable<IItem> Parse(string input);
	}

	public class HistoryItemParser : IHistoryItemParser
	{
		private readonly HistoryParsers _historyParser;

		public HistoryItemParser(HistoryParsers historyParser)
		{
			_historyParser = historyParser;
		}

		public IEnumerable<IItem> Parse(string input)
        {
	        return _historyParser.Item().Many().End().Parse(input);
        }
	}
}