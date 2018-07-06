namespace Dovetail.SDK.History.Serialization
{
	public interface IHistoryMapOverrideParser
	{
		bool ShouldParse(string filePath);
		bool Matches(ModelMap.ModelMap map, string filePath);
		void Parse(ModelMap.ModelMap map, string filePath);
	}
}