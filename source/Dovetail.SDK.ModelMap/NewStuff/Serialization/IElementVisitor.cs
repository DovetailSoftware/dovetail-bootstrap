using System.Xml.Linq;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization
{
    public interface IElementVisitor
    {
        bool Matches(XElement element, ModelMap map, ParsingContext context);
        void Visit(XElement element, ModelMap map, ParsingContext context);

        void ChildrenBound(ModelMap map, ParsingContext context);
    }
}