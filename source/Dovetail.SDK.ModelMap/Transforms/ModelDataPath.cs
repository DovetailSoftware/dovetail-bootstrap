using System;
using System.Collections.Generic;
using System.Linq;

namespace Dovetail.SDK.ModelMap.Transforms
{
	public class ModelDataPath
	{
		public static readonly string This = "$this";

		private readonly string[] _parts;

		public ModelDataPath(string[] parts)
		{
			_parts = parts;
		}

		public object Get(ModelData data)
		{
			if (_parts.Length == 1 && _parts[0] == This)
				return data;

			var target = data;
			for (var i = 0; i < _parts.Length; ++i)
			{
				var value = target[_parts[i]];
				var modelData = value as ModelData;
				if (modelData != null)
				{
					target = modelData;
					continue;
				}

				return value;
			}

			return null;
		}

		public void Set(ModelData data, object value)
		{
			var target = data;
			_parts.Take(_parts.Length - 1).Each(_ => target = target.Child(_));

			target[_parts.Last()] = value;
		}

		public static ModelDataPath Parse(string input)
		{
			return new ModelDataPath(input.Split(new [] { "." }, StringSplitOptions.RemoveEmptyEntries));
		}
	}
}