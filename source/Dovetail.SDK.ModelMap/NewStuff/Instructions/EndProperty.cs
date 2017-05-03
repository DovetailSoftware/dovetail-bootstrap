namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class EndProperty : IModelMapInstruction
    {

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}