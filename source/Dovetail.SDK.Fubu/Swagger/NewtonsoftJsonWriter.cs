using FubuMVC.Core.Runtime;
using Newtonsoft.Json;

namespace Dovetail.SDK.Fubu.Swagger
{
    public class NewtonsoftJsonWriter : IJsonWriter
    {        
        private readonly IOutputWriter _outputWriter;

        public NewtonsoftJsonWriter(IOutputWriter outputWriter)
        {
            _outputWriter = outputWriter;
        }

        public void Write(object output)
        {
            Write(output, MimeType.Json.ToString());
        }

        public void Write(object output, string mimeType)
        {
            var json = JsonConvert.SerializeObject(output, Formatting.None);
            _outputWriter.Write(mimeType, json);
        }
    }
}