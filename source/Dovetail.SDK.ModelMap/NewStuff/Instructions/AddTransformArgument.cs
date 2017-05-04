namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
	public class AddTransformArgument : IModelMapInstruction
	{
		public string Name { get; set; }
		public string Property { get; set; }
		public string Value { get; set; }

		public void Accept(IModelMapVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}