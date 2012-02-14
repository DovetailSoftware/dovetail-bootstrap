using System;

namespace Dovetail.SDK.Fubu.Swagger
{
    public class AllowableValuesAttribute : Attribute
    {
        public string[] AllowableValues { get; private set; }

        public AllowableValuesAttribute(params string[] allowableValues)
        {
            AllowableValues = allowableValues;
        }
    }
}