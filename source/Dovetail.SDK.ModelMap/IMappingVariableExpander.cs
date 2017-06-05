namespace Dovetail.SDK.ModelMap
{
    public interface IMappingVariableExpander
    {
        bool IsVariable(string value);
        object Expand(string value);

	    void PushContext(VariableExpanderContext context);
	    void PopContext();
    }
}