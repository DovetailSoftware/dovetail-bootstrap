using System;

namespace Dovetail.SDK.ModelMap.Transforms
{
	public class IsEqualTransform : IMappingTransform
	{
		public object Execute(TransformContext context)
		{
			var field = context.Arguments.Get<string>("field");
			var value = context.Arguments.Get("value");
			var fieldValue = context.Model[field];

			if (fieldValue == null)
				return value == null;

			if (fieldValue.GetType() != value.GetType())
			{
				value = Convert.ChangeType(value, fieldValue.GetType());
			}

			return Equals(value, fieldValue);
		}
	}
}