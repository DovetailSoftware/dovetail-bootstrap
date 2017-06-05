using System;
using FubuCore;

namespace Dovetail.SDK.ModelMap
{
	public class DynamicValue : IDynamicValue, IEquatable<DynamicValue>
	{
		private readonly string _value;

		public DynamicValue(string value)
		{
			_value = value;
		}

		public object Resolve(IServiceLocator services)
		{
			if (services == null)
				return _value;

			var expander = services.GetInstance<IMappingVariableExpander>();
			if (!expander.IsVariable(_value))
				return _value;

			return expander.Expand(_value);
		}

		public override string ToString()
		{
			return "Dynamic evaluation of \"" + _value + "\"";
		}

		public bool Equals(DynamicValue other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(_value, other._value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((DynamicValue) obj);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		public static bool operator ==(DynamicValue left, DynamicValue right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(DynamicValue left, DynamicValue right)
		{
			return !Equals(left, right);
		}
	}
}