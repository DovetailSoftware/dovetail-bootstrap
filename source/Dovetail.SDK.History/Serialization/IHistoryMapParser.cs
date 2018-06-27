namespace Dovetail.SDK.History.Serialization
{
	public interface IHistoryMapParser
	{
		ModelMap.ModelMap Parse(string filePath);
		void Parse(ModelMap.ModelMap map, string filePath);
	}
}