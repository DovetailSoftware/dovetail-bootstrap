using System;

namespace Dovetail.SDK.ModelMap.Registration.DSL
{
    public interface IMapRelatedModelExpressionPostTo<PARENTMODEL, CHILDMODEL>
    {
        MapExpression<PARENTMODEL> ViaRelation(string relationName, Action<IMapExpressionPostRoot<CHILDMODEL>> mapAction);
    }
}