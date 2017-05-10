namespace Dovetail.SDK.ModelMap.NewStuff.Serialization.Overrides
{
	public interface IModelMapDiff
	{
		void Diff(ModelMap map, ModelMap overrides);
	}
}