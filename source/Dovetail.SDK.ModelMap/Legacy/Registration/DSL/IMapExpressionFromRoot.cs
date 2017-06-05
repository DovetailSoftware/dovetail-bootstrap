namespace Dovetail.SDK.ModelMap.Legacy.Registration.DSL
{
    public interface IMapExpressionFromRoot<MODEL>
    {
        IMapExpressionPostRoot<MODEL> FromTable(string tableName);
        IMapExpressionPostView<MODEL> FromView(string viewName);
    }
}