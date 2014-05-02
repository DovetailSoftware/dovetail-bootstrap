using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.History.Configuration;
using FChoice.Foundation.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FubuCore;
using FubuLocalization;

namespace Dovetail.SDK.Bootstrap.History
{
	public class HistoryRequest
	{
		public WorkflowObject WorkflowObject { get; set; }
		public bool ShowAllActivities { get; set; }
		public int? HistoryItemLimit { get; set; }
		public DateTime? Since { get; set; }
	}

	public interface IHistoryItemAssembler
	{
		IEnumerable<HistoryItem> Assemble(ClarifyGeneric actEntryGeneric, IDictionary<int, ActEntryTemplate> templatesByCode, HistoryRequest historyRequest);
	}

	public class HistoryItemAssembler : IHistoryItemAssembler
	{
		private readonly ILogger _logger;
		private readonly IHistoryEmployeeAssembler _employeeAssembler;
		private readonly IHistoryContactAssembler _contactAssembler;
		private readonly IListCache _listCache;

		public HistoryItemAssembler(ILogger logger, IHistoryEmployeeAssembler employeeAssembler, IHistoryContactAssembler contactAssembler, IListCache listCache)
		{
			_logger = logger;
			_employeeAssembler = employeeAssembler;
			_contactAssembler = contactAssembler;
			_listCache = listCache;
		}

		public IEnumerable<HistoryItem> Assemble(ClarifyGeneric actEntryGeneric, IDictionary<int, ActEntryTemplate> templatesByCode, HistoryRequest historyRequest)
		{
			var codes = templatesByCode.Values.Select(d => d.Code).ToArray();
			actEntryGeneric.DataFields.AddRange("act_code", "entry_time", "addnl_info");

			if (!historyRequest.ShowAllActivities)
			{
				actEntryGeneric.Filter(f => f.IsIn("act_code", codes));
			}

			//adding related generics expected by any fancy act entry templates
			var templateRelatedGenerics = traverseRelatedGenerics(actEntryGeneric, templatesByCode);

			_employeeAssembler.TraverseEmployee(actEntryGeneric);
			_contactAssembler.TraverseContact(actEntryGeneric);
			if (historyRequest.HistoryItemLimit.HasValue)
			{
				actEntryGeneric.MaximumRows = historyRequest.HistoryItemLimit.Value;
			}
			actEntryGeneric.Query();
			
			var actEntryDTOS = actEntryGeneric.DataRows().Select(actEntryRecord =>
			{
				var id = historyRequest.WorkflowObject.Id;
				if (historyRequest.WorkflowObject.IsChild) 
				{
					//child objects are loaded in bulk so historyRequest.WorkflowObject.Id is not the correct id
					//find the act_entry's parent generic id field value 
					var info = WorkflowObjectInfo.GetObjectInfo(actEntryGeneric.ParentRelation.TargetName);
					var parentId = actEntryRecord.AsInt(actEntryGeneric.ParentRelation.Name);
					var parentRow = actEntryGeneric.ParentGeneric.DataRows().First(r => r.DatabaseIdentifier() == parentId);
					id = (info.HasIDFieldName) ? parentRow[info.IDFieldName].ToString() : Convert.ToString(parentRow.DatabaseIdentifier());
				}

				var code = actEntryRecord.AsInt("act_code");

				return new ActEntry
				{
					Id = id,
					Type = historyRequest.WorkflowObject.Type,
					Template = findTemplateByActCode(code, templatesByCode),
					When = actEntryRecord.AsDateTime("entry_time"),
					Who = _employeeAssembler.Assemble(actEntryRecord, _contactAssembler),
					AdditionalInfo = actEntryRecord.AsString("addnl_info"),
					ActEntryRecord = actEntryRecord
				};

			}).ToList();

			return actEntryDTOS.Select(dto => createActivityDTOFromMapper(dto, templateRelatedGenerics)).Where(i=>!i.IsCancelled).ToList();
		}

		private ActEntryTemplate findTemplateByActCode(int code, IDictionary<int, ActEntryTemplate> templatesByCode)
		{
			if (templatesByCode.ContainsKey(code))
			{
				return templatesByCode[code];
			}

			_logger.LogDebug("No template found for act_code {0}. Using default act entry template.", code);
			
			//create a copy of the default template with the code and display name set correctly.
			var template = new ActEntryTemplate(templatesByCode[ActEntryTemplatePolicyConfiguration.DefaultActEntryTemplateMagicCode])
			{
				Code = code,
			};
			return template;
		}

