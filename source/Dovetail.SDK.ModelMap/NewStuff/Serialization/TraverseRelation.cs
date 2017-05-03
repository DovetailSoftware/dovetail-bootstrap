using System.Xml.Linq;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization
{
    public class TraverseRelation : IElementVisitor
    {
        public bool Matches(XElement element, ModelMap map, ParsingContext context)
        {
            return element.Name == "traverseRelation" && context.IsCurrent<IQueryContext>();
        }

        public void Visit(XElement element, ModelMap map, ParsingContext context)
        {
            var relationshipDef = XElementSerializer.Deserialize<RelationshipDef>(element);
            if (relationshipDef.IsAdhoc)
            {
                map.AddInstruction(new BeginAdHocRelation
                {
                    FromTableField = relationshipDef.Field,
                    ToTableFieldName = relationshipDef.TargetField,
                    ToTableName = relationshipDef.Table
                });
                return;
            }

            map.AddInstruction(new BeginRelation {  RelationName = relationshipDef.Name });
        }

        public void ChildrenBound(ModelMap map, ParsingContext context)
        {
            map.AddInstruction(new EndRelation());
        }

        private class RelationshipDef
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Field { get; set; }
            public string Table { get; set; }
            public string TargetField { get; set; }

            public bool IsAdhoc
            {
                get { return "adhoc".EqualsIgnoreCase(Type); }
            }
        }
    }
}