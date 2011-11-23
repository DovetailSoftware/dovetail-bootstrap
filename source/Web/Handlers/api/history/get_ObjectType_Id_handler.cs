using System;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.History;

namespace Bootstrap.Web.Handlers.api.history
{
	public class get_ObjectType_Id_handler
	{
        private readonly IHistoryBuilder _historyBuilder;

        public get_ObjectType_Id_handler(IHistoryBuilder historyBuilder) 
        {
            _historyBuilder = historyBuilder;
        }

        public HistoryViewModel Execute(HistoryRequest request)
        {
			var workflowObject = WorkflowObject.Create(request.ObjectType, request.Id); 
			
			if (request.Since.HasValue)
            {
            	return _historyBuilder.GetHistorySince(workflowObject, request.Since.Value);
            }

        	return _historyBuilder.GetHistory(workflowObject);
        }
    }

	public class HistoryRequest : IUnauthenticatedApi 
    {
        public string ObjectType { get; set; }
		public string Id { get; set; }
		public DateTime? Since { get; set; }
		public string Timezone { get; set; }
    }
}