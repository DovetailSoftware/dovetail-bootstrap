using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.Extensions;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Clarify.Workflow;
using FChoice.Foundation.Schema;

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
		private readonly ILogger _logger;
		private readonly ISchemaCache _schemaCache;

		public SeekerAttachmentPolicy(IAttachmentHistoryItemUpdater attachmentHistoryItemUpdater, IHistoryOutputParser historyOutputParser, ILogger logger, ISchemaCache schemaCache)
			: base(historyOutputParser)
		{
			_attachmentHistoryItemUpdater = attachmentHistoryItemUpdater;
			_logger = logger;
			_schemaCache = schemaCache;
		}

		protected override void DefineTemplate(WorkflowObject workflowObject)
		{
			//use the work object info to get the object's relation to actentry
			var objectInfo = WorkflowObjectInfo.GetObjectInfo(workflowObject.Type);
			var table = _schemaCache.Tables[objectInfo.ObjectName];
			//unfortunately the object info doesn't have the relation from the workflow object to doc_inst.
			var docInstRelation = table.Relationships.Cast<ISchemaRelation>().First(r => r.TargetTable.Name == "doc_inst");

			ActEntry(8900).DisplayName(HistoryBuilderTokens.ATTACHMENT_ADDED)
				.GetRelatedRecord(objectInfo.ActivityRelation, workflowGeneric =>
				{
					var docInstGeneric = workflowGeneric.TraverseWithFields(docInstRelation.Name, "title");
					docInstGeneric.TraverseWithFields("attach_info2doc_path", "path");
				})
				.UpdateActivityDTOWith((caseRow, item, template) =>
				{
					var docInst = caseRow.RelatedRows(docInstRelation.Name).FirstOrDefault(d =>
					{
						var docPath = d.RelatedRows("attach_info2doc_path").First();
						return (item.Detail.Contains(docPath.AsString("path")));
					});

					if (docInst == null)
					{
						_logger.LogDebug("Could not find an attachment whose additional info matches one of the attachment paths. The history item will contain the plain additional info.");
						return;
					}

					//cancel the htmlizer as we are emitting HTML
					template.HTMLizer = i => { };

					var docInstDetail = new DocInstDetail
					{
						ObjId = docInst.DatabaseIdentifier(),
						Title = docInst.AsString("title").HtmlEncode()
					};
					_attachmentHistoryItemUpdater.Update(docInstDetail, item);
					item.Internal = string.Empty;
				});
		}
	}
}
