﻿using System;
using System.Linq;
using System.Xml.Linq;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public class XElementSerializer : IXElementSerializer
	{
		public object ValueFor(XAttribute attribute)
		{
			return attribute.Value;
		}

		public T Deserialize<T>(XElement element) where T : class, new()
		{
			var target = new T();
			var properties = typeof(T).GetProperties();

			foreach (var attribute in element.Attributes())
			{
				var prop = properties.SingleOrDefault(_ => _.Name.EqualsIgnoreCase(attribute.Name.ToString()));
				if (prop != null)
				{
					object value = ValueFor(attribute);
					if (value == null) continue;

					if (value.GetType() != prop.PropertyType)
					{
						var type = prop.PropertyType;
						if (type.Closes(typeof(Nullable<>)))
							type = type.GetGenericArguments()[0];

						value = Convert.ChangeType(value, type);
					}

					prop.SetValue(target, value);
				}
			}

			return target;
		}
	}
}
