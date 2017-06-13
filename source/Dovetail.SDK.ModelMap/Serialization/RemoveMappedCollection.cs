using System.Xml.Linq;

namespace Dovetail.SDK.ModelMap.Serialization
{
	public class RemoveMappedCollection : IElementVisitor
	{
		public bool Matches(XElement element, ModelMap map, ParsingContext context)
		{
			return element.Name == "removeMappedCollection";
		}

		public void Visit(XElement element, ModelMap map, ParsingContext context)
		{
			var prop = context.Serializer.Deserialize<Instructions.RemoveMappedCollection>(element);
			map.AddInstruction(prop);
		}

		public void ChildrenBound(ModelMap map, ParsingContext context)
		{
		}
	}
}