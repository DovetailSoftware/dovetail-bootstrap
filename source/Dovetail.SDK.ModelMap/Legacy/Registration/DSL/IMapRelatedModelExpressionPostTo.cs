using System;

namespace Dovetail.SDK.ModelMap.Legacy.Registration.DSL
{
    public interface IMapRelatedModelExpressionPostTo<PARENTMODEL, CHILDMODEL>
    {
        MapExpression<PARENTMODEL> ViaRelation(string relationName, Action<IMapExpressionPostRoot<CHILDMODEL>> mapAction);
    }
}