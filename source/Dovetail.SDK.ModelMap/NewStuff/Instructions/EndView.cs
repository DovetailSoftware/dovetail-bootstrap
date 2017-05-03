namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class EndView : IModelMapInstruction
    {
        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}