using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.History;
using Dovetail.SDK.ModelMap;
using FubuMVC.Swagger.Specification;

namespace Bootstrap.Web.Handlers.api.history
{
	public class get_Type_Id_handler
	{
		private readonly IHistoryAssembler _historyAssembler;
		private readonly ISystemTime _systemTime;

		public get_Type_Id_handler(IHistoryAssembler historyAssembler, ISystemTime systemTime)
		{
			_historyAssembler = historyAssembler;
			_systemTime = systemTime;
		}

		public HistoryViewModel Execute(GETHistoryRequest request)
		{
			var workflowObject = WorkflowObject.Create(request.Type, request.Id);

			var historyRequest = new HistoryRequest {WorkflowObject = workflowObject, ShowAllActivities = request.IsVerbose};

			if (request.DaysOfHistory > 0)
			{
				var since = _systemTime.Now.Subtract(TimeSpan.FromDays(request.DaysOfHistory));
				historyRequest.Since = since;
			}

			return _historyAssembler.GetHistory(historyRequest);
		}
	}

	[Description("Workflow object history")]
	public class GETHistoryRequest : IApi
	{
		[Required, Description("Type of workflow object. Typically this is 'case'.")]
		[AllowableValues("case", "subcase", "solution", "<any workflow object name>")]
		public string Type { get; set; }

		[Required, Description("Id of the workflow object.")]
		public string Id { get; set; }

		[Description("Limit the amout of history returned the given number of days. When this parameter is not specified. All history items will be returned.")]
		public int DaysOfHistory { get; set; }

		public bool IsVerbose { get; set; }
	}
}