using System.Xml.Linq;
using Dovetail.SDK.ModelMap.Instructions;

namespace Dovetail.SDK.ModelMap.Serialization
{
	public class AddMappedCollection : IElementVisitor
	{
		public bool Matches(XElement element, ModelMap map, ParsingContext context)
		{
			return element.Name == "addMappedCollection";
		}

		public void Visit(XElement element, ModelMap map, ParsingContext context)
		{
			var prop = context.Serializer.Deserialize<BeginMappedCollection>(element);
			map.AddInstruction(prop);
		}

		public void ChildrenBound(ModelMap map, ParsingContext context)
		{
			map.AddInstruction(new EndMappedCollection());
		}
	}

	public class AddTransform : IElementVisitor
	{
		public bool Matches(XElement element, ModelMap map, ParsingContext context)
		{
			return element.Name == "addTransform";
		}

		public void Visit(XElement element, ModelMap map, ParsingContext context)
		{
			var instruction = context.Serializer.Deserialize<BeginTransform>(element);
			map.AddInstruction(instruction);
			context.PushObject(instruction);
		}

		public void ChildrenBound(ModelMap map, ParsingContext context)
		{
			context.PopObject();
			map.AddInstruction(new EndTransform());
		}
	}

	public class AddArgument : IElementVisitor
	{
		public bool Matches(XElement element, ModelMap map, ParsingContext context)
		{
			return element.Name == "addArgument";
		}

		public void Visit(XElement element, ModelMap map, ParsingContext context)
		{
			var instruction = context.Serializer.Deserialize<AddTransformArgument>(element);
			map.AddInstruction(instruction);
		}

		public void ChildrenBound(ModelMap map, ParsingContext context)
		{
		}
	}
}