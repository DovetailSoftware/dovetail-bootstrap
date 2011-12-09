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

        public HistoryItemAssembler(IDictionary<int, ActEntryTemplate> templatesByCode)
        {
            _templatesByCode = templatesByCode;
        }

        public IEnumerable<HistoryItem> Assemble(ClarifyGeneric actEntryGeneric)
        {
            var codes = _templatesByCode.Values.Select(d => d.Code);
            actEntryGeneric.DataFields.AddRange("act_code", "entry_time", "addnl_info");
            actEntryGeneric.AppendFilterInList("act_code", true, codes.ToArray());

            var actEntryUserGeneric = actEntryGeneric.Traverse("act_entry2user");
            actEntryUserGeneric.DataFields.AddRange("objid", "login_name");

            var actEntryEmployeeGeneric = actEntryUserGeneric.Traverse("user2employee");
            actEntryEmployeeGeneric.DataFields.AddRange("first_name", "last_name");

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

        private static IEnumerable<ActEntry> assembleActEntryDTOs(ClarifyGeneric actEntryGeneric, IDictionary<int, ActEntryTemplate> actEntryTemplatesByCode, Func<ClarifyDataRow, HistoryItemEmployee> employeeAssembler)
        {
            foreach (ClarifyDataRow actEntryRecord in actEntryGeneric.Rows)
            {
                var code = Convert.ToInt32(actEntryRecord["act_code"]);
                var template = actEntryTemplatesByCode[code];

                var when = Convert.ToDateTime(actEntryRecord["entry_time"]);
                var detail = actEntryRecord["addnl_info"].ToString();
                var who = employeeAssembler(actEntryRecord);

                yield return new ActEntry { Template = template, When = when, Who = who, AdditionalInfo = detail, ActEntryRecord = actEntryRecord };
            }
        }

        private static HistoryItem createActivityDTOFromMapper(ActEntry actEntry)
        {
            var dto = defaultActivityDTOAssembler(actEntry);

            if (isActivityDTOUpdaterPresent(actEntry))
            {
                var relatedRow = actEntry.ActEntryRecord;

                if (actEntry.Template.RelatedGeneric != null)
                {
                    var relatedRows = actEntry.ActEntryRecord.RelatedRows(actEntry.Template.RelatedGeneric);

                    relatedRow = relatedRows.Length > 0 ? relatedRows[0] : null;
                }

                if (relatedRow != null)
                    actEntry.Template.ActivityDTOUpdater(relatedRow, dto);
            }

            if (isActivityDTOEditorPresent(actEntry))
            {
                actEntry.Template.ActivityDTOEditor(dto);
            }

            actEntry.Template.HTMLizer(dto);

            return dto;
        }

        private static HistoryItem defaultActivityDTOAssembler(ActEntry actEntry)
        {
            return new HistoryItem
                       {
                           Id = actEntry.ActEntryRecord.DatabaseIdentifier(),
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