namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public interface IModelMapInstruction
    {
        void Accept(IModelMapVisitor visitor);
    }

	public interface IModelMapRemovalInstruction : IModelMapInstruction
	{
		string Key { get; }
	}
}