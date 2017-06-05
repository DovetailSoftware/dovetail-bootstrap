namespace Dovetail.SDK.ModelMap.Instructions
{
	public class EndTransform : IModelMapInstruction
	{
		public void Accept(IModelMapVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}