using System;

namespace Dovetail.SDK.ModelMap.Legacy.Registration.DSL
{
	public interface IMapExpressionPostAssignBase<MODEL>
	{
		IMapExpressionPostBasedOnField<MODEL> BasedOnField(string fieldName);
		IMapExpressionPostBasedOnFields<MODEL> BasedOnFields(params string[] fieldNames);
		IMapExpressionPostRoot<MODEL> FromField(string fieldName);
		IMapExpressionPostRoot<MODEL> FromFields(params string[] fieldNames);
		IMapExpressionPostRoot<MODEL> FromIdentifyingField(string fieldName);
		IMapExpressionPostRoot<MODEL> FromGlobalList(string globalListName);
		IMapExpressionPostRoot<MODEL> FromGlobalList(string globalListName, string selectionFromRelationNamed);
		IMapExpressionPostRoot<MODEL> FromFunction(Func<object> fieldValueMethod);
	}

	public interface IMapExpressionPostAssign<MODEL> : IMapExpressionPostAssignBase<MODEL>
	{
        IMapExpressionPostDoNotEncode<MODEL> DoNotEncode();
	    IMapExpressionPostBasedOnField<MODEL> BasedOnIdentifyingField(string fieldName);
	}

	public interface IMapExpressionPostDoNotEncode<MODEL> : IMapExpressionPostAssignBase<MODEL>
	{
	}
}
