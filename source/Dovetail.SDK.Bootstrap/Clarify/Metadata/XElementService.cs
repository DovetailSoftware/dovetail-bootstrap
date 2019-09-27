using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public class XElementService : IXElementService
	{
		private readonly IEnumerable<IXElementVisitor> _visitors;

		public XElementService(IEnumerable<IXElementVisitor> visitors)
		{
			_visitors = visitors;
		}

		public void Visit(XElement element, ParsingContext context)
		{
			context.PushElement(element);

			_visitors
				.Where(visitor => visitor.Matches(element, context))
				.Each(visitor =>
				{
					visitor.Visit(element, context);
					element
						.Elements()
						.Each(child => Visit(child, context));

					visitor.ChildrenBound(context);
				});

			context.PopElement();
		}
	}
}