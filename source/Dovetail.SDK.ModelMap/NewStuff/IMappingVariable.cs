namespace Dovetail.SDK.ModelMap.NewStuff
{
    public interface IMappingVariable
    {
        bool Matches(VariableExpansionContext context);
        object Expand(VariableExpansionContext context);
    }

    public abstract class MappingVariable : IMappingVariable
    {
        public abstract string Key { get; }

        public virtual bool Matches(VariableExpansionContext context)
        {
	        return context.Matches(Key);
        }

        public abstract object Expand(VariableExpansionContext context);
    }
}