using Dovetail.SDK.History.Instructions;
using Dovetail.SDK.ModelMap;

namespace Dovetail.SDK.History
{
	public interface IHistoryModelMapVisitor : IModelMapVisitor
	{
		void Visit(BeginActEntry instruction);
		void Visit(EndActEntry instruction);
		void Visit(BeginCancellationPolicy instruction);
		void Visit(EndCancellationPolicy instruction);
		void Visit(BeginWhen instruction);
		void Visit(EndWhen instruction);
	}
}