		private IDictionary<ActEntryTemplate, ClarifyGeneric> traverseRelatedGenerics(ClarifyGeneric actEntryGeneric, IDictionary<int, ActEntryTemplate> templatesByCode)
		{
			var relatedGenericByTemplate = new Dictionary<ActEntryTemplate, ClarifyGeneric>();
			var genericsByRelation = new Dictionary<string, ClarifyGeneric>();

			foreach (var template in templatesByCode.Values.Where(t => t.RelatedGenericRelationName.IsNotEmpty()))
			{
				//avoid traversing the same relation (from actentry) twice.
				ClarifyGeneric relatedGeneric;
				if (genericsByRelation.ContainsKey(template.RelatedGenericRelationName))
				{
					relatedGeneric = genericsByRelation[template.RelatedGenericRelationName];
				}
				else
				{
					relatedGeneric = actEntryGeneric.TraverseWithFields(template.RelatedGenericRelationName);
					if (template.RelatedGenericAction != null)
					{
						template.RelatedGenericAction(relatedGeneric);
					}
					genericsByRelation.Add(template.RelatedGenericRelationName, relatedGeneric);
				}

				relatedGeneric.DataFields.AddRange(template.RelatedGenericFields);
				relatedGenericByTemplate.Add(template, relatedGeneric);
			}

			return relatedGenericByTemplate;
		}

		private HistoryItem createActivityDTOFromMapper(ActEntry actEntry, IDictionary<ActEntryTemplate, ClarifyGeneric> templateRelatedGenerics)
		{
			var dto = defaultActivityDTOAssembler(actEntry);

			var template = new ActEntryTemplate(actEntry.Template);

			updateActivityDto(actEntry, dto, template, templateRelatedGenerics);

			if (isActivityDTOEditorPresent(template))
			{
				template.ActivityDTOEditor(dto);
			}

			template.HTMLizer(dto);

			return dto;
		}

		private bool updateActivityDto(ActEntry actEntry, HistoryItem dto, ActEntryTemplate template, IDictionary<ActEntryTemplate, ClarifyGeneric> templateRelatedGenerics)
		{
			if (!isActivityDTOUpdaterPresent(template)) return false;

			var relatedRow = actEntry.ActEntryRecord;
			var relatedGenericKey = actEntry.Template;

			if (templateRelatedGenerics.ContainsKey(relatedGenericKey))
			{
				var relatedRows = actEntry.ActEntryRecord.RelatedRows(templateRelatedGenerics[relatedGenericKey]);

				//when a row related to the act entry was retrieved give that row to the updater.
				relatedRow = relatedRows.Length > 0 ? relatedRows[0] : null;

				if (relatedRow == null)
				{
					_logger.LogDebug("Activity updater for code {0} against object {1}-{2} did not work because no related row for relation {3} was found."
							.ToFormat(template.Code, dto.Type, dto.Id, template.RelatedGenericRelationName));
					return false;
				}
			}

			if (relatedRow != null)
			{
				template.ActivityDTOUpdater(relatedRow, dto, template);
				return true;
			}

			return false;
		}

		private HistoryItem defaultActivityDTOAssembler(ActEntry actEntry)
		{
			//When the display name is not set use the GBST list to get a localized
			if (actEntry.Template.DisplayName == null)
			{
				var actEntryNameElement = _listCache.GetGbstListElements("Activity Name", false).FirstOrDefault(e=>e.Rank == actEntry.Template.Code);

				string displayName;
				if (actEntryNameElement == null)
				{
					_logger.LogWarn("No entry was found in GBST 'Activity Name' for code {0} (by rank). Using code value as a display name so there is something to show.", actEntry.Template.Code);
					displayName = Convert.ToString(actEntry.Template.Code);
				}
				else
				{
					displayName = actEntryNameElement.Title;
				}

				actEntry.Template.DisplayName = StringToken.FromKeyString("HISTORY_ACTIVITY_NAME_{0}".ToFormat(actEntry.Template.Code), displayName);
			}

			return new HistoryItem
			{
				Id = actEntry.Id,
				Type = actEntry.Type,
				Title = actEntry.Template.DisplayName,
				Who = actEntry.Who,
				When = actEntry.When,
				Detail = actEntry.AdditionalInfo
			};
		}

		private static bool isActivityDTOUpdaterPresent(ActEntryTemplate template)
		{
			return template.ActivityDTOUpdater != null;
		}

		private static bool isActivityDTOEditorPresent(ActEntryTemplate template)
		{
			return template.ActivityDTOEditor != null;
		}
	}
}