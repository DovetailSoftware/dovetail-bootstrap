using System.Collections.Generic;
using System.Linq;

namespace Dovetail.SDK.ModelMap
{
    public class MappingVariableRegistry : IMappingVariableRegistry
    {
        private readonly IEnumerable<IMappingVariableSource> _sources;

        public MappingVariableRegistry(IEnumerable<IMappingVariableSource> sources)
        {
            _sources = sources;
        }

        public IMappingVariable Find(VariableExpansionContext context)
        {
            return _sources
                .SelectMany(_ => _.Variables())
                .LastOrDefault(_ => _.Matches(context));
        }
    }
}