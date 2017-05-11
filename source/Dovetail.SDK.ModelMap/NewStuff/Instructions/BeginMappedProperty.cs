using System;

namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class BeginMappedProperty : IModelMapInstruction
    {
        public string Key { get; set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }

	    protected bool Equals(BeginMappedProperty other)
	    {
		    return string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);
	    }

	    public override bool Equals(object obj)
	    {
		    if (ReferenceEquals(null, obj)) return false;
		    if (ReferenceEquals(this, obj)) return true;
		    if (obj.GetType() != this.GetType()) return false;
		    return Equals((BeginMappedProperty) obj);
	    }

	    public override int GetHashCode()
	    {
		    return StringComparer.OrdinalIgnoreCase.GetHashCode(Key);
	    }

	    public static bool operator ==(BeginMappedProperty left, BeginMappedProperty right)
	    {
		    return Equals(left, right);
	    }

	    public static bool operator !=(BeginMappedProperty left, BeginMappedProperty right)
	    {
		    return !Equals(left, right);
	    }
    }
}