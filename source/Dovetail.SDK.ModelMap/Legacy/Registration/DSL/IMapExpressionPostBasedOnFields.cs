using System;

namespace Dovetail.SDK.ModelMap.Legacy.Registration.DSL
{
    public interface IMapExpressionPostBasedOnFields<MODEL>
    {
        IMapExpressionPostRoot<MODEL> Do(Func<string[], object> mapFieldValuesToObject);
    }
}