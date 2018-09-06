using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FubuCore;

namespace Dovetail.SDK.History.Serialization
{
	public class HistoryPrivilegePolicyCache : IHistoryPrivilegePolicyCache
	{
		private readonly HistorySettings _settings;
		private readonly Lazy<Dictionary<Tuple<int, string, string>, PrivilegePolicy>> _activities;

		public HistoryPrivilegePolicyCache(HistorySettings settings)
		{
			_settings = settings;
			_activities = new Lazy<Dictionary<Tuple<int, string, string>, PrivilegePolicy>>(findActivities);
		}

		public IEnumerable<PrivilegePolicy> Find(string objectType, int actCode)
		{
			return GetAll().Where(_ => _.ActCode == actCode && _.ObjectType.EqualsIgnoreCase(objectType)).ToList();
		}

		public IEnumerable<PrivilegePolicy> GetAll()
		{
			return _activities.Value.Values.ToArray();
		}

		private Dictionary<Tuple<int, string, string>, PrivilegePolicy> findActivities()
		{
			var activities = new Dictionary<Tuple<int, string, string>, PrivilegePolicy>();

			var files = new FileSystem();
			if (!files.FileExists(_settings.HistoryConfigPath)) return activities;

			var doc = new XmlDocument();
			doc.Load(_settings.HistoryConfigPath);

			var node = doc.SelectSingleNode("history");
			if (node == null) return activities;

			var prop = node.SelectNodes("addPrivilegePolicy");
			if (prop == null) return activities;

			foreach (XmlNode item in prop)
			{
				if (item == null) continue;
				if (item.Attributes == null) continue;

				if (item.Attributes.Count != 3) continue;

				var actCodeValue = item.Attributes.GetNamedItem("actCode").Value;
				var privilege = item.Attributes.GetNamedItem("privilege").Value;
				var objectType = item.Attributes.GetNamedItem("objectType").Value;

				int actCode;

				if (!int.TryParse(actCodeValue, out actCode)) continue;

				var activity = new Tuple<int, string, string>(actCode, objectType, privilege);
				activities.Add(activity, new PrivilegePolicy { ActCode = actCode, ObjectType = objectType, Privilege = privilege });
			}

			return activities;
		}
	}
}
