using System.Linq;
using System.Xml.Linq;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization
{
	public class AddPartial : IElementVisitor
	{
		public bool Matches(XElement element, ModelMap map, ParsingContext context)
		{
			return element.Name == "addPartial";
		}

		public void Visit(XElement element, ModelMap map, ParsingContext context)
		{
			var instruction = context.Serializer.Deserialize<IncludePartial>(element);
			foreach (var attribute in element.Attributes().Where(_ => !_.Name.ToString().EqualsIgnoreCase("name")))
			{
				instruction.Attributes.Add(attribute.Name.ToString(), new DynamicValue(attribute.Value));
			}

			map.AddInstruction(instruction);
		}

		public void ChildrenBound(ModelMap map, ParsingContext context)
		{
		}
	}
}