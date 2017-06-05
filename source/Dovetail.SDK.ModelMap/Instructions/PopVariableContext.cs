namespace Dovetail.SDK.ModelMap.Instructions
{
	public class PopVariableContext : IModelMapInstruction
	{
		public void Accept(IModelMapVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}