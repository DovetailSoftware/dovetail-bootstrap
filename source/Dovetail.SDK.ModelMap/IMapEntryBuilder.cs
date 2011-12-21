using Dovetail.SDK.ModelMap.ObjectModel;
using Dovetail.SDK.ModelMap.Registration;

namespace Dovetail.SDK.ModelMap
{
    public interface IMapEntryBuilder
    {
        ClarifyGenericMapEntry BuildFromModelMap<MODEL>(ModelMap<MODEL> modelMap);
    }
}