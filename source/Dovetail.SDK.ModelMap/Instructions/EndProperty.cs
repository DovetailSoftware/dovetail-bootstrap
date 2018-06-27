namespace Dovetail.SDK.ModelMap.Instructions
{
    public class EndProperty : IModelMapInstruction
    {
        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}