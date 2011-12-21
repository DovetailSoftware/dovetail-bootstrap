using FChoice.Foundation.Filters;

namespace Dovetail.SDK.ModelMap.Instructions
{
	public class AddFilter
	{
		public Filter Filter { get; private set; }

		public AddFilter(Filter filter)
		{
			Filter = filter;
		}
	}
}