using Dovetail.SDK.ModelMap.NewStuff.ObjectModel;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public interface IMapEntryBuilder
    {
        ClarifyGenericMapEntry BuildFromModelMap(ModelMap modelMap);
    }
}