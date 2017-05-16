using System.Xml.Linq;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization
{
	public class AddTag : IElementVisitor
	{
		public bool Matches(XElement element, ModelMap map, ParsingContext context)
		{
			return element.Name == "addTag";
		}

		public void Visit(XElement element, ModelMap map, ParsingContext context)
		{
			var instruction = context.Serializer.Deserialize<Instructions.AddTag>(element);
			map.AddInstruction(instruction);
		}

		public void ChildrenBound(ModelMap map, ParsingContext context)
		{
		}
	}
}