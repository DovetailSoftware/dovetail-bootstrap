using System;
using FubuCore;

namespace Dovetail.SDK.ModelMap.Instructions
{
    public class BeginRelation : IModelMapInstruction, IEquatable<BeginRelation>
    {
		private string _key;

		public string Key
		{
			get
			{
				if (_key.IsNotEmpty())
					return _key;

				return RelationName.Resolve(null).ToString();
			}
			set { _key = value; }
		}

		public IDynamicValue RelationName { get; set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }

	    public bool Equals(BeginRelation other)
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
		    return Equals((BeginRelation) obj);
	    }

	    public override int GetHashCode()
	    {
		    return Key.GetHashCode();
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