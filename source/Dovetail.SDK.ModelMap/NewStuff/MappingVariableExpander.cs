using System.Text.RegularExpressions;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public class MappingVariableExpander : IMappingVariableExpander
    {
        private static readonly Regex VariableRegex = new Regex(@"\$\{(?<Variable>[\w\.\-]*)\}", RegexOptions.Compiled);

        private readonly IMappingVariableRegistry _registry;
        private readonly IServiceLocator _services;

        public MappingVariableExpander(IMappingVariableRegistry registry, IServiceLocator services)
        {
            _registry = registry;
            _services = services;
        }

        public bool IsVariable(string value)
        {
            return VariableRegex.IsMatch(value);
        }

        public object Expand(string value)
        {
            var match = VariableRegex.Match(value);
            var key = match.Groups["Variable"].Value;

            var variable = _registry.Find(key);
            if (variable == null)
                return value;

            return variable.Expand(key, _services);
        }
    }
}