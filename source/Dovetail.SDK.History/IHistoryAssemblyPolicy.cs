namespace Dovetail.SDK.History
{
	public interface IHistoryAssemblyPolicy
	{
		bool Matches(HistoryRequest request);
		HistoryResult HistoryFor(HistoryRequest request, IHistoryBuilder builder);
	}
}