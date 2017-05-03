using FChoice.Foundation.Filters;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization
{
    public interface IFilterPolicy
    {
        Filter CreateFilter();
    }
}