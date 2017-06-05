using System;

namespace Dovetail.SDK.ModelMap.Legacy.Instructions
{
    public class BeginModelMap
    {
        public Type ModelType { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as BeginModelMap;
            return other != null && Equals(other);
        }

        public bool Equals(BeginModelMap other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.ModelType, ModelType);
        }

        public override int GetHashCode()
        {
            return (ModelType != null ? ModelType.GetHashCode() : 0);
        }
    }
}