namespace Dovetail.SDK.ModelMap.NewStuff
{
    public interface IMappingVariableExpander
    {
        bool IsVariable(string value);
        object Expand(string value);
    }
}