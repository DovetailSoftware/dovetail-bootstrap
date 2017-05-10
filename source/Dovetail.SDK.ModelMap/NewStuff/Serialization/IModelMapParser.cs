namespace Dovetail.SDK.ModelMap.NewStuff.Serialization
{
    public interface IModelMapParser
    {
        ModelMap Parse(string filePath);
	    void Parse(ModelMap map, string filePath);
    }
}