using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.ModelMap.NewStuff.Serialization;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public class ModelMapCache : IModelMapCache
    {
        private Lazy<ModelMap[]> _maps;
        private readonly IModelMapParser _parser;
        private readonly ModelMapSettings _settings;

        public ModelMapCache(Lazy<ModelMap[]> maps, IModelMapParser parser, ModelMapSettings settings)
        {
            _maps = maps;
            _parser = parser;
            _settings = settings;

            Clear();
        }

        public IEnumerable<ModelMap> Maps()
        {
            if (!_settings.EnableCache)
                Clear();

            return _maps.Value;
        }

        public void Clear()
        {
            _maps = new Lazy<ModelMap[]>(findMaps);
        }

        private ModelMap[] findMaps()
        {
            var files = new FileSystem();
            var mapFiles = files.FindFiles(_settings.Directory, new FileSet
            {
                Include = "*.map.config",
                DeepSearch = true
            });

            var maps = mapFiles
                .Select(_ => _parser.Parse(_))
                .ToArray();

            var conflicts = maps.GroupBy(_ => _.Name).Where(_ => _.Count() > 1).ToArray();
            if (conflicts.Any())
                throw new ModelMapException("Multiple models found with the same name: " + conflicts.Select(_ => _.Key).Join(", "));


            return maps;
        }
    }
}