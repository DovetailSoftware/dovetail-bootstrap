namespace Dovetail.SDK.ModelMap.Serialization.Overrides
{
	public interface IModelMapDiff
	{
		void Diff(ModelMap map, ModelMap overrides, ModelMapDiffOptions options);
	}
}