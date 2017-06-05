using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Dovetail.SDK.ModelMap.Serialization
{
    public class XElementService : IXElementService
    {
        private readonly IEnumerable<IElementVisitor> _visitors;

        public XElementService(IEnumerable<IElementVisitor> visitors)
        {
            _visitors = visitors;
        }

        public void Visit(XElement element, ModelMap map, ParsingContext context)
        {
            context.PushElement(element);

            _visitors
                .Where(visitor => visitor.Matches(element, map, context))
                .Each(visitor =>
                {
                    visitor.Visit(element, map, context);
                    element
                        .Elements()
                        .Each(child => Visit(child, map, context));

                    visitor.ChildrenBound(map, context);
                });

            context.PopElement();
        }
    }
}