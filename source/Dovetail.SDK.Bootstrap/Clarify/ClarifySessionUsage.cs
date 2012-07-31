using System;
using System.Collections.Generic;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface IClarifySessionUsage
	{
		IEnumerable<ClarifySessionUser> Sessions { get; set; }
		IEnumerable<ClarifySessionUser> InvalidSessions { get; set; }
	}

	public class ClarifySessionUsage : IClarifySessionUsage
	{
		public ClarifySessionUsage(IEnumerable<ClarifySessionUser> validSessions, IEnumerable<ClarifySessionUser> inValidSessions)
		{
			Sessions = validSessions;
			InvalidSessions = inValidSessions;
		}

		public IEnumerable<ClarifySessionUser> Sessions { get; set; }
		public IEnumerable<ClarifySessionUser> InvalidSessions { get; set; }
	}

	public class ClarifySessionUser
	{
		public string Username { get; set; }
		public Guid SessionId { get; set; }
	}
}