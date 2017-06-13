using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.ModelMap.Serialization;
using Dovetail.SDK.ModelMap.Serialization.Overrides;
using FubuCore;

namespace Dovetail.SDK.ModelMap
{
    public class ModelMapCache : IModelMapCache
    {
        private Lazy<ModelMap[]> _maps;
		private Lazy<ModelMap[]> _partials;
	    private bool _visiting;
		private readonly IModelMapParser _parser;
	    private readonly IModelMapOverrideParser _overrides;
        private readonly ModelMapSettings _settings;
		private static readonly object Lock = new object();

        public ModelMapCache(IModelMapParser parser, IModelMapOverrideParser overrides, ModelMapSettings settings)
        {
            _parser = parser;
	        _overrides = overrides;
	        _settings = settings;

	        Clear();
        }

        public IEnumerable<ModelMap> Maps()
        {
            if (!_settings.EnableCache)
                Clear();

			return _maps.Value;
        }

		public IEnumerable<ModelMap> Partials()
		{
			if (!_settings.EnableCache)
				Clear();

			return _partials.Value;
		}

		public void Clear()
        {
			if (_visiting) return;

			_visiting = true;

			_maps = new Lazy<ModelMap[]>(() => findMaps("*.map.config"));
			_partials = new Lazy<ModelMap[]>(() => findMaps("*.partial.config"));

			foreach (var map in _partials.Value)
			{
				map.As<IExpandableMap>().Expand(this);
			}

			foreach (var map in _maps.Value)
			{
				map.As<IExpandableMap>().Expand(this);
			}

			_visiting = false;
        }

        private ModelMap[] findMaps(string include)
        {
	        lock (Lock)
	        {
		        var files = new FileSystem();
		        var mapFiles = files.FindFiles(_settings.Directory, new FileSet
		        {
			        Include = include,
			        DeepSearch = true
		        });

		        var maps = mapFiles
			        .Where(_ => !_overrides.ShouldParse(_))
			        .Select(_ => _parser.Parse(_))
			        .ToArray();

		        var conflicts = maps.GroupBy(_ => _.Name).Where(_ => _.Count() > 1).ToArray();
		        if (conflicts.Any())
			        throw new ModelMapException("Multiple models found with the same name: " +
			                                    conflicts.Select(_ => _.Key).Join(", "));

		        mapFiles
			        .Where(_ => _overrides.ShouldParse(_))
			        .Each(_ => maps
				        .Where(__ => _overrides.Matches(__, _))
				        .Each(__ => _overrides.Parse(__, _)));

		        return maps;
	        }
        }
    }
}