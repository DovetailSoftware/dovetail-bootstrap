using System.Collections.Generic;

namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
	public class IncludePartial : IModelMapInstruction
	{
		public IncludePartial()
		{
			Attributes = new Dictionary<string, string>();
		}

		public string Name { get; set; }
		public IDictionary<string, string> Attributes { get; set; }

		public void Accept(IModelMapVisitor visitor)
		{
			// no-op
		}
	}
}