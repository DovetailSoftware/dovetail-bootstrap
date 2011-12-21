using System;

namespace Dovetail.SDK.ModelMap.Registration.DSL
{
    public interface IMapExpressionPostBasedOnField<MODEL>
    {
        IMapExpressionPostRoot<MODEL> Do(Func<string, object> mapFieldValueToObject);
    }
}