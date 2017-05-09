using System;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public class PropertyTypes
    {
        public static Type Parse(string dataType)
        {
            switch (dataType.ToLower())
            {
                case "int":
                    return typeof(int);
                case "string":
                    return typeof(string);
                case "datetime":
                    return typeof(DateTime);
				case "decimal":
					return typeof(decimal);
				case "bool":
					return typeof(bool);
				case "double":
					return typeof(double);
				case "float":
					return typeof(float);
				case "short":
					return typeof(short);
				default:
                    throw new ModelMapException("Invalid dataType specified: " + dataType);
            }
        }
    }
}