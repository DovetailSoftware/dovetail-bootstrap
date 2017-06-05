using System;

namespace Dovetail.SDK.ModelMap.Legacy.Registration.DSL
{
    public interface IMapExpressionPostAssignWithList<MODEL>
    {
        IMapExpressionPostRoot<MODEL> FromGlobalList(string globalListName);
        IMapExpressionPostRoot<MODEL> FromGlobalList(string globalListName, string selectionFromRelationNamed);
		IMapExpressionPostRoot<MODEL> FromGlobalList(string globalListName, Action<GlobalListConfigurationExpression> config);
        IMapExpressionPostRoot<MODEL> FromUserDefinedList(string userDefinedListName, string[] listValues);
    }
}