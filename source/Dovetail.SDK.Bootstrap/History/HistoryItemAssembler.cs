using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.History.Configuration;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History
{
	public class HistoryItemAssembler
	{
		private readonly IDictionary<int, ActEntryTemplate> _templatesByCode;
		private readonly WorkflowObject _workflowObject;
		private readonly ILogger _logger;

		public HistoryItemAssembler(IDictionary<int, ActEntryTemplate> templatesByCode, WorkflowObject workflowObject, ILogger logger)
		{
			_templatesByCode = templatesByCode;
			_workflowObject = workflowObject;
			_logger = logger;
		}

		public IEnumerable<HistoryItem> Assemble(ClarifyGeneric actEntryGeneric)
		{
			var codes = _templatesByCode.Values.Select(d => d.Code).ToArray();
			actEntryGeneric.DataFields.AddRange("act_code", "entry_time", "addnl_info");
			actEntryGeneric.Filter(f => f.IsIn("act_code", codes));

			//adding related generics expected by any fancy act entry templates
			var templateRelatedGenerics = traverseRelatedGenerics(actEntryGeneric);

			var actEntryContactGeneric = actEntryGeneric.TraverseWithFields("act_entry2contact", "first_name", "last_name", "e_mail");

			Func<ClarifyDataRow, HistoryItemContact> contactAssembler = actEntryRecord =>
			{
				var contactRows = actEntryRecord.RelatedRows(actEntryContactGeneric);
				if (contactRows.Length == 0)
					return null;

				var contactRecord = contactRows[0];
				var name = "{0} {1}".ToFormat(contactRecord.AsString("first_name"), contactRecord.AsString("last_name"));
				var email = contactRecord.AsString("e_mail");
				var id = contactRecord.DatabaseIdentifier();

				return new HistoryItemContact {Name = name, Id = id, Email = email};
			};

			var actEntryUserGeneric = actEntryGeneric.TraverseWithFields("act_entry2user", "objid", "login_name");
			var actEntryEmployeeGeneric = actEntryUserGeneric.TraverseWithFields("user2employee", "first_name", "last_name", "e_mail");

			Func<ClarifyDataRow, HistoryItemEmployee> employeeAssembler = actEntryRecord =>
			{
				var userRows = actEntryRecord.RelatedRows(actEntryUserGeneric);
				if (userRows.Length == 0)
					return new HistoryItemEmployee();

				var userRecord = userRows[0];
				var login = userRecord.AsString("login_name");
				var employeeRows = userRecord.RelatedRows(actEntryEmployeeGeneric);
				if (employeeRows.Length == 0)
					return new HistoryItemEmployee {Login = login};

				var employeeRecord = employeeRows[0];
				var name = "{0} {1}".ToFormat(employeeRecord.AsString("first_name"), employeeRecord.AsString("last_name"));
				var email = employeeRecord.AsString("e_mail");
				var id = employeeRecord.DatabaseIdentifier();

				return new HistoryItemEmployee
				{
					Name = name,
					Id = id,
					Login = login,
					Email = email,
					PerformedByContact = contactAssembler(actEntryRecord)
				};
			};

			actEntryGeneric.Query();

			var actEntryDTOS = assembleActEntryDTOs(actEntryGeneric, _templatesByCode, employeeAssembler);

			return actEntryDTOS.Select(dto => createActivityDTOFromMapper(dto, templateRelatedGenerics)).ToArray();
		}

		private IDictionary<ActEntryTemplate, ClarifyGeneric> traverseRelatedGenerics(ClarifyGeneric actEntryGeneric)
		{
			var relatedGenericByTemplate = new Dictionary<ActEntryTemplate, ClarifyGeneric>();
			foreach (var template in _templatesByCode.Values.Where(t => t.RelatedGenericRelationName.IsNotEmpty()))
			{
				var relatedGeneric = actEntryGeneric.TraverseWithFields(template.RelatedGenericRelationName, template.RelatedGenericFields);
				relatedGenericByTemplate.Add(template, relatedGeneric);
			}

			return relatedGenericByTemplate;
		}

		private IEnumerable<ActEntry> assembleActEntryDTOs(ClarifyGeneric actEntryGeneric, IDictionary<int, ActEntryTemplate> actEntryTemplatesByCode, Func<ClarifyDataRow, HistoryItemEmployee> employeeAssembler)
		{
			return actEntryGeneric.DataRows().Select(actEntryRecord =>
			{
				var code = actEntryRecord.AsInt("act_code");
				var template = actEntryTemplatesByCode[code];

				var when = actEntryRecord.AsDateTime("entry_time");

				var detail = actEntryRecord.AsString("addnl_info");
				var who = employeeAssembler(actEntryRecord);

				return new ActEntry {Template = template, When = when, Who = who, AdditionalInfo = detail, ActEntryRecord = actEntryRecord, Type = _workflowObject.Type};
			}).ToArray();
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

		private bool updateActivityDto(ActEntry actEntry, HistoryItem dto, ActEntryTemplate template,
			IDictionary<ActEntryTemplate, ClarifyGeneric> templateRelatedGenerics)
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
			return new HistoryItem
			{
				Id = _workflowObject.Id,
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