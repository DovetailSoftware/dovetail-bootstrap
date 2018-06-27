using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.History.Serialization;
using Dovetail.SDK.ModelMap;
using FubuCore;

namespace Dovetail.SDK.History
{
	public class HistoryMapCache : IHistoryMapCache, IModelMapCache
	{
		private Lazy<ModelMap.ModelMap[]> _maps;
		private Lazy<ModelMap.ModelMap[]> _partials;
		private bool _visiting;
		private readonly IHistoryMapParser _parser;
		private readonly HistoryMapSettings _settings;
		private static readonly object Lock = new object();

		public HistoryMapCache(IHistoryMapParser parser, HistoryMapSettings settings)
		{
			_parser = parser;
			_settings = settings;

			Clear();
		}

		public IEnumerable<ModelMap.ModelMap> Maps()
		{
			return _maps.Value;
		}

		public IEnumerable<ModelMap.ModelMap> Partials()
		{
			return _partials.Value;
		}

		public void Clear()
		{
			if (_visiting) return;

			try
			{
				_visiting = true;

				_maps = new Lazy<ModelMap.ModelMap[]>(() => findMaps("*.history.config"));
				_partials = new Lazy<ModelMap.ModelMap[]>(() => findMaps("*.partial.config"));

				foreach (var map in _partials.Value)
				{
					map.As<IExpandableMap>().Expand(this);
				}

				foreach (var map in _maps.Value)
				{
					map.As<IExpandableMap>().Expand(this);
				}
			}
			finally
			{
				_visiting = false;
			}
		}

		private ModelMap.ModelMap[] findMaps(string include)
		{
			lock (Lock)
			{
				var files = new FileSystem();
				var mapFiles = files.FindFiles(_settings.Directory, new FileSet
				{
					Include = include,
					DeepSearch = true
				}).ToArray();

				var maps = mapFiles
					//.Where(_ => !_overrides.ShouldParse(_) && !_replacements.ShouldParse(_))
					.Select(_ => _parser.Parse(_))
					.ToArray();

				var conflicts = maps.GroupBy(_ => _.Name).Where(_ => _.Count() > 1).ToArray();
				if (conflicts.Any())
					throw new ModelMapException("Multiple history models found with the same name: " +
					                            conflicts.Select(_ => _.Key).Join(", "));

				//mapFiles
				//	.Where(_ => _overrides.ShouldParse(_))
				//	.Each(_ => maps
				//		.Where(__ => _overrides.Matches(__, _))
				//		.Each(__ => _overrides.Parse(__, _)));

				//mapFiles
				//	.Where(_ => _replacements.ShouldParse(_))
				//	.Each(_ => maps
				//		.Where(__ => _replacements.Matches(__, _))
				//		.Each(__ => _replacements.Parse(__, _)));

				return maps;
			}
		}
	}
}