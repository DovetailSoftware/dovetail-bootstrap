using System.Xml.Linq;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization
{
	public class RemoveProperty : IElementVisitor
	{
		public bool Matches(XElement element, ModelMap map, ParsingContext context)
		{
			return element.Name == "removeProperty";
		}

		public void Visit(XElement element, ModelMap map, ParsingContext context)
		{
			var prop = context.Serializer.Deserialize<Instructions.RemoveProperty>(element);
			map.AddInstruction(prop);
		}

		public void ChildrenBound(ModelMap map, ParsingContext context)
		{
		}
	}
}