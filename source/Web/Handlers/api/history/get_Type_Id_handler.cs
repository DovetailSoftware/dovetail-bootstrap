using System;
using System.ComponentModel.DataAnnotations;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.History;
using Dovetail.SDK.ModelMap;

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

	    public HistoryViewModel Execute(HistoryRequest request)
        {
			var workflowObject = WorkflowObject.Create(request.Type, request.Id); 
			
			if (request.DaysOfHistory> 0)
			{
			    var since = _systemTime.Now.Subtract(TimeSpan.FromDays(request.DaysOfHistory));
            	return _historyAssembler.GetHistorySince(workflowObject, since);
            }

        	return _historyAssembler.GetHistory(workflowObject);
        }
    }

	public class HistoryRequest : IApi 
    {
        [Required]
        public string Type { get; set; }
        [Required]
		public string Id { get; set; }

		public int DaysOfHistory { get; set; }
    }
}