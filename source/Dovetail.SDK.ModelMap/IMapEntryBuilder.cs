using Dovetail.SDK.ModelMap.ObjectModel;

namespace Dovetail.SDK.ModelMap
{
    public interface IMapEntryBuilder
    {
        ClarifyGenericMapEntry BuildFromModelMap(ModelMap modelMap);
    }
}