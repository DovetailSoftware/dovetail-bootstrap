using System.IO;
using System.Xml.Linq;
using FubuCore;

namespace Dovetail.SDK.ModelMap.Serialization.Overrides
{
	public class ModelMapReplacementParser : IModelMapReplacementParser
	{
		private readonly IModelMapParser _inner;

		public ModelMapReplacementParser(IModelMapParser inner)
		{
			_inner = inner;
		}

		public bool ShouldParse(string filePath)
		{
			var doc = openFile(filePath);
			return doc.Root.Attribute("replaces") != null;
		}

		public bool Matches(ModelMap map, string filePath)
		{
			var doc = openFile(filePath);
			var replaces = doc.Root.Attribute("replaces");
			if (replaces == null) return false;

			return map.Name.EqualsIgnoreCase(replaces.Value);
		}

		public void Parse(ModelMap map, string filePath)
		{
			var replacement = new ModelMap(map.Name, map.Entity);
			_inner.Parse(replacement, filePath);
			map.ReplaceWith(replacement);
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