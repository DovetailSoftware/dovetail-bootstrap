using System;

namespace Dovetail.SDK.ModelMap.Legacy.Registration.DSL
{
	public interface IMapExpressionPostRoot<MODEL> : IMapExpressionPostView<MODEL>
	{
		IMapExpressionPostRoot<MODEL> ViaRelation(string relationName, Action<IMapExpressionPostRoot<MODEL>> mapAction);
		IMapRelatedModelExpression<MODEL, CHILDMODEL> MapMany<CHILDMODEL>();
		IMapRelatedModelExpression<MODEL, CHILDMODEL> MapOne<CHILDMODEL>();
	}
}