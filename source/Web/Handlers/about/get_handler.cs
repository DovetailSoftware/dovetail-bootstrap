using System;
using System.Reflection;
using Dovetail.SDK.Bootstrap.Clarify;

namespace Bootstrap.Web.Handlers.about
{
    public class get_handler
    {
        private readonly IClarifySession _userSession;

        public get_handler(IClarifySession userSession)
        {
            _userSession = userSession;
        }

        public AboutModel Execute(AboutRequest request)
        {
            var sqlHelper = _userSession.CreateSqlHelper("SELECT count(*) from table_case");
            var caseCount = Convert.ToInt32(sqlHelper.ExecuteScalar());

            sqlHelper.CommandText = "SELECT count(*) from table_subcase";
            var subcaseCount = Convert.ToInt32(sqlHelper.ExecuteScalar());

            sqlHelper.CommandText = "SELECT count(*) from table_site";
            var siteCount = Convert.ToInt32(sqlHelper.ExecuteScalar());

            sqlHelper.CommandText = "SELECT count(*) from table_contact";
            var contactCount = Convert.ToInt32(sqlHelper.ExecuteScalar());

            sqlHelper.CommandText = "SELECT count(*) from table_probdesc";
            var solutionCount = Convert.ToInt32(sqlHelper.ExecuteScalar());

            return new AboutModel
                       {
                           Version = Assembly.GetCallingAssembly().GetName().Version.ToString(),
                           CaseCount = caseCount,
                           ContactCount = contactCount,
                           SiteCount = siteCount,
                           SolutionCount = solutionCount,
                           SubcaseCount = subcaseCount
                       };
        }
    }
    
    public class AboutRequest
    {
    }

    public class AboutModel 
    {
        public string Version { get; set; }
    
        public int CaseCount { get; set; }
        public int SubcaseCount { get; set; }
        public int SiteCount { get; set; }
        public int ContactCount { get; set; }
        public int SolutionCount { get; set; }
    }
}