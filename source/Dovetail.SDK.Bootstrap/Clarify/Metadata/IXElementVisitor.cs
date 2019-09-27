using System.Xml.Linq;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public interface IXElementVisitor
	{
		bool Matches(XElement element, ParsingContext context);
		void Visit(XElement element, ParsingContext context);

		void ChildrenBound(ParsingContext context);
	}
}
