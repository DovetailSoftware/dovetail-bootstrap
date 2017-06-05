namespace Dovetail.SDK.ModelMap.Instructions
{
    public class EndView : IModelMapInstruction
    {
        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}