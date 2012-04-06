using System.Collections.Generic;
using Sprache;

namespace Dovetail.SDK.Bootstrap.History.Parser
{
    public class HistoryItemParser
    {
        public IEnumerable<IItem> Parse(string input)
        {
            return HistoryParsers.Item.Many().End().Parse(input);
        }
    }
}