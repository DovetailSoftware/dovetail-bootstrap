using System.IO;
using System.Xml.Linq;
using Dovetail.SDK.ModelMap.Serialization.Overrides;
using FubuCore;

namespace Dovetail.SDK.History.Serialization
{
	public class HistoryMapOverrideParser : IHistoryMapOverrideParser
	{
		private readonly IHistoryMapParser _inner;
		private readonly IModelMapDiff _diff;
		private readonly HistoryMapDiffOptions _options;

		public HistoryMapOverrideParser(IHistoryMapParser inner, IModelMapDiff diff, HistoryMapDiffOptions options)
		{
			_inner = inner;
			_diff = diff;
			_options = options;
		}

		public bool ShouldParse(string filePath)
		{
			var doc = openFile(filePath);
			return doc.Root.Attribute("overrides") != null;
		}

		public bool Matches(ModelMap.ModelMap map, string filePath)
		{
			var doc = openFile(filePath);
			var overrides = doc.Root.Attribute("overrides");
			if (overrides == null) return false;

			return map.Name.EqualsIgnoreCase(WorkflowObject.KeyFor(overrides.Value));
		}

		public void Parse(ModelMap.ModelMap map, string filePath)
		{
			var overrides = new ModelMap.ModelMap(map.Name, map.Entity);
			_inner.Parse(overrides, filePath);
			_diff.Diff(map, overrides, _options);
		}

		private XDocument openFile(string filePath)
		{
			using (var reader = new StreamReader(filePath))
			{
				var doc = XDocument.Load(reader);
				return doc;
			}
		}
	}
}