namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface IClarifySessionUsage
	{
		int TotalSessions { get; set; }
		int ValidSessions { get; set; }
	}

	public class ClarifySessionUsage : IClarifySessionUsage
	{
		public int TotalSessions { get; set; }
		public int ValidSessions { get; set; }
	}
}