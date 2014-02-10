using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.Extensions;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;

namespace Dovetail.SDK.Bootstrap.History.TemplatePolicies
{
	public interface IAttachmentHistoryItemUpdater
	{
		void Update(DocInstDetail docInst, HistoryItem item);
	}

	public class DocInstDetail
	{
		public int ObjId { get; set; }
		public string Title { get; set; }
	}
	public class SeekerAttachmentPolicy : ActEntryTemplatePolicyExpression
	{
		private readonly IAttachmentHistoryItemUpdater _attachmentHistoryItemUpdater;

		public SeekerAttachmentPolicy(IAttachmentHistoryItemUpdater attachmentHistoryItemUpdater, IHistoryOutputParser historyOutputParser)
			: base(historyOutputParser)
		{
			_attachmentHistoryItemUpdater = attachmentHistoryItemUpdater;
		}

		protected override void DefineTemplate(WorkflowObject workflowObject)
		{
			ActEntry(8900).DisplayName(HistoryBuilderTokens.ATTACHMENT_ADDED)
				.GetRelatedRecord("act_entry2doc_inst")
				.WithFields("title")
				.UpdateActivityDTOWith((row, item, template) =>
				{
					//cancel the htmlizer as we are emitting HTML
					template.HTMLizer = i => { };

					var docInstDetail = new DocInstDetail { ObjId = row.DatabaseIdentifier(), Title = row.AsString("title").HtmlEncode() };
					_attachmentHistoryItemUpdater.Update(docInstDetail, item);
					item.Internal = string.Empty;
				});
		}
	}
}
