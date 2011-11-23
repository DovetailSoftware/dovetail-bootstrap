using System;
using System.Collections.Generic;

namespace Dovetail.SDK.Bootstrap.History
{
	public class WorkflowObject : IEquatable<WorkflowObject>
	{
		public static WorkflowObject Case { get { return Create("case"); } }
		public static WorkflowObject Subcase { get { return Create("subcase"); } }
		public static WorkflowObject Solution { get { return Create("solution"); } }

		public static WorkflowObject Create(string objectName)
		{
			return new WorkflowObject(objectName);
		}

		private readonly string _objectName;

		internal WorkflowObject(string objectName)
		{
			_objectName = objectName.ToLowerInvariant();
		}

		public bool Equals(WorkflowObject other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other._objectName, _objectName);
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
			return _objectName.GetHashCode();
		}

		public static bool operator ==(WorkflowObject left, WorkflowObject right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(WorkflowObject left, WorkflowObject right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return _objectName;
		}
	}

	public class HistoryItem
	{
		public int Id { get; set; }
		public DateTime When { get; set; }
		public string Kind { get; set; }
		public string Detail { get; set; }
		public string Internal { get; set; }
		public HistoryItemEmployee Who { get; set; }
	}

	public class HistoryItemEmployee
	{
		public int Id { get; set; }
		public string Login { get; set; }
		public string Name { get; set; }
	}
	public class HistoryViewModel 
	{
		public string Id { get; set; }
	    public IEnumerable<HistoryItem> HistoryItems { get; set; }
	}
}