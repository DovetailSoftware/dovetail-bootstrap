using FubuCore;

namespace Dovetail.SDK.Bootstrap.History
{
    public class WorkflowObject 
    {
        public const string Case = "case";
        public const string Subcase = "subcase";
        public const string Solution = "solution";

        public string Id { get; set; }
        public string Type { get; set; }
        public bool IsChild { get; set; }

        public static WorkflowObject Create(string type, string id)
        {
            return new WorkflowObject {Type = type, Id = id};
        }

        public override string ToString()
        {
            return "{0} {1}".ToFormat(Type, Id ?? "");
        }
    }
}