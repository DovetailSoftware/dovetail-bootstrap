namespace Dovetail.SDK.ModelMap
{
	public interface IModelMapQuery
	{
		ModelMapProperty FindIdentifyingField(string name);
	}

	public class ModelMapQuery : IModelMapQuery
	{
		private readonly ModelInspectorVisitor _visitor;
		private readonly IModelMapRegistry _maps;

		public ModelMapQuery(ModelInspectorVisitor visitor, IModelMapRegistry maps)
		{
			_visitor = visitor;
			_maps = maps;
		}

		public ModelMapProperty FindIdentifyingField(string name)
		{
			var map = _maps.Find(name);
			map.Accept(_visitor);

			return _visitor.Identifier;
		}
	}
}