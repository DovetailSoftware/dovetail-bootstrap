using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Instructions;
using FubuCore;

namespace Dovetail.SDK.History.Instructions
{
	public class BeginWhen : IModelMapInstruction
	{
		public bool IsChild { get; set; }

		public void Accept(IModelMapVisitor visitor)
		{
			visitor.As<HistoryModelMapVisitor>().Visit(this);
		}
	}
}