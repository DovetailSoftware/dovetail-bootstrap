namespace Dovetail.SDK.History
{
	public interface IHistoryMapRegistry
	{
		ModelMap.ModelMap Find(WorkflowObject workflowObject);
		ModelMap.ModelMap FindPartial(string name);
	}
}