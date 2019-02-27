namespace Dovetail.SDK.Bootstrap.Http
{
	public interface IHttpIntervalService
	{
		int Interval { get; }
		void Execute();
		void CleanUp();
	}
}
