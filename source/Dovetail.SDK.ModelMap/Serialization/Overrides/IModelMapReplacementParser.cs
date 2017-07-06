namespace Dovetail.SDK.ModelMap.Serialization.Overrides
{
	public interface IModelMapReplacementParser
	{
		bool ShouldParse(string filePath);
		bool Matches(ModelMap map, string filePath);
		void Parse(ModelMap map, string filePath);
	}
}