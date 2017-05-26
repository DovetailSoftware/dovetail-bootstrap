namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class EndRelation : IModelMapInstruction
    {
        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}