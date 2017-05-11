using System;

namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class BeginAdHocRelation : IModelMapInstruction
    {
        public string FromTableField { get; set; }
        public string ToTableName { get; set; }
        public string ToTableFieldName { get; set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }

	    protected bool Equals(BeginAdHocRelation other)
	    {
		    return string.Equals(FromTableField, other.FromTableField, StringComparison.OrdinalIgnoreCase) && string.Equals(ToTableName, other.ToTableName, StringComparison.OrdinalIgnoreCase) && string.Equals(ToTableFieldName, other.ToTableFieldName, StringComparison.OrdinalIgnoreCase);
	    }

	    public override bool Equals(object obj)
	    {
		    if (ReferenceEquals(null, obj)) return false;
		    if (ReferenceEquals(this, obj)) return true;
		    if (obj.GetType() != this.GetType()) return false;
		    return Equals((BeginAdHocRelation) obj);
	    }

	    public override int GetHashCode()
	    {
		    unchecked
		    {
			    var hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(FromTableField);
			    hashCode = (hashCode*397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(ToTableName);
			    hashCode = (hashCode*397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(ToTableFieldName);
			    return hashCode;
		    }
	    }

	    public static bool operator ==(BeginAdHocRelation left, BeginAdHocRelation right)
	    {
		    return Equals(left, right);
	    }

	    public static bool operator !=(BeginAdHocRelation left, BeginAdHocRelation right)
	    {
		    return !Equals(left, right);
	    }
    }
}