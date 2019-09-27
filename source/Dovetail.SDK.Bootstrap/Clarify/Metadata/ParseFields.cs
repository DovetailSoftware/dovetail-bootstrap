using System.Xml.Linq;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public class ParseFields : IXElementVisitor
	{
		public bool Matches(XElement element, ParsingContext context)
		{
			return element.Name == "field";
		}

		public void Visit(XElement element, ParsingContext context)
		{
			var field = context.Serializer.Deserialize<FieldSchemaMetadata>(element);
			context.CurrentObject<TableSchemaMetadata>().AddField(field);
			context.PushObject(field);
		}

		public void ChildrenBound(ParsingContext context)
		{
			context.PopObject();
		}
	}
}