namespace Dovetail.SDK.ModelMap.Registration.DSL
{
    public interface IMapExpressionFromRoot<MODEL>
    {
        IMapExpressionPostRoot<MODEL> FromTable(string tableName);
        IMapExpressionPostView<MODEL> FromView(string viewName);
    }
}