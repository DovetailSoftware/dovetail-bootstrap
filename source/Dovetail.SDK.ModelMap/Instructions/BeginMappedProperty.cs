using System;

namespace Dovetail.SDK.ModelMap.Instructions
{
    public class BeginMappedProperty : IModelMapInstruction, IEquatable<BeginMappedProperty>
    {
        public IDynamicValue Key { get; set; }
		public bool AllowEmpty { get; set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }

	    public bool Equals(BeginMappedProperty other)
	    {
		    if (ReferenceEquals(null, other)) return false;
		    if (ReferenceEquals(this, other)) return true;
		    return Key.Equals(other.Key);
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
		    return Key.GetHashCode();
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