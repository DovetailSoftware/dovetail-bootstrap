using System.ComponentModel.DataAnnotations;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Fubu.Clarify.Lists;

namespace Bootstrap.Web.Handlers.api.cases.create
{
    public class CreateCaseModel : IApi
    {
        public bool UserIsAuthenticated { get; set; }
        [Required]
        public string ContactFirstName { get; set; }
        [Required]
        public string ContactLastName { get; set; }
        [Required]
        public string ContactPhone { get; set; }
        [Required]
        public string SiteId { get; set; }
        
        public string Queue { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        [GbstListValue(ListNames.CaseType)]
        public string CaseType { get; set; }

        [GbstListValue(ListNames.CaseSeverity)]
        public string IssueSeverity { get; set; }

        [GbstListValue(ListNames.CasePriority)]
        public string Priority { get; set; }
    }
}