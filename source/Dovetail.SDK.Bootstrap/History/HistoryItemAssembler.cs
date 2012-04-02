using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.History.Configuration;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.DataObjects;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History
{
    public class HistoryItemAssembler
    {
        private readonly IDictionary<int, ActEntryTemplate> _templatesByCode;
        private readonly WorkflowObject _workflowObject;
        private readonly ILocaleCache _localeCache;

        public HistoryItemAssembler(IDictionary<int, ActEntryTemplate> templatesByCode, WorkflowObject workflowObject, ILocaleCache localeCache)
        {
            _templatesByCode = templatesByCode;
            _workflowObject = workflowObject;
            _localeCache = localeCache;
        }

        public IEnumerable<HistoryItem> Assemble(ClarifyGeneric actEntryGeneric)
        {
            var codes = _templatesByCode.Values.Select(d => d.Code).ToArray();
            actEntryGeneric.DataFields.AddRange("act_code", "entry_time", "addnl_info");
            actEntryGeneric.Filter(f => f.IsIn("act_code", codes));

            var actEntryUserGeneric = actEntryGeneric.TraverseWithFields("act_entry2user", "objid", "login_name");
            var actEntryEmployeeGeneric = actEntryUserGeneric.TraverseWithFields("user2employee", "first_name", "last_name");

            //adding related generics expected by any fancy act entry templates
            var templateRelatedGenerics = traverseRelatedGenerics(actEntryGeneric);

            actEntryGeneric.Query();
			
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
                var id = employeeRecord.DatabaseIdentifier();

                return new HistoryItemEmployee {Name = name, Id = id, Login = login};
            };

            var actEntryDTOS = assembleActEntryDTOs(actEntryGeneric, _templatesByCode, employeeAssembler);

            return actEntryDTOS.Select(dto => createActivityDTOFromMapper(dto, templateRelatedGenerics)).ToArray();
        }

        private IDictionary<ActEntryTemplate, ClarifyGeneric> traverseRelatedGenerics(ClarifyGeneric actEntryGeneric)
        {
            var relatedGenericByTemplate = new Dictionary<ActEntryTemplate, ClarifyGeneric>();
            foreach (var actEntryTemplate in _templatesByCode.Values.Where(t => t.RelatedGenericRelationName.IsNotEmpty()))
            {
                var relatedGeneric = actEntryGeneric.TraverseWithFields(actEntryTemplate.RelatedGenericRelationName,
                                                                        actEntryTemplate.RelatedGenericFields);
                relatedGenericByTemplate.Add(actEntryTemplate, relatedGeneric);
            }

            return relatedGenericByTemplate;
        }

        private IEnumerable<ActEntry> assembleActEntryDTOs(ClarifyGeneric actEntryGeneric, IDictionary<int, ActEntryTemplate> actEntryTemplatesByCode, Func<ClarifyDataRow, HistoryItemEmployee> employeeAssembler)
        {
            return actEntryGeneric.DataRows().Select(actEntryRecord =>
                                                         {
                                                             var code = actEntryRecord.AsInt("act_code");
                                                             var template = actEntryTemplatesByCode[code];

                                                             var serverWhen = actEntryRecord.AsDateTime("entry_time");
                                                             var utcWhen = ConvertToUTC(serverWhen);
                                                             
                                                             var detail = actEntryRecord.AsString("addnl_info");
                                                             var who = employeeAssembler(actEntryRecord);

                                                             return new ActEntry { Template = template, When = utcWhen, Who = who, AdditionalInfo = detail, ActEntryRecord = actEntryRecord, Type = _workflowObject.Type };
                                                         }).ToArray();
        }

        private DateTime ConvertToUTC(DateTime when)
        {
            if (_localeCache.ServerTimeZone.UtcOffsetSeconds != 0)
            {
                var fromUtcOffset = getUTCOffset(_localeCache.ServerTimeZone, when);

                when = when.AddSeconds(0 - fromUtcOffset);
            }

            return when;
        }

        private static int getUTCOffset(ITimeZone fromZone, DateTime date)
        {
            var offset = fromZone.UtcOffsetSeconds;

            var isZoneInDST = fromZone.IsDaylightSavingsTime(date);

            if (isZoneInDST) offset += 3600;

            return offset;
        }

        private HistoryItem createActivityDTOFromMapper(ActEntry actEntry, IDictionary<ActEntryTemplate, ClarifyGeneric> templateRelatedGenerics)
        {
            var dto = defaultActivityDTOAssembler(actEntry);

            var actEntryTemplate = actEntry.Template;

            updateActivityDto(actEntry, dto, templateRelatedGenerics);

            if (isActivityDTOEditorPresent(actEntry))
            {
                actEntryTemplate.ActivityDTOEditor(dto);
            }

            actEntryTemplate.HTMLizer(dto);

            return dto;
        }

        private void updateActivityDto(ActEntry actEntry, HistoryItem dto, IDictionary<ActEntryTemplate, ClarifyGeneric> templateRelatedGenerics)
        {
            if (!isActivityDTOUpdaterPresent(actEntry)) return;

            var actEntryTemplate = actEntry.Template;
            var relatedRow = actEntry.ActEntryRecord;

            if (templateRelatedGenerics.ContainsKey(actEntryTemplate))
            {
                var relatedRows = actEntry.ActEntryRecord.RelatedRows(templateRelatedGenerics[actEntryTemplate]);

                relatedRow = relatedRows.Length > 0 ? relatedRows[0] : null;
            }

            if (relatedRow != null)
                actEntryTemplate.ActivityDTOUpdater(relatedRow, dto);
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