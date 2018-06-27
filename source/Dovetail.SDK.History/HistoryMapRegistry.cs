using System.Linq;
using FubuCore;

namespace Dovetail.SDK.History
{
	public class HistoryMapRegistry : IHistoryMapRegistry
	{
		private readonly IHistoryMapCache _cache;

		public HistoryMapRegistry(IHistoryMapCache cache)
		{
			_cache = cache;
		}

		public ModelMap.ModelMap Find(WorkflowObject workflowObject)
		{
			return _cache.Maps().SingleOrDefault(_ => _.Name.EqualsIgnoreCase(workflowObject.Key));
		}

		public ModelMap.ModelMap FindPartial(string name)
		{
			return _cache.Partials().SingleOrDefault(_ => _.Name.EqualsIgnoreCase(name));
		}
	}
}