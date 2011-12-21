using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.ModelMap.Registration;

namespace Bootstrap.Web.Handlers.home
{
    public class UserOpenCaseListing
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ContactName { get; set; }
        public string SiteName { get; set; }
        public string SiteId { get; set; }
        public string Severity { get; set; }
    }

    public class UserOpenCaseListingMap : ModelMap<UserOpenCaseListing>
    {
        private readonly ICurrentSDKUser _user;

        public UserOpenCaseListingMap(ICurrentSDKUser user)
        {
            _user = user;
        }

        protected override void MapDefinition()
        {
            FromTable("qry_case_view")
                .Assign(a => a.Id).FromField("id_number")
                .Assign(a => a.Title).FromField("title")
                .Assign(a => a.SiteName).FromField("site_name")
                .Assign(a=>a.ContactName).FromFields("first_name", "last_name")
                .Assign(a=>a.Severity).FromField("severity")
                .FilteredBy(f => f.And(f.Equals("owner", _user.Username), 
                                       f.NotStartsWith("condition","Closed")));
        }
    }
}