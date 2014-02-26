using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.History.Configuration;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History
{
	public interface IHistoryItemAssembler
	{
		IEnumerable<HistoryItem> Assemble(ClarifyGeneric actEntryGeneric, IDictionary<int, ActEntryTemplate> templatesByCode, WorkflowObject workflowObject);
	}

	public class HistoryItemAssembler : IHistoryItemAssembler
	{
		private readonly ILogger _logger;
		private readonly IHistoryEmployeeAssembler _employeeAssembler;
		private readonly IHistoryContactAssembler _contactAssembler;

		public HistoryItemAssembler(ILogger logger, IHistoryEmployeeAssembler employeeAssembler, IHistoryContactAssembler contactAssembler)
		{
			_logger = logger;
			_employeeAssembler = employeeAssembler;
			_contactAssembler = contactAssembler;
		}

		public IEnumerable<HistoryItem> Assemble(ClarifyGeneric actEntryGeneric, IDictionary<int, ActEntryTemplate> templatesByCode, WorkflowObject workflowObject)
		{
			var codes = templatesByCode.Values.Select(d => d.Code).ToArray();
			actEntryGeneric.DataFields.AddRange("act_code", "entry_time", "addnl_info");
			actEntryGeneric.Filter(f => f.IsIn("act_code", codes));

			//adding related generics expected by any fancy act entry templates
			var templateRelatedGenerics = traverseRelatedGenerics(actEntryGeneric, templatesByCode);

			_employeeAssembler.TraverseEmployee(actEntryGeneric);
			_contactAssembler.TraverseContact(actEntryGeneric);
			actEntryGeneric.Query();

			var actEntryDTOS = actEntryGeneric.DataRows().Select(actEntryRecord =>
			{
				var code = actEntryRecord.AsInt("act_code");
				var template = templatesByCode[code];

				var when = actEntryRecord.AsDateTime("entry_time");

				var detail = actEntryRecord.AsString("addnl_info");
				var who = _employeeAssembler.Assemble(actEntryRecord, _contactAssembler);

				return new ActEntry { Template = template, When = when, Who = who, AdditionalInfo = detail, ActEntryRecord = actEntryRecord, Type = workflowObject.Type };
			}).ToList();

			return actEntryDTOS.Select(dto => createActivityDTOFromMapper(dto, workflowObject, templateRelatedGenerics)).Where(i=>!i.IsCancelled).ToList();
		}

		private IDictionary<ActEntryTemplate, ClarifyGeneric> traverseRelatedGenerics(ClarifyGeneric actEntryGeneric, IDictionary<int, ActEntryTemplate> templatesByCode)
		{
			var relatedGenericByTemplate = new Dictionary<ActEntryTemplate, ClarifyGeneric>();
			foreach (var template in templatesByCode.Values.Where(t => t.RelatedGenericRelationName.IsNotEmpty()))
			{
				var relatedGeneric = actEntryGeneric.TraverseWithFields(template.RelatedGenericRelationName);
				template.RelatedGenericAction(relatedGeneric);
				relatedGenericByTemplate.Add(template, relatedGeneric);
			}

			return relatedGenericByTemplate;
		}

		private HistoryItem createActivityDTOFromMapper(ActEntry actEntry, WorkflowObject workflowObject, IDictionary<ActEntryTemplate, ClarifyGeneric> templateRelatedGenerics)
		{
			var dto = defaultActivityDTOAssembler(actEntry, workflowObject);

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

		private HistoryItem defaultActivityDTOAssembler(ActEntry actEntry, WorkflowObject workflowObject)
		{
			return new HistoryItem
			{
				Id = workflowObject.Id,
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