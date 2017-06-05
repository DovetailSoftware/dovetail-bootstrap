namespace Dovetail.SDK.ModelMap.Instructions
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