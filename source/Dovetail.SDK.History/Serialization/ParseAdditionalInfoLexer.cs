using System.Xml.Linq;
using Dovetail.SDK.History.Instructions;
using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Instructions;
using Dovetail.SDK.ModelMap.Serialization;

namespace Dovetail.SDK.History.Serialization
{
	public class ParseAdditionalInfoLexer : IElementVisitor
	{
		public bool Matches(XElement element, ModelMap.ModelMap map, ParsingContext context)
		{
			return element.Name == "additionalInfoLexer" && context.IsCurrent<BeginActEntry>();
		}

		public void Visit(XElement element, ModelMap.ModelMap map, ParsingContext context)
		{
			var additionalInfo = context.Serializer.Deserialize<AdditionalInfoElement>(element);

			map.AddInstruction(new BeginProperty
			{
				DataType = new DynamicValue("string"),
				Field = new DynamicValue("addnl_info"),
				Key = new DynamicValue(AdditionalInfoLexerTransform.Key),
			});

			map.AddInstruction(new EndProperty());
			map.AddInstruction(new BeginTransform
			{
				Name = new DynamicValue("additionalInfoLexer")
			});
			map.AddInstruction(new AddTransformArgument
			{
				Name = new DynamicValue("pattern"),
				Value = additionalInfo.Pattern
			});
			map.AddInstruction(new EndTransform());
		}

		public void ChildrenBound(ModelMap.ModelMap map, ParsingContext context)
		{
		}

		public class AdditionalInfoElement
		{
			public IDynamicValue Pattern { get; set; }
		}
	}
}