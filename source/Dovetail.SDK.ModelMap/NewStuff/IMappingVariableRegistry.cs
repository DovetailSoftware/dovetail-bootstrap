namespace Dovetail.SDK.ModelMap.NewStuff
{
    public interface IMappingVariableRegistry
    {
        IMappingVariable Find(VariableExpansionContext context);
    }
}