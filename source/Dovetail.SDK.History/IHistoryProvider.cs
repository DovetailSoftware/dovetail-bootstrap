namespace Dovetail.SDK.History
{
	public interface IHistoryProvider
	{
		HistoryResult HistoryFor(HistoryRequest request);
	}
}