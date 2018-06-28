using System.Collections.Generic;

namespace Dovetail.SDK.History
{
	public interface IEmailParser
	{
		IDictionary<string, object> Parse(string input);
	}
}