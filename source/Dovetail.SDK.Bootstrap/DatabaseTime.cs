using System;
using Dovetail.SDK.Bootstrap.Clarify;

namespace Dovetail.SDK.Bootstrap
{
	public interface IDatabaseTime
	{
		DateTime Now { get; }
	}

	public class DatabaseTime : IDatabaseTime
	{
		private readonly Lazy<DateTime> _now;

		public DatabaseTime(IApplicationClarifySession session)
		{
			_now = new Lazy<DateTime>(() => ((ClarifySessionWrapper)session).ClarifySession.GetCurrentDate());
		}

		public DateTime Now
		{
			get { return _now.Value; }
		}
	}
}