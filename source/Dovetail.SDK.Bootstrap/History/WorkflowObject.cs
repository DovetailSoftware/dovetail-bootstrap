using FubuCore;

namespace Dovetail.SDK.Bootstrap.History
{
    public class WorkflowObject
    {
        public const string Case = "case";
        public const string Subcase = "subcase";
        public const string Solution = "solution";

        public string Id { get; set; }
        public string Type { get { return _objectName; } }

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
    }
}