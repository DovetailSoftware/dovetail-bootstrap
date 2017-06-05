using Dovetail.SDK.Bootstrap.History;
using Dovetail.SDK.Bootstrap.History.TemplatePolicies;
using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Legacy;
using Dovetail.SDK.ModelMap.Legacy.Registration;
using FubuCore;
using FubuMVC.Core.Urls;

namespace Bootstrap.Web.Handlers.api.attachments
{
	public class AttachmentHistoryItemUpdater : IAttachmentHistoryItemUpdater
	{
		private readonly IUrlRegistry _urlRegistry;

		public AttachmentHistoryItemUpdater(IUrlRegistry urlRegistry)
		{
			_urlRegistry = urlRegistry;
		}

		public void Update(DocInstDetail docInst, HistoryItem item)
		{
			if (docInst.Title.IsEmpty())
			{
				return;
			}

			var urlFor = _urlRegistry.UrlFor(new AttachmentRequest {Id = docInst.ObjId});

			item.Detail = @"Added attachment <a href=""{0}"">{1}</a>".ToFormat(urlFor, docInst.Title);
		}
	}

	public class Attachment
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Path { get; set; }
	}

	public class AttachmentMap : ModelMap<Attachment>
	{
		protected override void MapDefinition()
		{
			FromTable("doc_inst")
				.Assign(d => d.Id).FromIdentifyingField("objid")
				.Assign(d => d.Title).FromField("title")
				.ViaRelation("attach_info2doc_path", path => path
					.Assign(d => d.Path).FromField("path")
				);
		}
	}

	public class get_Id_handler
	{
		private readonly IModelBuilder<Attachment> _builder;

		public get_Id_handler(IModelBuilder<Attachment> builder)
		{
			_builder = builder;
		}

		public Attachment Execute(AttachmentRequest request)
		{
			return _builder.GetOne(request.Id);
		}
	}

	public class AttachmentRequest
	{
		public int Id { get; set; }
	}
}