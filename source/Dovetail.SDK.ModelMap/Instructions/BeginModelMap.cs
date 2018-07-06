using System;

namespace Dovetail.SDK.ModelMap.Instructions
{
    public class BeginModelMap : IModelMapInstruction, IEquatable<BeginModelMap>
    {
	    public BeginModelMap(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }

		public bool Equals(BeginModelMap other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((BeginModelMap)obj);
		}

		public override int GetHashCode()
		{
			return StringComparer.InvariantCultureIgnoreCase.GetHashCode(Name);
		}

		public static bool operator ==(BeginModelMap left, BeginModelMap right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(BeginModelMap left, BeginModelMap right)
		{
			return !Equals(left, right);
		}
	}
}