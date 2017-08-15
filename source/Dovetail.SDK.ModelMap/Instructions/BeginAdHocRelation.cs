using FubuCore;

namespace Dovetail.SDK.ModelMap.Instructions
{
    public class BeginAdHocRelation : IModelMapInstruction
    {
	    private string _key;

	    public string Key
	    {
		    get
		    {
			    if (_key.IsNotEmpty())
				    return _key;

			    return "From={0};To={1}&Field={2};".ToFormat(FromTableField.Resolve(null), ToTableName.Resolve(null), ToTableFieldName.Resolve(null));
		    }
			set { _key = value; }
	    }

        public IDynamicValue FromTableField { get; set; }
        public IDynamicValue ToTableName { get; set; }
        public IDynamicValue ToTableFieldName { get; set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }

	    protected bool Equals(BeginAdHocRelation other)
	    {
		    return Key.Equals(other.Key);
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
		    return Key.GetHashCode();
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