using System;

namespace Dovetail.SDK.ModelMap.Transforms
{
	public interface IMappingTransformRegistry
	{
		bool HasPolicy(string name);
		Type FindPolicy(string name);
	}
}