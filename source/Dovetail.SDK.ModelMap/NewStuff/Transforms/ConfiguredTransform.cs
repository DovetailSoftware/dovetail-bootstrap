using System.Collections.Generic;
using System.Linq;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public class ConfiguredTransform
	{
		private readonly ModelDataPath _path;
		private readonly IMappingTransform _transform;
		private readonly IEnumerable<ITransformArgument> _arguments;

		public ConfiguredTransform(ModelDataPath path, IMappingTransform transform, IEnumerable<ITransformArgument> arguments)
		{
			_path = path;
			_transform = transform;
			_arguments = arguments;
		}

		public void Execute(ModelData data, IServiceLocator services)
		{
			var arguments = new TransformArguments(_arguments.ToDictionary(_ => _.Name, _ => _.Resolve(data)));
			var context = new TransformContext(data, arguments, services);
			var value = _transform.Execute(context);

			_path.Set(data, value);
		}
	}
}