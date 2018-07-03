using System.Xml.Linq;
using Dovetail.SDK.History.Instructions;
using Dovetail.SDK.ModelMap.Serialization;

namespace Dovetail.SDK.History.Serialization
{
	public class ParseCancellationPolicy : IElementVisitor
	{
		public bool Matches(XElement element, ModelMap.ModelMap map, ParsingContext context)
		{
			return element.Name == "addCancellationPolicy";
		}

		public void Visit(XElement element, ModelMap.ModelMap map, ParsingContext context)
		{
			var policy = context.Serializer.Deserialize<BeginCancellationPolicy>(element);
			map.AddInstruction(policy);
		}

		public void ChildrenBound(ModelMap.ModelMap map, ParsingContext context)
		{
			map.AddInstruction(new EndCancellationPolicy());
		}
	}
}