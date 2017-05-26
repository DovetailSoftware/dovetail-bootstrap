namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
	public class PopVariableContext : IModelMapInstruction
	{
		public void Accept(IModelMapVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}