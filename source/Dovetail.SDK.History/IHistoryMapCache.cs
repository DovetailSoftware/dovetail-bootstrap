using System;
using System.Collections.Generic;
using System.IO;

namespace Dovetail.SDK.History
{
	public interface IHistoryMapCache
	{
		IEnumerable<ModelMap.ModelMap> Maps();
		IEnumerable<ModelMap.ModelMap> Partials();
		void Clear();
	}

	public class HistoryMapSettings
	{
		public HistoryMapSettings()
		{
			Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "History");
		}

		public string Directory { get; set; }
		public bool EnableCache { get; set; }
	}
}