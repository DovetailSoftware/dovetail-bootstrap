namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class EndModelMap : IModelMapInstruction
    {
        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}