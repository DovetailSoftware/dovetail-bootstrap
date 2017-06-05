using System.Collections.Generic;
using System.Linq;

namespace Dovetail.SDK.ModelMap.Serialization
{
    public class ObjectBuilderResult
    {
        private readonly IList<ObjectBuilderError> _errors = new List<ObjectBuilderError>();

        public object Result { get; set; }

        public void AddError(string key, string message)
        {
            _errors.Add(new ObjectBuilderError
            {
                Key = key,
                Message = message
            });
        }

        public bool HasErrors()
        {
            return _errors.Any();
        }

        public IEnumerable<ObjectBuilderError> Errors
        {
            get { return _errors; }
        }
    }
}