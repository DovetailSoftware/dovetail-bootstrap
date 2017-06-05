using System;

namespace Dovetail.SDK.ModelMap.Instructions
{
    public class BeginTable : IModelMapInstruction, IQueryContext
    {
        public BeginTable(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; private set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }

	    protected bool Equals(BeginTable other)
	    {
		    return string.Equals(TableName, other.TableName, StringComparison.OrdinalIgnoreCase);
	    }

	    public override bool Equals(object obj)
	    {
		    if (ReferenceEquals(null, obj)) return false;
		    if (ReferenceEquals(this, obj)) return true;
		    if (obj.GetType() != this.GetType()) return false;
		    return Equals((BeginTable) obj);
	    }

	    public override int GetHashCode()
	    {
		    return StringComparer.OrdinalIgnoreCase.GetHashCode(TableName);
	    }

	    public static bool operator ==(BeginTable left, BeginTable right)
	    {
		    return Equals(left, right);
	    }

	    public static bool operator !=(BeginTable left, BeginTable right)
	    {
		    return !Equals(left, right);
	    }
    }
}