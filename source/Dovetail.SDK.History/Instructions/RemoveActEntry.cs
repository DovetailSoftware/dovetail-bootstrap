using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Instructions;

namespace Dovetail.SDK.History.Instructions
{
	public class RemoveActEntry : IModelMapRemovalInstruction
	{
		public int Code { get; set; }
		public string Key { get { return Code.ToString(); } }

		public void Accept(IModelMapVisitor visitor)
		{
		}
	}
}