using Dovetail.SDK.ModelMap.Legacy.ObjectModel;
using Dovetail.SDK.ModelMap.Legacy.Registration;

namespace Dovetail.SDK.ModelMap.Legacy
{
    public interface IMapEntryBuilder
    {
        ClarifyGenericMapEntry BuildFromModelMap<MODEL>(ModelMap<MODEL> modelMap);
    }
}