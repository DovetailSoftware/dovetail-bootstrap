namespace Dovetail.SDK.ModelMap.Instructions
{
    public class EndRelation : IModelMapInstruction
    {
        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}