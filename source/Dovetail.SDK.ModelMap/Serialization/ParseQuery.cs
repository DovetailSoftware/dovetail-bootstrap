﻿using System.Xml.Linq;
using Dovetail.SDK.ModelMap.Instructions;

namespace Dovetail.SDK.ModelMap.Serialization
{
    public class ParseQuery : IElementVisitor
    {
        public bool Matches(XElement element, ModelMap map, ParsingContext context)
        {
            return element.Name == "query" && context.IsCurrent<ModelMap>();
        }

        public void Visit(XElement element, ModelMap map, ParsingContext context)
        {
            var query = context.Serializer.Deserialize<QueryElement>(element);

            var queryContext = query.Type == "view"
                ? (IModelMapInstruction) new BeginView(query.From)
                : new BeginTable(query.From); 
            
            map.AddInstruction(queryContext);
            context.PushObject(queryContext);
        }

        public void ChildrenBound(ModelMap map, ParsingContext context)
        {
	        var query = context.CurrentObject<IQueryContext>();
			var instruction = query is BeginView
				? (IModelMapInstruction) new EndView()
				: new EndTable();

			map.AddInstruction(instruction);
			context.PopObject();
		}

        private class QueryElement
        {
            [Required]
            public string From { get; set; }
            public string Type { get; set; }
        }
    }
}