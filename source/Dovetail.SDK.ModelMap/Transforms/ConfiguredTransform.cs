using System;
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
		private readonly Func<ModelData, bool> _condition;

		public ConfiguredTransform(ModelDataPath path, IMappingTransform transform, IEnumerable<ITransformArgument> arguments, IMappingVariableExpander expander, IServiceLocator services, Func<ModelData, bool> condition = null)
		{
			_path = path;
			_transform = transform;
			_arguments = arguments;
			_expander = expander;
			_services = services;
			_condition = condition;
		}

		public bool ShouldExecute(ModelData data)
		{
			return _condition == null || _condition(data);
		}

		public virtual object Execute(ModelData data, IServiceLocator services)
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

				return value;
			}
			finally
			{
				_expander.PopContext();
			}
		}
	}
}
