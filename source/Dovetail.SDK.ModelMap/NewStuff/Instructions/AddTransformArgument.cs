namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
	public class AddTransformArgument : IModelMapInstruction
	{
		public IDynamicValue Name { get; set; }
		public IDynamicValue Property { get; set; }
		public IDynamicValue Value { get; set; }

		public void Accept(IModelMapVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}