namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class EndMappedCollection : IModelMapInstruction
    {
        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}