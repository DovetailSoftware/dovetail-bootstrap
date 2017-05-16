namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
	public class RemoveMappedProperty : IModelMapRemovalInstruction
	{
		public string Key { get; set; }

		public void Accept(IModelMapVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}