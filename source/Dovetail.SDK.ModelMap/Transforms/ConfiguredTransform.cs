using System.Collections.Generic;
using System.Linq;
using FubuCore;

namespace Dovetail.SDK.ModelMap.Transforms
{
	public class ConfiguredTransform
	{
		private readonly ModelDataPath _path;
		private readonly IMappingTransform _transform;
		private readonly IEnumerable<ITransformArgument> _arguments;
		private readonly IMappingVariableExpander _expander;
		private readonly IServiceLocator _services;

		public ConfiguredTransform(ModelDataPath path, IMappingTransform transform, IEnumerable<ITransformArgument> arguments, IMappingVariableExpander expander, IServiceLocator services)
		{
			_path = path;
			_transform = transform;
			_arguments = arguments;
			_expander = expander;
			_services = services;
		}

		public void Execute(ModelData data, IServiceLocator services)
		{
			_expander.PushContext(new VariableExpanderContext(data, new Dictionary<string, object>()));
			
			try
			{
				var arguments = new TransformArguments(_services, _arguments.ToDictionary(_ => _.Name, _ => _.Resolve(data)));
				var context = new TransformContext(data, arguments, services);
				var value = _transform.Execute(context);

				if (value is IDynamicValue)
				{
					value = value.As<IDynamicValue>().Resolve(_services);
				}

				if (value != null)
					_path.Set(data, value);
			}
			finally
			{
				_expander.PopContext();
			}
		}
	}
}