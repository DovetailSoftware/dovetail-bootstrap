using System.Collections.Generic;
using System.Linq;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public class MappingVariableRegistry : IMappingVariableRegistry
    {
        private readonly IEnumerable<IMappingVariableSource> _sources;

        public MappingVariableRegistry(IEnumerable<IMappingVariableSource> sources)
        {
            _sources = sources;
        }

        public IMappingVariable Find(string key)
        {
            return _sources
                .SelectMany(_ => _.Variables())
                .LastOrDefault(_ => _.Matches(key));
        }
    }
}