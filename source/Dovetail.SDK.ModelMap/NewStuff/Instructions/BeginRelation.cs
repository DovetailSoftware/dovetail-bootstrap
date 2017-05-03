namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class BeginRelation : IModelMapInstruction
    {
        public string RelationName { get; set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}