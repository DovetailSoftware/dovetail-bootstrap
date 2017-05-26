using System;

namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class BeginRelation : IModelMapInstruction, IEquatable<BeginRelation>
    {
        public IDynamicValue RelationName { get; set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }

	    public bool Equals(BeginRelation other)
	    {
		    if (ReferenceEquals(null, other)) return false;
		    if (ReferenceEquals(this, other)) return true;
		    return RelationName.Equals(other.RelationName);
	    }

	    public override bool Equals(object obj)
	    {
		    if (ReferenceEquals(null, obj)) return false;
		    if (ReferenceEquals(this, obj)) return true;
		    if (obj.GetType() != this.GetType()) return false;
		    return Equals((BeginRelation) obj);
	    }

	    public override int GetHashCode()
	    {
		    return RelationName.GetHashCode();
	    }

	    public static bool operator ==(BeginRelation left, BeginRelation right)
	    {
		    return Equals(left, right);
	    }

	    public static bool operator !=(BeginRelation left, BeginRelation right)
	    {
		    return !Equals(left, right);
	    }
    }
}