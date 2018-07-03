using System.Xml.Linq;
using Dovetail.SDK.History.Instructions;
using Dovetail.SDK.ModelMap.Serialization;

namespace Dovetail.SDK.History.Serialization
{
	public class ParseWhen : IElementVisitor
	{
		public bool Matches(XElement element, ModelMap.ModelMap map, ParsingContext context)
		{
			return element.Name == "when";
		}

		public void Visit(XElement element, ModelMap.ModelMap map, ParsingContext context)
		{
			var actEntry = context.Serializer.Deserialize<BeginWhen>(element);
			map.AddInstruction(actEntry);
		}

		public void ChildrenBound(ModelMap.ModelMap map, ParsingContext context)
		{
			map.AddInstruction(new EndWhen());
		}
	}
}