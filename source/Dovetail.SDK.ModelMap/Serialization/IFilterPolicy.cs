using FChoice.Foundation.Filters;

namespace Dovetail.SDK.ModelMap.Serialization
{
    public interface IFilterPolicy
    {
        Filter CreateFilter();
    }
}