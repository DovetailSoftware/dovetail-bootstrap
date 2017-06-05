namespace Dovetail.SDK.ModelMap.Instructions
{
    public class EndMappedCollection : IModelMapInstruction
    {
        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}