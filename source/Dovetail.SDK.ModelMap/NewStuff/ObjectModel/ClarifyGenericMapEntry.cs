using System.Collections.Generic;
using Dovetail.SDK.ModelMap.NewStuff.Transforms;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.ModelMap.NewStuff.ObjectModel
{
    public class ClarifyGenericMapEntry
    {
        private readonly List<FieldMap> _fieldMaps = new List<FieldMap>();
		private readonly List<ConfiguredTransform> _transforms = new List<ConfiguredTransform>();
		private readonly List<ClarifyGenericMapEntry> _childMaps = new List<ClarifyGenericMapEntry>();
		private readonly IList<string> _tags = new List<string>();

		public IEnumerable<string> Tags
		{
			get { return _tags; }
		}

		public FieldMap[] FieldMaps
        {
            get { return _fieldMaps.ToArray(); }
        }

		public ConfiguredTransform[] Transforms
		{
			get { return _transforms.ToArray(); }
		}

		public ClarifyGenericMapEntry[] ChildGenericMaps
        {
            get { return _childMaps.ToArray(); }
        }

        public ClarifyGeneric ClarifyGeneric { get; set; }
        public ModelInformation Model { get; set; }
        public SubRootInformation NewRoot { get; set; }

        public bool IsNewRoot()
        {
            return NewRoot != null;
        }

		public void AddTag(string tag)
		{
			_tags.Add(tag);
		}

		public void AddFieldMap(FieldMap fieldMap)
        {
            _fieldMaps.Add(fieldMap);
        }

		public void AddTransform(ConfiguredTransform transform)
		{
			_transforms.Add(transform);
		}

		public void AddChildGenericMap(ClarifyGenericMapEntry clarifyGenericMap)
        {
            _childMaps.Add(clarifyGenericMap);
        }

        public string GetIdentifierFieldName()
        {
            var identifierField = _fieldMaps.Find(f => f.IsIdentifier);
            if (identifierField == null || identifierField.FieldNames.Length == 0)
            {
                return null;
            }

            return identifierField.FieldNames[0];
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ClarifyGenericMapEntry)) return false;
            return Equals((ClarifyGenericMapEntry)obj);
        }

        public bool Equals(ClarifyGenericMapEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.ClarifyGeneric, ClarifyGeneric) && Equals(other.Model, Model) && Equals(other.NewRoot, NewRoot);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (ClarifyGeneric != null ? ClarifyGeneric.GetHashCode() : 0);
                result = (result * 397) ^ (Model != null ? Model.GetHashCode() : 0);
                result = (result * 397) ^ (NewRoot != null ? NewRoot.GetHashCode() : 0);
                return result;
            }
        }
    }
}