using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.Bootstrap.History
{
    public class CaseHistoryAssemblerPolicy : IHistoryAssemblerPolicy
    {
        private readonly IClarifySessionCache _sessionCache;
        private readonly HistoryBuilder _historyBuilder;
        private readonly DefaultActEntryTemplateBuilder _defaultTemplateBuilder;
        private readonly ChildSubcaseActEntryTemplateBuilder _childSubcaseActEntryTemplateBuilder;

        public CaseHistoryAssemblerPolicy(IClarifySessionCache sessionCache, HistoryBuilder historyBuilder, DefaultActEntryTemplateBuilder defaultTemplateBuilder, ChildSubcaseActEntryTemplateBuilder childSubcaseActEntryTemplateBuilder)
        {
            _sessionCache = sessionCache;
            _historyBuilder = historyBuilder;
            _defaultTemplateBuilder = defaultTemplateBuilder;
            _childSubcaseActEntryTemplateBuilder = childSubcaseActEntryTemplateBuilder;
        }

        public bool Handles(WorkflowObject workflowObject)
        {
            return workflowObject.Type == WorkflowObject.Case;
        }

        public IEnumerable<HistoryItem> BuildHistory(WorkflowObject workflowObject, Filter actEntryFilter)
        {
            var subcaseIds = GetSubcaseIds(workflowObject);
            
            var caseHistory = _historyBuilder.Build(workflowObject, actEntryFilter, _defaultTemplateBuilder);

            var subcaseHistories = subcaseIds.Select(id => _historyBuilder.Build(new WorkflowObject("subcase") { Id = id, IsChild = true}, actEntryFilter, _childSubcaseActEntryTemplateBuilder));

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