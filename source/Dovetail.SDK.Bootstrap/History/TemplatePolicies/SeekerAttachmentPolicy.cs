using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.Extensions;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;
using FubuCore;

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
				.GetRelatedRecord("act_entry2case", caseGeneric =>
				{
				    var docInstGeneric = caseGeneric.TraverseWithFields("case_attch2doc_inst", "title");
					docInstGeneric.TraverseWithFields("attach_info2doc_path", "path");
				})
				.UpdateActivityDTOWith((caseRow, item, template) =>
				{
					var docInst = caseRow.RelatedRows("case_attch2doc_inst").FirstOrDefault(d =>
					{
						var docPath = d.RelatedRows("attach_info2doc_path").First();
						return (item.Detail.Contains(docPath.AsString("path")));
					});

					if (docInst == null) return;

					//item.IsCancelled = true;
		
					//cancel the htmlizer as we are emitting HTML
					template.HTMLizer = i => { };

					var docInstDetail = new DocInstDetail { ObjId = docInst.DatabaseIdentifier(), Title = docInst.AsString("title").HtmlEncode() };
					_attachmentHistoryItemUpdater.Update(docInstDetail, item);
					item.Internal = string.Empty;
				});
		}
	}
}
