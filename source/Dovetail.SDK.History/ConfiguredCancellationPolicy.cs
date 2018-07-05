using System.Collections.Generic;
using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Transforms;
using FubuCore;

namespace Dovetail.SDK.History
{
	public class ConfiguredCancellationPolicy : ConfiguredTransform
	{
		private readonly int _actCode;

		public ConfiguredCancellationPolicy(int actCode, ModelDataPath path, IMappingTransform transform, IEnumerable<ITransformArgument> arguments, IMappingVariableExpander expander, IServiceLocator services) 
			: base(path, transform, arguments, expander, services)
		{
			_actCode = actCode;
		}

		public override object Execute(ModelData data, IServiceLocator services)
		{
			var actCode = data.Get<int>("type");
			return actCode == _actCode && (bool) base.Execute(data, services);
		}
	}
}