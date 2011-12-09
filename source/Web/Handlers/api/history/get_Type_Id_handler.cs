using System;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.History;

namespace Bootstrap.Web.Handlers.api.history
{
	public class get_Type_Id_handler
	{
        private readonly IHistoryAssembler _historyAssembler;

        public get_Type_Id_handler(IHistoryAssembler historyAssembler) 
        {
            _historyAssembler = historyAssembler;
        }

        public HistoryViewModel Execute(HistoryRequest request)
        {
			var workflowObject = WorkflowObject.Create(request.Type, request.Id); 
			
			if (request.Since.HasValue)
            {
            	return _historyAssembler.GetHistorySince(workflowObject, request.Since.Value);
            }

        	return _historyAssembler.GetHistory(workflowObject);
        }
    }

	public class HistoryRequest : IApi 
    {
        public string Type { get; set; }
		public string Id { get; set; }
		public DateTime? Since { get; set; }
		public string Timezone { get; set; }
    }
}