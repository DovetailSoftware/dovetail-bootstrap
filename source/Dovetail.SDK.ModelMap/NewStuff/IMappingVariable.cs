using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public interface IMappingVariable
    {
        bool Matches(string key);
        object Expand(string key, IServiceLocator services);
    }

    public abstract class MappingVariable : IMappingVariable
    {
        public abstract string Key { get; }

        public virtual bool Matches(string key)
        {
            return Key.EqualsIgnoreCase(key);
        }

        public abstract object Expand(string key, IServiceLocator services);
    }
}