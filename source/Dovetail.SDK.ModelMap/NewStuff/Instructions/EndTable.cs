namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class EndTable : IModelMapInstruction
    {
        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}