using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public class MappingVariableExpander : IMappingVariableExpander
    {
        private static readonly Regex VariableRegex = new Regex(@"\$\{(?<Variable>[\w\.\-]*)\}", RegexOptions.Compiled);

        private readonly IMappingVariableRegistry _registry;
        private readonly IServiceLocator _services;
	    private readonly Stack<VariableExpanderContext> _contexts;

        public MappingVariableExpander(IMappingVariableRegistry registry, IServiceLocator services)
        {
            _registry = registry;
            _services = services;
			_contexts = new Stack<VariableExpanderContext>();
        }

        public bool IsVariable(string value)
        {
            return VariableRegex.IsMatch(value);
        }

        public object Expand(string value)
        {
            var match = VariableRegex.Match(value);
            var key = match.Groups["Variable"].Value;
	        ModelData data = null;

	        if (_contexts.Any())
	        {
		        var expansionContext = _contexts.Peek();
		        data = expansionContext.Data;

				if (expansionContext.Has(key))
					return expansionContext.Get(key);
	        }

			var context = new VariableExpansionContext(_services, key, data);
			var variable = _registry.Find(context);
            if (variable == null)
                return value;

            return variable.Expand(context);
        }

	    public void PushContext(VariableExpanderContext context)
	    {
		    _contexts.Push(context);
	    }

	    public void PopContext()
	    {
		    _contexts.Pop();
	    }
    }
}