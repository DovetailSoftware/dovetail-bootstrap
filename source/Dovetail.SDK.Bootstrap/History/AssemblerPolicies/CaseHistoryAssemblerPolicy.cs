using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.TemplatePolicies;
using FChoice.Foundation;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.Bootstrap.History.AssemblerPolicies
{
    public class CaseHistoryAssemblerPolicy : IHistoryAssemblerPolicy
    {
        private readonly IClarifySessionCache _sessionCache;
        private readonly HistoryBuilder _historyBuilder;
        private readonly HistorySettings _historySettings;

        //TODO add settings object to allow getting subcases to be configurable - default (do not get subcase)

        public CaseHistoryAssemblerPolicy(IClarifySessionCache sessionCache, HistoryBuilder historyBuilder, HistorySettings historySettings)
        {
            _sessionCache = sessionCache;
            _historyBuilder = historyBuilder;
            _historySettings = historySettings;
        }

        public bool Handles(WorkflowObject workflowObject)
        {
            return workflowObject.Type == WorkflowObject.Case;
        }

        public IEnumerable<HistoryItem> BuildHistory(WorkflowObject workflowObject, Filter actEntryFilter)
        {
            if(!_historySettings.MergeCaseHistoryChildSubcases)
            {
                return _historyBuilder.Build(workflowObject, actEntryFilter);
            }

            var subcaseIds = GetSubcaseIds(workflowObject);
            
            var caseHistory = _historyBuilder.Build(workflowObject, actEntryFilter);

            var subcaseHistories = subcaseIds.Select(id =>
                                                         {
                                                             var subcaseWorkflowObject = new WorkflowObject(WorkflowObject.Subcase) { Id = id, IsChild = true};
                                                             return _historyBuilder.Build(subcaseWorkflowObject, actEntryFilter);
                                                         });

            var results = subcaseHistories.SelectMany(result => result).Concat(caseHistory);

            return results.OrderByDescending(r => r.When);
        }

        private IEnumerable<string> GetSubcaseIds(WorkflowObject workflowObject)
        {
            var clarifyDataSet = _sessionCache.GetUserSession().CreateDataSet();
            var caseGeneric = clarifyDataSet.CreateGenericWithFields("case");
            caseGeneric.AppendFilter("id_number", StringOps.Equals, workflowObject.Id);

            var subcaseGeneric = caseGeneric.TraverseWithFields("case2subcase", "id_number");

            caseGeneric.Query();

            return subcaseGeneric.Count > 0 ? subcaseGeneric.DataRows().Select(s => s.AsString("id_number")) : new string[0];
        }
    }
}