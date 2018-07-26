using Dovetail.SDK.History.Instructions;
using Dovetail.SDK.ModelMap.Instructions;
using Dovetail.SDK.ModelMap.Serialization.Overrides;
using FubuCore;

namespace Dovetail.SDK.History.Serialization
{
	public class HistoryMapDiffOptions : ModelMapDiffOptions
	{
		public HistoryMapDiffOptions()
		{
			Offset<BeginModelMap>();

			PropertyContext<BeginActEntry, EndActEntry>();

			Remove<RemoveActEntry>((map, key, startIndex) =>
			{
				return map.FindInstructionSet(startIndex, _ =>
				{
					var begin = _ as BeginActEntry;
					return begin != null && begin.Code.ToString().EqualsIgnoreCase(key);
				}, _ => _ is EndActEntry);
			});
		}
	}
}
