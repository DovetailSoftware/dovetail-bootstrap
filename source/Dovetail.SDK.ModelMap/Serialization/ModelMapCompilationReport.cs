using System.Collections.Generic;
using Dovetail.SDK.Bootstrap;

namespace Dovetail.SDK.ModelMap.Serialization
{
    public class ModelMapCompilationReport
    {
        private readonly IList<string> _errors = new List<string>();

        public IEnumerable<string> Errors { get { return _errors; } }

        public void AddError(string error)
        {
            _errors.Add(error);
        }

        public void ReportTo(ILogger logger)
        {
            _errors.Each(_ => logger.LogError(_));
        }
    }
}