namespace Dovetail.SDK.ModelMap.Instructions
{
	public class AddTag : IModelMapInstruction
	{
		public string Tag { get; set; }

		public void Accept(IModelMapVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}