using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.History
{
	public class CaseHistoryAssemblyPolicy : IHistoryAssemblyPolicy
	{
		private readonly IHistoryMapRegistry _models;
		private readonly ISchemaCache _schema;
		private readonly HistorySettings _settings;

		public CaseHistoryAssemblyPolicy(IHistoryMapRegistry models, ISchemaCache schema, HistorySettings settings)
		{
			_models = models;
			_schema = schema;
			_settings = settings;
		}

		public bool Matches(HistoryRequest request)
		{
			return request.WorkflowObject.Type.EqualsIgnoreCase("case")
			       && _settings.MergeCaseHistoryChildSubcases;
		}

		public HistoryResult HistoryFor(HistoryRequest request, IHistoryBuilder builder)
		{
			throw new System.NotImplementedException();
		}
	}
}