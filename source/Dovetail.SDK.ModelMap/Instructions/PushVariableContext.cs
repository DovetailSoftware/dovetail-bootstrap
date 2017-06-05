using System.Collections.Generic;

namespace Dovetail.SDK.ModelMap.Instructions
{
	public class PushVariableContext : IModelMapInstruction
	{
		public PushVariableContext()
		{
			Attributes = new Dictionary<string, IDynamicValue>();
		}

		public IDictionary<string, IDynamicValue> Attributes { get; set; }

		public void Accept(IModelMapVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}