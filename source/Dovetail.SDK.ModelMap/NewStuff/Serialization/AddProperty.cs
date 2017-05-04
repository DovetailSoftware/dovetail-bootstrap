using System.Xml.Linq;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization
{
    public class AddProperty : IElementVisitor
    {
        public bool Matches(XElement element, ModelMap map, ParsingContext context)
        {
            return element.Name == "addProperty" && context.IsCurrent<IQueryContext>();
        }

        public void Visit(XElement element, ModelMap map, ParsingContext context)
        {
            var prop = context.Serializer.Deserialize<BeginProperty>(element);
            map.AddInstruction(prop);
        }

        public void ChildrenBound(ModelMap map, ParsingContext context)
        {
            map.AddInstruction(new EndProperty());
        }
    }
}