using System.Collections.Generic;

namespace Dovetail.SDK.ModelMap.Instructions
{
	public class IncludePartial : IModelMapInstruction
	{
		public IncludePartial()
		{
			Attributes = new Dictionary<string, IDynamicValue>();
		}

		public string Name { get; set; }
		public IDictionary<string, IDynamicValue> Attributes { get; set; }

		public void Accept(IModelMapVisitor visitor)
		{
			// no-op
		}
	}
}