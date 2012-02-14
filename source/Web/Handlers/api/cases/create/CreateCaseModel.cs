using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Fubu.Clarify.Lists;

namespace Bootstrap.Web.Handlers.api.cases.create
{
    [Description("Basic create case. Specifying a queue will automatically dispatch the case to that queue.")]
    public class CreateCaseModel : IApi
    {
        public bool UserIsAuthenticated { get; set; }
        [Required, Description("Case contact's first name.")]
        public string ContactFirstName { get; set; }
        [Required, Description("Case contact's last name.")]
        public string ContactLastName { get; set; }
        [Required, Description("Case contact's phone number.")]
        public string ContactPhone { get; set; }
        [Required, Description("Site Id the case is being reported against. The contact needs to have a contact_role for this site.")]
        public string SiteId { get; set; }
        
        [Description("Clarify queue to which the case should be dispatched being reported against. When this is left blank the case will not be dispatched.")]
        public string Queue { get; set; }

        [Description("Title of the case.")]
        public string Title { get; set; }

        [Description("Detailed description of the case. This field will be logged as a phone note against the case.")]
        public string Description { get; set; }

        [GbstListValue(ListNames.CaseType), Description("The type of case. This should be a selection from the 'Case Type' application list.")]
        public string CaseType { get; set; }

        [GbstListValue(ListNames.CaseSeverity), Description("Severity of the case. This should be a selection from the 'Problem Severity Level' application list.")]
        public string IssueSeverity { get; set; }

        [GbstListValue(ListNames.CasePriority), Description("Priority of the case. This should be a selection from the 'Response Priority Code' application list.")]
        public string Priority { get; set; }
    }
}