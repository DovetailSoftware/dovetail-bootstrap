using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dovetail.SDK.ModelMap.Registration.DSL
{
    public interface IMapRelatedModelExpression<PARENTMODEL, CHILDMODEL>
    {
        IMapRelatedModelExpressionPostTo<PARENTMODEL, CHILDMODEL> To(Expression<Func<PARENTMODEL, IEnumerable<CHILDMODEL>>> expression);
        IMapRelatedModelExpressionPostTo<PARENTMODEL, CHILDMODEL> To(Expression<Func<PARENTMODEL, CHILDMODEL>> expression);
    }
}