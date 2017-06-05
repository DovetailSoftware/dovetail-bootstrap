using System;

namespace Dovetail.SDK.ModelMap.Instructions
{
    public class BeginView : IModelMapInstruction, IQueryContext
    {
        public BeginView(string viewName)
        {
            ViewName = viewName;
        }

        public string ViewName { get; private set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }

	    protected bool Equals(BeginView other)
	    {
		    return string.Equals(ViewName, other.ViewName, StringComparison.OrdinalIgnoreCase);
	    }

	    public override bool Equals(object obj)
	    {
		    if (ReferenceEquals(null, obj)) return false;
		    if (ReferenceEquals(this, obj)) return true;
		    if (obj.GetType() != this.GetType()) return false;
		    return Equals((BeginView) obj);
	    }

	    public override int GetHashCode()
	    {
		    return StringComparer.OrdinalIgnoreCase.GetHashCode(ViewName);
	    }

	    public static bool operator ==(BeginView left, BeginView right)
	    {
		    return Equals(left, right);
	    }

	    public static bool operator !=(BeginView left, BeginView right)
	    {
		    return !Equals(left, right);
	    }
    }
}