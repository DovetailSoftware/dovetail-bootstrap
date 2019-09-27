using System.Collections.Generic;
using System.Xml.Linq;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public class ParseTables : IXElementVisitor
	{
		public bool Matches(XElement element, ParsingContext context)
		{
			return element.Name == "table";
		}

		public void Visit(XElement element, ParsingContext context)
		{
			var table = context.Serializer.Deserialize<TableSchemaMetadata>(element);
			context.CurrentObject<List<TableSchemaMetadata>>().Add(table);
			context.PushObject(table);
		}

		public void ChildrenBound(ParsingContext context)
		{
			context.PopObject();
		}
	}
}
