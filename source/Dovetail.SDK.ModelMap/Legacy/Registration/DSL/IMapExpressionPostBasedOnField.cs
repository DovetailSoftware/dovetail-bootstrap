using System;

namespace Dovetail.SDK.ModelMap.Legacy.Registration.DSL
{
    public interface IMapExpressionPostBasedOnField<MODEL>
    {
        IMapExpressionPostRoot<MODEL> Do(Func<string, object> mapFieldValueToObject);
    }
}