namespace Dovetail.SDK.ModelMap.Instructions
{
    public class EndMappedProperty : IModelMapInstruction
    {
        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}