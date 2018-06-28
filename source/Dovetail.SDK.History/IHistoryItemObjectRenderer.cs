using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.History.Parser;

namespace Dovetail.SDK.History
{
	public interface IHistoryItemObjectRenderer
	{
		IDictionary<string, object> Render(IItem item);
		IDictionary<string, object>[] Render(IEnumerable<IItem> items);
	}
}