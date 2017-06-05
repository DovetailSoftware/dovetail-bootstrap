using System;
using System.Linq.Expressions;
using Dovetail.SDK.ModelMap.Clarify;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.ModelMap.Legacy.Registration.DSL
{
    public interface IMapExpressionPostView<MODEL>
    {
		IMapExpressionPostRoot<MODEL> FilteredBy(Func<FilterExpression, Filter> filterAction);
		IMapExpressionPostRoot<MODEL> FilteredBy(Filter filter);
        IMapExpressionPostAssign<MODEL> Assign(Expression<Func<MODEL, object>> expression);
        IMapExpressionPostAssignWithList<MODEL> Assign(Expression<Func<MODEL, IClarifyList>> expression);
        IMapExpressionPostRoot<MODEL> SortDescendingBy(string fieldName);
        IMapExpressionPostRoot<MODEL> SortAscendingBy(string fieldName);
        IMapExpressionPostRoot<MODEL> ViaAdhocRelation(string fromFieldName, string toTableName, string toFieldName, Action<IMapExpressionPostRoot<MODEL>> mapAction);
    }
}