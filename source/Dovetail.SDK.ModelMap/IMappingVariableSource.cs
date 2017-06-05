using System.Collections.Generic;

namespace Dovetail.SDK.ModelMap
{
    public interface IMappingVariableSource
    {
        IEnumerable<IMappingVariable> Variables();
    }
}