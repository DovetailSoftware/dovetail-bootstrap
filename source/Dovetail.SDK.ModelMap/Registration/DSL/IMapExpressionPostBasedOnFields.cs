using System;

namespace Dovetail.SDK.ModelMap.Registration.DSL
{
    public interface IMapExpressionPostBasedOnFields<MODEL>
    {
        IMapExpressionPostRoot<MODEL> Do(Func<string[], object> mapFieldValuesToObject);
    }
}