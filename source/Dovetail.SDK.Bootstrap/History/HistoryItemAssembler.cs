using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History
{
    public class HistoryItemAssembler
    {
        private readonly IDictionary<int, ActEntryTemplate> _templatesByCode;
        private IDictionary<ActEntryTemplate, ClarifyGeneric> _templateRelatedGenerics;

        public HistoryItemAssembler(IDictionary<int, ActEntryTemplate> templatesByCode)
        {
            _templatesByCode = templatesByCode;
        }

        public IEnumerable<HistoryItem> Assemble(ClarifyGeneric actEntryGeneric)
        {
            var codes = _templatesByCode.Values.Select(d => d.Code);
            actEntryGeneric.DataFields.AddRange("act_code", "entry_time", "addnl_info");
            actEntryGeneric.AppendFilterInList("act_code", true, codes.ToArray());

            var actEntryUserGeneric = actEntryGeneric.TraverseWithFields("act_entry2user", "objid", "login_name");
            var actEntryEmployeeGeneric = actEntryUserGeneric.TraverseWithFields("user2employee", "first_name", "last_name");

            //TODO cache these child generics so we don't have to do handsprings to retrieve them
            //adding related generics expected by any fancy act entry templates
            _templateRelatedGenerics = TraverseRelatedGenerics(actEntryGeneric);

            actEntryGeneric.Query();
			
            Func<ClarifyDataRow, HistoryItemEmployee> employeeAssembler = actEntryRecord =>
            {
                var userRows = actEntryRecord.RelatedRows(actEntryUserGeneric);
                if (userRows.Length == 0)
                    return new HistoryItemEmployee();

                var userRecord = userRows[0];
                var login = userRecord["login_name"].ToString();
                var employeeRows = userRecord.RelatedRows(actEntryEmployeeGeneric);
                if (employeeRows.Length == 0)
                    return new HistoryItemEmployee {Login = login};

                var employeeRecord = employeeRows[0];
                var name = "{0} {1}".ToFormat(employeeRecord["first_name"], employeeRecord["last_name"]);
                var id = Convert.ToInt32(employeeRecord.UniqueID);

                return new HistoryItemEmployee {Name = name, Id = id, Login = login};
            };

            var actEntryDTOS = assembleActEntryDTOs(actEntryGeneric, _templatesByCode, employeeAssembler);

            return actEntryDTOS.Select(createActivityDTOFromMapper).ToArray();
        }

        private IDictionary<ActEntryTemplate, ClarifyGeneric> TraverseRelatedGenerics(ClarifyGeneric actEntryGeneric)
        {
            var relatedGenericByTemplate = new Dictionary<ActEntryTemplate, ClarifyGeneric>();
            foreach (var actEntryTemplate in _templatesByCode.Values.Where(t => t.RelatedGenericRelation.IsNotEmpty()))
            {
                var relatedGeneric = actEntryGeneric.TraverseWithFields(actEntryTemplate.RelatedGenericRelation,
                                                                        actEntryTemplate.RelatedGenericFields);
                relatedGenericByTemplate.Add(actEntryTemplate, relatedGeneric);
            }

            return relatedGenericByTemplate;
        }

        private static IEnumerable<ActEntry> assembleActEntryDTOs(ClarifyGeneric actEntryGeneric, IDictionary<int, ActEntryTemplate> actEntryTemplatesByCode, Func<ClarifyDataRow, HistoryItemEmployee> employeeAssembler)
        {
            //HACK using the act entry parent table name to set the type of object (case, or subcase) for this entry 
            var objectType = actEntryGeneric.ParentGeneric.TableName;

            foreach (ClarifyDataRow actEntryRecord in actEntryGeneric.Rows)
            {
                var code = Convert.ToInt32(actEntryRecord["act_code"]);
                var template = actEntryTemplatesByCode[code];

                var when = Convert.ToDateTime(actEntryRecord["entry_time"]);
                var detail = actEntryRecord["addnl_info"].ToString();
                var who = employeeAssembler(actEntryRecord);

                yield return new ActEntry { Template = template, When = when, Who = who, AdditionalInfo = detail, ActEntryRecord = actEntryRecord, Type = objectType};
            }
        }

        private HistoryItem createActivityDTOFromMapper(ActEntry actEntry)
        {
            var dto = defaultActivityDTOAssembler(actEntry);

            var actEntryTemplate = actEntry.Template;

            updateActivityDto(actEntry, dto);

            if (isActivityDTOEditorPresent(actEntry))
            {
                actEntryTemplate.ActivityDTOEditor(dto);
            }

            actEntryTemplate.HTMLizer(dto);

            return dto;
        }

        private void updateActivityDto(ActEntry actEntry, HistoryItem dto)
        {
            if (!isActivityDTOUpdaterPresent(actEntry)) return;

            var actEntryTemplate = actEntry.Template;
            var relatedRow = actEntry.ActEntryRecord;

            if (actEntryTemplate.RelatedGenericRelation.IsNotEmpty() &&
                _templateRelatedGenerics.ContainsKey(actEntryTemplate))
            {
                var relatedRows = actEntry.ActEntryRecord.RelatedRows(_templateRelatedGenerics[actEntryTemplate]);

                relatedRow = relatedRows.Length > 0 ? relatedRows[0] : null;
            }

            if (relatedRow != null)
                actEntryTemplate.ActivityDTOUpdater(relatedRow, dto);
        }

        private static HistoryItem defaultActivityDTOAssembler(ActEntry actEntry)
        {
            return new HistoryItem
                       {
                           Id = actEntry.ActEntryRecord.DatabaseIdentifier(),
                           Type = actEntry.Type,
                           Kind = actEntry.Template.DisplayName,
                           Who = actEntry.Who,
                           When = actEntry.When,
                           Detail = actEntry.AdditionalInfo
                       };
        }

        private static bool isActivityDTOUpdaterPresent(ActEntry actEntry)
        {
            return actEntry.Template.ActivityDTOUpdater != null;
        }

        private static bool isActivityDTOEditorPresent(ActEntry actEntry)
        {
            return actEntry.Template.ActivityDTOEditor != null;
        }

    }
}