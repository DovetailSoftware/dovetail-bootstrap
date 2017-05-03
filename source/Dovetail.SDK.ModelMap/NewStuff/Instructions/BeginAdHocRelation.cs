namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class BeginAdHocRelation : IModelMapInstruction
    {
        public string FromTableField { get; set; }
        public string ToTableName { get; set; }
        public string ToTableFieldName { get; set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}