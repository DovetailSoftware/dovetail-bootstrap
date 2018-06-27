using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Instructions;
using FubuCore;

namespace Dovetail.SDK.History.Instructions
{
	public class BeginActEntry : IModelMapInstruction
	{
		public int Code { get; set; }
		public bool IsVerbose { get; set; }

		public void Accept(IModelMapVisitor visitor)
		{
			visitor.As<HistoryModelMapVisitor>().Visit(this);
		}
	}
}