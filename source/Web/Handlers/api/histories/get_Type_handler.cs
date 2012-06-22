using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.History;
using FubuMVC.Swagger.Specification;

namespace Bootstrap.Web.Handlers.api.histories
{
	public class get_Type_handler
	{
        private readonly IHistoryAssembler _historyAssembler;

	    public get_Type_handler(IHistoryAssembler historyAssembler)
        {
            _historyAssembler = historyAssembler;
        }

		public HistoriesModel Execute(HistoriesRequest request)
		{
			var ids = request.Ids.Split(',');

			var items = request.Since.HasValue ? _historyAssembler.GetHistoriesSince(request.Type, ids, request.Since.Value) : _historyAssembler.GetHistories(request.Type, ids);

			return new HistoriesModel
				{
					HistoryItems = items,
					Type = request.Type,
					Ids = ids,
					Since = request.Since
				};
        }
    }

	public class HistoriesModel 
	{
		public string Type { get; set; }
		public string[] Ids { get; set; }
		public DateTime? Since { get; set; }
		public HistoryItem[] HistoryItems { get; set; }
	}
	
    [Description("Workflow objects history")]
	public class HistoriesRequest : IApi 
    {
        [Required, Description("Type of workflow object. Typically this is 'case'.")]
        [AllowableValues("case", "subcase", "solution", "<any workflow object name>")]
        public string Type { get; set; }
        [Required, Description("Ids of the workflow objects.")]
        public string Ids { get; set; }

        [Description("")]
		public DateTime? Since{ get; set; }
    }
}