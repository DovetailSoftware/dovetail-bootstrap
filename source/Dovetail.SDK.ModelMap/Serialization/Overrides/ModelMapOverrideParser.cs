﻿using System.IO;
using System.Xml.Linq;
using FubuCore;

namespace Dovetail.SDK.ModelMap.Serialization.Overrides
{
	public class ModelMapOverrideParser : IModelMapOverrideParser
	{
		private readonly IModelMapParser _inner;
		private readonly IModelMapDiff _diff;
		private readonly ModelMapDiffOptions _options;

		public ModelMapOverrideParser(IModelMapParser inner, IModelMapDiff diff, ModelMapDiffOptions options)
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

		public bool Matches(ModelMap map, string filePath)
		{
			var doc = openFile(filePath);
			var overrides = doc.Root.Attribute("overrides");
			if (overrides == null) return false;

			return map.Name.EqualsIgnoreCase(overrides.Value);
		}

		public void Parse(ModelMap map, string filePath)
		{
			var overrides = new ModelMap(map.Name, map.Entity);
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