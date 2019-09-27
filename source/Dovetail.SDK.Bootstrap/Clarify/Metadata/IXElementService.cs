using System.Xml.Linq;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public interface IXElementService
	{
		void Visit(XElement element, ParsingContext context);
	}
}
