using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Instructions;
using FubuCore;

namespace Dovetail.SDK.History.Instructions
{
	public class BeginCancellationPolicy : IModelMapInstruction
	{
		public IDynamicValue Name { get; set; }

		public void Accept(IModelMapVisitor visitor)
		{
			visitor.As<HistoryModelMapVisitor>().Visit(this);
		}
	}
}