using System;
using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.ModelMap;

namespace Bootstrap.Web.Handlers.home
{
    public class get_handler
    {
        private readonly IClarifySession _userSession;
        private readonly IModelBuilder<UserOpenCaseListing> _listingBuilder;

        public get_handler(IClarifySession userSession, IModelBuilder<UserOpenCaseListing> listingBuilder)
        {
            _userSession = userSession;
            _listingBuilder = listingBuilder;
        }

        public HomeModel Execute(HomeRequest request)
        {
            var userCases = _listingBuilder.GetAll();

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

            return new HomeModel
                       {
                           Cases = userCases,
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
        public IEnumerable<UserOpenCaseListing> Cases { get; set; }
        public int CaseCount { get; set; }
        public int SubcaseCount { get; set; }
        public int SiteCount { get; set; }
        public int ContactCount { get; set; }
        public int SolutionCount { get; set; }
    }
}