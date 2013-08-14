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
        public IEnumerable<IItem> Parse(string input)
        {
            return HistoryParsers.Item.Many().End().Parse(input);
        }
    }
}