using System.Xml.Linq;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization
{
    public class AddSort : IElementVisitor
    {
        public bool Matches(XElement element, ModelMap map, ParsingContext context)
        {
            return element.Name == "addSort";
        }

        public void Visit(XElement element, ModelMap map, ParsingContext context)
        {
            var instruction = context.Serializer.Deserialize<FieldSortMap>(element);
            map.AddInstruction(instruction);
        }

        public void ChildrenBound(ModelMap map, ParsingContext context)
        {
        }
    }
}