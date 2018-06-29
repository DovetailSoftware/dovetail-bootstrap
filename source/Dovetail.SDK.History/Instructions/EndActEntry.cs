using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Instructions;
using FubuCore;

namespace Dovetail.SDK.History.Instructions
{
	public class EndActEntry : IModelMapInstruction
	{
		public void Accept(IModelMapVisitor visitor)
		{
			visitor.As<IHistoryModelMapVisitor>().Visit(this);
		}
	}
}