using System.Xml.Linq;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization
{
    public class AddMappedProperty : IElementVisitor
    {
        public bool Matches(XElement element, ModelMap map, ParsingContext context)
        {
            return element.Name == "addMappedProperty";
        }

        public void Visit(XElement element, ModelMap map, ParsingContext context)
        {
            var prop = context.Serializer.Deserialize<BeginMappedProperty>(element);
            map.AddInstruction(prop);
        }

        public void ChildrenBound(ModelMap map, ParsingContext context)
        {
            map.AddInstruction(new EndMappedProperty());
        }
    }
}