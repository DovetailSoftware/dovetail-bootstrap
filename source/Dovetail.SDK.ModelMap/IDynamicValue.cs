using FubuCore;

namespace Dovetail.SDK.ModelMap
{
	public interface IDynamicValue
	{
		object Resolve(IServiceLocator services);
	}
}