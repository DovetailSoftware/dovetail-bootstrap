using System;
using FubuCore;

namespace Dovetail.SDK.History
{
	public class WorkflowObject : IEquatable<WorkflowObject>
	{
		public const string Case = "case";
		public const string Subcase = "subcase";
		public const string Solution = "solution";

		public string Id { get; set; }
		public string Type { get; set; }
		public bool IsChild { get; set; }

		public string Key
		{
			get { return KeyFor(Type); }
		}

		public static string KeyFor(string type)
		{
			return "history:" + type;
		}

		public static WorkflowObject Create(string type, string id)
		{
			return new WorkflowObject { Type = type, Id = id };
		}

		public override string ToString()
		{
			return "{0} {1}".ToFormat(Type, Id ?? "");
		}

		public bool Equals(WorkflowObject other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Id, Id) && Equals(other.Type, Type) && other.IsChild.Equals(IsChild);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(WorkflowObject)) return false;
			return Equals((WorkflowObject)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = Id.GetHashCode();
				result = (result * 397) ^ Type.GetHashCode();
				result = (result * 397) ^ IsChild.GetHashCode();
				return result;
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