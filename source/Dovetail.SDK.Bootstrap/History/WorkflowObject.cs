using System;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History
{
    public class WorkflowObject : IEquatable<WorkflowObject>
    {
        public const string Case = "case";
        public const string Subcase = "subcase";
        public const string Solution = "solution";

        public string Id { get; set; }
        public string Type { get { return _objectName; } }
        public bool IsChild { get; set; }

        public static WorkflowObject Create(string objectName, string id)
        {
            return new WorkflowObject(objectName) {Id = id};
        }

        private readonly string _objectName;

        internal WorkflowObject(string objectName)
        {
            _objectName = objectName.ToLowerInvariant();
        }

        public override string ToString()
        {
            return "{0} {1}".ToFormat(_objectName, Id ?? "");
        }

        // little weird that the ID is not considered by equality. 
        // For the caching scenario it is not considered part of the object's identity.
        public bool Equals(WorkflowObject other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.IsChild.Equals(IsChild) && Equals(other._objectName, _objectName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (WorkflowObject)) return false;
            return Equals((WorkflowObject) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (IsChild.GetHashCode()*397) ^ _objectName.GetHashCode();
            }
        }

        public static bool operator ==(WorkflowObject left, WorkflowObject right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WorkflowObject left, WorkflowObject right)
        {
            return !Equals(left, right);
        }
    }
}