using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.History.Instructions;
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
		private readonly HistorySettings _settings;
		private readonly IHistoryMapOverrideParser _overrides;
		private static readonly object Lock = new object();

		public HistoryMapCache(IHistoryMapParser parser, HistorySettings settings, IHistoryMapOverrideParser overrides)
		{
			_parser = parser;
			_settings = settings;
			_overrides = overrides;

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
					map.As<IExpandableMap>().Expand(this);

				foreach (var map in _maps.Value)
					map.As<IExpandableMap>().Expand(this);

				foreach (var map in _maps.Value)
					removeDuplicateActEntries(map);
			}
			finally
			{
				_visiting = false;
			}
		}

		private void removeDuplicateActEntries(ModelMap.ModelMap map)
		{
			var locations = new List<Tuple<int, int>>();
			var instructions = map.Instructions;
			for (var i = 0; i < instructions.Length; ++i)
			{
				var entry = instructions[i] as BeginActEntry;
				if (entry != null)
				{
					locations.Add(new Tuple<int, int>(entry.Code, i));
				}
			}

			var visited = new List<int>();
			foreach (var pair in locations)
			{
				if (!visited.Contains(pair.Item1))
				{
					visited.Add(pair.Item1);
					continue;
				}

				var set = map.FindInstructionSet(pair.Item2 - 1, _ =>
				{
					var entry = _ as BeginActEntry;
					return entry != null && entry.Code == pair.Item1;
				}, _ => _ is EndActEntry);

				foreach(var instruction in set.Instructions)
					map.RemoveInstruction(instruction);
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
					.Where(_ => !_overrides.ShouldParse(_))// && !_replacements.ShouldParse(_))
					.Select(_ => _parser.Parse(_))
					.OrderBy(_ => _.Name)
					.ToArray();

				var conflicts = maps.GroupBy(_ => _.Name).Where(_ => _.Count() > 1).ToArray();
				if (conflicts.Any())
					throw new ModelMapException("Multiple history models found with the same name: " +
					                            conflicts.Select(_ => _.Key).Join(", "));

				mapFiles
					.Where(_ => _overrides.ShouldParse(_))
					.Each(_ => maps
						.Where(__ => _overrides.Matches(__, _))
						.Each(__ => _overrides.Parse(__, _)));

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
