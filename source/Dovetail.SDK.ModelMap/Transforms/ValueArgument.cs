namespace Dovetail.SDK.ModelMap.Transforms
{
	public class ValueArgument : ITransformArgument
	{
		private readonly string _name;
		private readonly object _value;

		public ValueArgument(string name, object value)
		{
			_name = name;
			_value = value;
		}

		public string Name
		{
			get { return _name; }
		}

		public object Resolve(ModelData data)
		{
			return _value;
		}
	}
}