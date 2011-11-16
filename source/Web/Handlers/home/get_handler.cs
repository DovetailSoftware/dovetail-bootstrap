using System;
using Dovetail.SDK.Bootstrap.Clarify;

namespace Bootstrap.Web.Handlers.home
{
    public class get_handler
    {
        private readonly IClarifySession _session;

        public get_handler(IClarifySession session)
        {
            _session = session;
        }

        public HomeModel Execute(HomeRequest request)
        {
            var sqlHelper = _session.CreateSqlHelper("SELECT count(*) from table_case");
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