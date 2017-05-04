using System;

namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public interface IMappingTransformRegistry
	{
		bool HasPolicy(string name);
		Type FindPolicy(string name);
	}
}