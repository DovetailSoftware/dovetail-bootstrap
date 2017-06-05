namespace Dovetail.SDK.ModelMap
{
    public interface IModelMapRegistry
    {
        ModelMap Find(string name);
	    ModelMap FindPartial(string name);
    }
}