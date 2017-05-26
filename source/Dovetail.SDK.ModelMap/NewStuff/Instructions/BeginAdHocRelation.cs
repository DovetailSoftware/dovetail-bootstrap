namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class BeginAdHocRelation : IModelMapInstruction
    {
        public IDynamicValue FromTableField { get; set; }
        public IDynamicValue ToTableName { get; set; }
        public IDynamicValue ToTableFieldName { get; set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }

	    protected bool Equals(BeginAdHocRelation other)
	    {
		    return FromTableField.Equals(other.FromTableField) && ToTableName.Equals(other.ToTableName) && ToTableFieldName.Equals(other.ToTableFieldName);
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
			    var hashCode = FromTableField.GetHashCode();
			    hashCode = (hashCode*397) ^ ToTableName.GetHashCode();
			    hashCode = (hashCode*397) ^ ToTableFieldName.GetHashCode();
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