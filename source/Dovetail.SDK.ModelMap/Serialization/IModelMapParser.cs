namespace Dovetail.SDK.ModelMap.Serialization
{
    public interface IModelMapParser
    {
        ModelMap Parse(string filePath);
	    void Parse(ModelMap map, string filePath);
    }
}