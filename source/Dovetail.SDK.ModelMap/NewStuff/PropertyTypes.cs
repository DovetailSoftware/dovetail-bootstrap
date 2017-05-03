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
                default:
                    throw new ModelMapException("Invalid dataType specified: " + dataType);
            }
        }
    }
}