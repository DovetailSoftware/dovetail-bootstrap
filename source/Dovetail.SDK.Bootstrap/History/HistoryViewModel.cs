using System;
using System.Collections.Generic;
using FubuCore.Util;

namespace Dovetail.SDK.Bootstrap.History
{
	public class HistoryItem
	{
		private readonly Cache<string, object> _data;

		public HistoryItem()
		{
			_data = new Cache<string, object>(_ => null);
		}

        public bool IsVerbose { get; set; }

		public bool IsCancelled { get; set; }

		/// <summary>
		/// Database Identifier of the activity entry.
		/// </summary>
		public int DatabaseIdentifier { get; set; }

		/// <summary>
		/// Object Id of the item.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Type (case, subcase, solution) of the item.
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// When the activity entry was logged.
		/// </summary>
		public DateTime When { get; set; }

		/// <summary>
		/// Placeholder for describing the history when field. If this is desired it should be populated downstream from Bootstrap. This field is NOT populated by Dovetail Bootstrap.
		/// </summary>
		public string WhenDescribed { get; set; }

		/// <summary>
		/// Title of the item. Likely populated fromt he DSL's DisplayName.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Public details of the item.
		/// </summary>
		public string Detail { get; set; }

		/// <summary>
		/// Internal item details. Not to be displayed to the end user.
		/// </summary>
		///
		public string Internal { get; set; }

		/// <summary>
		/// User account which logged the activity
		/// </summary>
		public HistoryItemEmployee Who { get; set; }

		public IDictionary<string, object> Data { get { return _data.ToDictionary(); } }

		public bool Has(string key)
		{
			return Data.ContainsKey(key);
		}

		public object this[string key]
		{
			get { return _data[key]; }
			set { _data[key] = value; }
		}
	}

	public class HistoryItemContact
	{
		public int Id { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }
		public string Firstname { get; set; }
		public string Lastname { get; set; }
		public dynamic Custom { get; set; }
	}

	public class HistoryItemEmployee
	{
		public HistoryItemContact PerformedByContact { get; set; }
		public int Id { get; set; }
		public string Login { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }
		public string Firstname { get; set; }
		public string Lastname { get; set; }
		public dynamic Custom { get; set; }
	}

	public class HistoryViewModel
	{
		public DateTime Since { get; set; }
		public bool AllActivitiesShown { get; set; }
		public WorkflowObject WorkflowObject { get; set; }
		public HistoryItem[] HistoryItems { get; set; }
	}
}
