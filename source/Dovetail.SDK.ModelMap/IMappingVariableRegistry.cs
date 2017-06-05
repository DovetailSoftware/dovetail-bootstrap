namespace Dovetail.SDK.ModelMap
{
    public interface IMappingVariableRegistry
    {
        IMappingVariable Find(VariableExpansionContext context);
    }
}