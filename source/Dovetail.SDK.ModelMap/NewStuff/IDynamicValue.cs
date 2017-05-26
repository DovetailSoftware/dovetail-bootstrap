using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff
{
	public interface IDynamicValue
	{
		object Resolve(IServiceLocator services);
	}
}