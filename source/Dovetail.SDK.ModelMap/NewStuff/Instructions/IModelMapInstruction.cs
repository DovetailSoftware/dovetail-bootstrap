namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public interface IModelMapInstruction
    {
        void Accept(IModelMapVisitor visitor);
    }
}