namespace Dovetail.SDK.ModelMap.Instructions
{
    public class EndModelMap : IModelMapInstruction
    {
        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}