using System.Collections.Generic;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public interface IMappingVariableSource
    {
        IEnumerable<IMappingVariable> Variables();
    }
}