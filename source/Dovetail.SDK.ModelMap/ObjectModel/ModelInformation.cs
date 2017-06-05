using System.Collections.Generic;

namespace Dovetail.SDK.ModelMap.ObjectModel
{
    public class ModelInformation
    {
		private readonly List<FieldMap> _fieldMaps = new List<FieldMap>();

		public string ModelName { get; set; }
        public string ParentProperty { get; set; }
        public bool IsCollection { get; set; }

		public FieldMap[] FieldMaps
		{
			get { return _fieldMaps.ToArray(); }
		}

		public void AddFieldMap(FieldMap fieldMap)
		{
			_fieldMaps.Add(fieldMap);
		}

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
            return Equals(other.ModelName, ModelName) && Equals(other.ParentProperty, ParentProperty);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ModelName != null ? ModelName.GetHashCode() : 0) * 397) ^ (ParentProperty != null ? ParentProperty.GetHashCode() : 0);
            }
        }
    }
}