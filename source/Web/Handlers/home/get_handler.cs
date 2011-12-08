using System;
using Dovetail.SDK.Bootstrap.Clarify;

namespace Bootstrap.Web.Handlers.home
{
    public class get_handler
    {
        private readonly IClarifySessionCache _sessionCache;

        public get_handler(IClarifySessionCache sessionCache)
        {
            _sessionCache = sessionCache;
        }

        public HomeModel Execute(HomeRequest request)
        {
            var sqlHelper = _sessionCache.GetUserSession().CreateSqlHelper("SELECT count(*) from table_case");
            var caseCount = Convert.ToInt32(sqlHelper.ExecuteScalar());
            
            sqlHelper.CommandText = "SELECT count(*) from table_subcase";
            var subcaseCount = Convert.ToInt32(sqlHelper.ExecuteScalar());

            sqlHelper.CommandText = "SELECT count(*) from table_site";
            var siteCount = Convert.ToInt32(sqlHelper.ExecuteScalar());

            sqlHelper.CommandText = "SELECT count(*) from table_contact";
            var contactCount = Convert.ToInt32(sqlHelper.ExecuteScalar());

            sqlHelper.CommandText = "SELECT count(*) from table_probdesc";
            var solutionCount = Convert.ToInt32(sqlHelper.ExecuteScalar());

            return new HomeModel
                       {
                           CaseCount = caseCount,
                           ContactCount = contactCount,
                           SiteCount = siteCount,
                           SolutionCount = solutionCount,
                           SubcaseCount = subcaseCount
                       };
        }
    }
    
    public class HomeRequest
    {
    }

    public class HomeModel 
    {
        public int CaseCount { get; set; }
        public int SubcaseCount { get; set; }
        public int SiteCount { get; set; }
        public int ContactCount { get; set; }
        public int SolutionCount { get; set; }
    }
}