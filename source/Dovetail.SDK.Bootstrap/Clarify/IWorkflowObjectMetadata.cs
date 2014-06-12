using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface IWorkflowObjectMetadata
	{
		string Alias { get; set; }
		WorkflowObjectInfo Register();
	}
}