using System;
using System.Reflection;

namespace Dovetail.SDK.ModelMap.Legacy.ObjectModel
{
    public class ModelInformation
    {
        public Type ModelType { get; set; }
        public PropertyInfo ParentProperty { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as ModelInformation;
            return other != null && Equals(other);
        }

        public bool Equals(ModelInformation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.ModelType, ModelType) && Equals(other.ParentProperty, ParentProperty);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ModelType != null ? ModelType.GetHashCode() : 0)*397) ^ (ParentProperty != null ? ParentProperty.GetHashCode() : 0);
            }
        }
    }
}