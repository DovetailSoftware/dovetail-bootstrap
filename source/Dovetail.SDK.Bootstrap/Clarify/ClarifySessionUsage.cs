using System;
using System.Collections.Generic;
using System.Linq;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface IClarifySessionUsage
	{
		int TotalSessions { get; }
		int ValidSessions { get; }
	}

	public class ClarifySessionUsage : IClarifySessionUsage
	{
		public ClarifySessionUsage(IEnumerable<ClarifySessionUser> validSessions, IEnumerable<ClarifySessionUser> inValidSessions)
		{
			Sessions = validSessions;
			InvalidSessions = inValidSessions;
		}

		public int TotalSessions { get { return Sessions.Count() + InvalidSessions.Count(); } }
		public int ValidSessions { get { return Sessions.Count(); } }
		public IEnumerable<ClarifySessionUser> Sessions { get; set; }
		public IEnumerable<ClarifySessionUser> InvalidSessions { get; set; }
	}

	public class ClarifySessionUser
	{
		public string Username { get; set; }
		public Guid SessionId { get; set; }
	}
}