using System.Collections.Generic;

namespace Dovetail.SDK.History
{
	public interface IActEntryResolutionPolicy
	{
		bool Matches(HistoryRequest request);
		ActEntryResolution IdsFor(HistoryRequest request, int[] actCodes);
	}

	public class ActEntryResolution
	{
		public int Count { get; set; }
		public IEnumerable<int> Ids { get; set; }
	}
}