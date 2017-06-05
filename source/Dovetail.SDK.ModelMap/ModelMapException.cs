using System;

namespace Dovetail.SDK.ModelMap
{
    public class ModelMapException : Exception
    {
        public ModelMapException(string message, Exception innerException) : base(message, innerException) { }

        public ModelMapException(string message) : base(message) { }
    }
}