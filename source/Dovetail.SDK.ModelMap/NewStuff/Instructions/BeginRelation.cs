using System;

namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class BeginRelation : IModelMapInstruction
    {
        public string RelationName { get; set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }

	    protected bool Equals(BeginRelation other)
	    {
		    return string.Equals(RelationName, other.RelationName, StringComparison.OrdinalIgnoreCase);
	    }

	    public override bool Equals(object obj)
	    {
		    if (ReferenceEquals(null, obj)) return false;
		    if (ReferenceEquals(this, obj)) return true;
		    if (obj.GetType() != GetType()) return false;
		    return Equals((BeginRelation) obj);
	    }

	    public override int GetHashCode()
	    {
		    return StringComparer.OrdinalIgnoreCase.GetHashCode(RelationName);
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