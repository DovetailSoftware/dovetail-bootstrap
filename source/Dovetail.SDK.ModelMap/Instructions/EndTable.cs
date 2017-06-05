namespace Dovetail.SDK.ModelMap.Instructions
{
    public class EndTable : IModelMapInstruction
    {
        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}