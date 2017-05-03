namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class EndMappedProperty : IModelMapInstruction
    {
        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}