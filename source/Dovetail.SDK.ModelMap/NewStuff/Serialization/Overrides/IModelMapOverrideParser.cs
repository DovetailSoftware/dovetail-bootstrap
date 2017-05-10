namespace Dovetail.SDK.ModelMap.NewStuff.Serialization.Overrides
{
	public interface IModelMapOverrideParser
	{
		bool ShouldParse(string filePath);
		bool Matches(ModelMap map, string filePath);
		void Parse(ModelMap map, string filePath);
	}
}