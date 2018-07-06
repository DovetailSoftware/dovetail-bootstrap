using System.Xml.Linq;
using Dovetail.SDK.History.Instructions;
using Dovetail.SDK.ModelMap.Serialization;

namespace Dovetail.SDK.History.Serialization
{
	public class ParseRemoveActEntry : IElementVisitor
	{
		public bool Matches(XElement element, ModelMap.ModelMap map, ParsingContext context)
		{
			return element.Name == "removeActEntry";
		}

		public void Visit(XElement element, ModelMap.ModelMap map, ParsingContext context)
		{
			var prop = context.Serializer.Deserialize<RemoveActEntry>(element);
			map.AddInstruction(prop);
		}

		public void ChildrenBound(ModelMap.ModelMap map, ParsingContext context)
		{
		}
	}
}