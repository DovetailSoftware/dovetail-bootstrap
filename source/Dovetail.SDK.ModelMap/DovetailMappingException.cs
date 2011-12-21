using System;
using FubuCore;

namespace Dovetail.SDK.ModelMap
{
    public class DovetailMappingException : Exception
    {
        private readonly int _errorCode;
        private readonly string _message;

        public DovetailMappingException(int errorCode, string message)
            : base(message)
        {
            _errorCode = errorCode;
            _message = message;
        }

        private DovetailMappingException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            _errorCode = errorCode;
            _message = message;
        }

        public DovetailMappingException(int errorCode, Exception inner, string template, params string[] substitutions)
            : this(errorCode, template.ToFormat(substitutions), inner)
        {
        }

        public DovetailMappingException(int errorCode, string template, params string[] substitutions)
            : this(errorCode, template.ToFormat(substitutions))
        {
        }

        public override string Message { get { return "Mapping Error {0}:  \n{1}".ToFormat(ErrorCode, _message); } }
        
        public int ErrorCode { get { return _errorCode; } }
    }
}