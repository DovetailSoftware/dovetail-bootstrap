using System.Xml.Linq;

namespace Dovetail.SDK.ModelMap.Serialization
{
	public class RemoveMappedProperty : IElementVisitor
	{
		public bool Matches(XElement element, ModelMap map, ParsingContext context)
		{
			return element.Name == "removeMappedProperty";
		}

		public void Visit(XElement element, ModelMap map, ParsingContext context)
		{
			var prop = context.Serializer.Deserialize<Instructions.RemoveMappedProperty>(element);
			map.AddInstruction(prop);
		}

		public void ChildrenBound(ModelMap map, ParsingContext context)
		{
		}
	}
}