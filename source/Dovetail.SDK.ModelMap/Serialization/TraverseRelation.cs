using System.Xml.Linq;
using Dovetail.SDK.ModelMap.Instructions;
using FubuCore;

namespace Dovetail.SDK.ModelMap.Serialization
{
    public class TraverseRelation : IElementVisitor
    {
        public bool Matches(XElement element, ModelMap map, ParsingContext context)
        {
	        return element.Name == "traverseRelation";
        }

        public void Visit(XElement element, ModelMap map, ParsingContext context)
        {
            var relationshipDef = context.Serializer.Deserialize<RelationshipDef>(element);
            if (relationshipDef.IsAdhoc)
            {
                map.AddInstruction(new BeginAdHocRelation
                {
                    FromTableField = relationshipDef.Field,
                    ToTableFieldName = relationshipDef.TargetField,
                    ToTableName = relationshipDef.Table,
					Key = relationshipDef.Key
                });
                return;
            }

            map.AddInstruction(new BeginRelation {  RelationName = relationshipDef.Name, Key = relationshipDef.Key });
        }

        public void ChildrenBound(ModelMap map, ParsingContext context)
        {
            map.AddInstruction(new EndRelation());
        }

        private class RelationshipDef
        {
            public IDynamicValue Name { get; set; }
            public string Type { get; set; }
            public IDynamicValue Field { get; set; }
            public IDynamicValue Table { get; set; }
            public IDynamicValue TargetField { get; set; }
			public string Key { get; set; }

            public bool IsAdhoc
            {
                get { return "adhoc".EqualsIgnoreCase(Type); }
            }
        }
    }
}